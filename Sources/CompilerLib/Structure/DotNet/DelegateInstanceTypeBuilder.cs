using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Ast2RLCompiler.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Instruction = Dot42.CompilerLib.RL.Instruction;
using MethodBody = Dot42.CompilerLib.RL.MethodBody;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for existing uses of .NET delegate types.
    /// </summary>
    internal sealed class DelegateInstanceTypeBuilder 
    {
        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        internal static DelegateInstanceType Create(
            ISourceLocation sequencePoint,
            AssemblyCompiler compiler,
            DexTargetPackage targetPackage,
            ClassDefinition delegateClass,
            XMethodDefinition invokeMethod,
            Prototype invokePrototype,
            XMethodDefinition equalsMethod,
            Prototype equalsPrototype,
            XMethodDefinition calledMethod)
        {
            // Prepare called method
            var target = targetPackage.DexFile;
            var owner = target.GetClass(calledMethod.DeclaringType.GetClassReference(targetPackage).Fullname) ??
                targetPackage.GetOrCreateGeneratedCodeClass();
            var calledMethodPrototype = PrototypeBuilder.BuildPrototype(compiler, targetPackage, owner, calledMethod);
            var calledMethodRef = calledMethod.GetReference(targetPackage);

            if (calledMethod.DeclaringType.HasDexImportAttribute())
            {
                // Delegate method is a Dex import method
            }
            else
            {
                // Delegate method is a .NET method
                var calledDexMethod = owner.Methods.Single(x => (x.Name == calledMethodRef.Name) && (x.Prototype.Equals(calledMethodRef.Prototype)));
                if (calledDexMethod.IsPrivate)
                {
                    calledDexMethod.IsPrivate = false;
                    calledDexMethod.IsProtected = true;
                }
            }

            var @class = new ClassDefinition();
            @class.Name = CreateInstanceTypeName(owner);
            @class.Namespace = owner.Namespace;
            @class.AccessFlags = AccessFlags.Public | AccessFlags.Final;
            owner.InnerClasses.Add(@class);

            // Set super class
            @class.SuperClass = delegateClass;

            // Implement delegate interface
            //@class.Interfaces.Add(delegateInterface);

            // Get type of instance 
            XTypeDefinition instanceType = calledMethod.DeclaringType;
            TypeReference instanceTypeRef = instanceType.GetReference(targetPackage);

            // Add instance field
            FieldDefinition instanceField = null;
            if (!calledMethod.IsStatic)
            {
                instanceField = new FieldDefinition();
                instanceField.Name = "instance";
                instanceField.Owner = @class;
                instanceField.Type = instanceTypeRef;
                instanceField.AccessFlags = AccessFlags.Private | AccessFlags.Final;
                @class.Fields.Add(instanceField);
            }

            // Add ctor
            var ctor = new Dot42.DexLib.MethodDefinition();
            ctor.Owner = @class;
            ctor.Name = "<init>";
            ctor.AccessFlags = AccessFlags.Public | AccessFlags.Constructor;
            ctor.Prototype = new Prototype(PrimitiveType.Void);
            if (!calledMethod.IsStatic)
            {
                ctor.Prototype.Parameters.Add(new Parameter(instanceTypeRef, "this"));
            }
            @class.Methods.Add(ctor);
            // Create ctor body
            var ctorBody = CreateCtorBody(calledMethod, instanceField, delegateClass);
            targetPackage.Record(new CompiledMethod() { DexMethod = ctor, RLBody = ctorBody });

            // Add Invoke method
            var invoke = new Dot42.DexLib.MethodDefinition(@class, "Invoke", invokePrototype) { AccessFlags = AccessFlags.Public };
            @class.Methods.Add(invoke);
            // Create body
            var invokeBody = CreateInvokeBody(sequencePoint, compiler, targetPackage, calledMethod, invokeMethod, invokePrototype, calledMethodPrototype, instanceField, delegateClass);
            targetPackage.Record(new CompiledMethod() { DexMethod = invoke, RLBody = invokeBody });

            // Add Equals method
            if (null != equalsMethod)
            {
                var equals = new Dot42.DexLib.MethodDefinition(@class, "equals", equalsPrototype) { AccessFlags = AccessFlags.Public };
                @class.Methods.Add(equals);
                // Create body
                if (!calledMethod.IsStatic)
                {
                    var equalsBody = CreateEqualsBody(sequencePoint, compiler, targetPackage, equalsMethod, equalsPrototype, instanceField, @class);
                    targetPackage.Record(new CompiledMethod() { DexMethod = equals, RLBody = equalsBody });
                }
                else
                {
                    var equalsBody = CreateEqualsBody();
                    targetPackage.Record(new CompiledMethod() { DexMethod = equals, RLBody = equalsBody });
                }
            }

            return new DelegateInstanceType(calledMethod, @class, ctor);
        }

        /// <summary>
        /// Create a typename for the delegate instance type
        /// </summary>
        private static string CreateInstanceTypeName(ClassDefinition owner)
        {
            var i = 1;
            while (true)
            {
                var name = owner.Name + "$d" + i++;
                if (owner.InnerClasses.Any(x => x.Name == name))
                    continue;
                return name;
            }
        }

        /// <summary>
        /// Create the body of the ctor.
        /// </summary>
        private static MethodBody CreateCtorBody(XMethodDefinition calledMethod, FieldDefinition instanceField, ClassReference baseClass)
        {
            var body = new MethodBody(null);
            // Create code
            var ins = body.Instructions;
            var rthis = body.AllocateRegister(RCategory.Argument, RType.Object);
            // Call base ctor
            var baseCtorRef = new MethodReference(baseClass, "<init>", new Prototype(PrimitiveType.Void));
            ins.Add(new Instruction(RCode.Invoke_direct, rthis) { Operand = baseCtorRef });
            if (!calledMethod.IsStatic)
            {
                // load instance into field
                var rvalue = body.AllocateRegister(RCategory.Argument, RType.Object);
                ins.Add(new Instruction(RCode.Iput_object, rvalue, rthis) {Operand = instanceField});
            }
            ins.Add(new Instruction(RCode.Return_void));
            return body;
        }

        /// <summary>
        /// Create the body of the invoke method.
        /// </summary>
        private static MethodBody CreateInvokeBody(ISourceLocation sequencePoint, AssemblyCompiler compiler, DexTargetPackage targetPackage, XMethodDefinition calledMethod, XMethodDefinition invokeMethod, Prototype invokePrototype, Prototype calledMethodPrototype, FieldDefinition instanceField, ClassReference delegateClass)
        {
            var body = new MethodBody(null);
            var rthis = body.AllocateRegister(RCategory.Argument, RType.Object);
            foreach (var p in invokePrototype.Parameters)
            {
                if (p.Type.IsWide())
                {
                    body.AllocateWideRegister(RCategory.Argument);
                }
                else
                {
                    var type = (p.Type is PrimitiveType) ? RType.Value : RType.Object;
                    body.AllocateRegister(RCategory.Argument, type);
                }
            }
            var incomingMethodArgs = body.Registers.ToArray();

            // Create code
            var ins = body.Instructions;
            Register instance = null;
            if (!calledMethod.IsStatic)
            {
                // load instance
                instance = body.AllocateRegister(RCategory.Temp, RType.Object);
                ins.Add(new Instruction(RCode.Iget_object, instance, rthis) { Operand = instanceField });
            }
            // Invoke
            var calledMethodRef = calledMethod.GetReference(targetPackage);
            var inputArgs = calledMethod.IsStatic ? incomingMethodArgs.Skip(1).ToArray() : incomingMethodArgs;
            
            // Cast arguments (if needed)
            var outputArgs = new List<Register>();
            if (!calledMethod.IsStatic)
            {
                outputArgs.Add(instance);
            }
            var parameterIndex = 0;
            for (var i = calledMethod.IsStatic ? 0 : 1; i < inputArgs.Length; )
            {
                var invokeType = invokePrototype.Parameters[parameterIndex].Type;
                var inputIsWide = invokeType.IsWide();
                var calledType = calledMethodPrototype.Parameters[parameterIndex].Type;
                if (!invokeType.Equals(calledType))
                {
                    // Add cast / unbox
                    var source = inputIsWide
                                     ? new RegisterSpec(inputArgs[i], inputArgs[i + 1], invokeType)
                                     : new RegisterSpec(inputArgs[i], null, invokeType);
                    var tmp = ins.Unbox(sequencePoint, source, calledMethod.Parameters[parameterIndex].ParameterType, compiler, targetPackage, body);
                    outputArgs.Add(tmp.Result.Register);
                    if (calledType.IsWide())
                    {
                        outputArgs.Add(tmp.Result.Register2);
                    }
                }
                else
                {
                    outputArgs.Add(inputArgs[i]);
                    if (calledType.IsWide())
                    {
                        outputArgs.Add(inputArgs[i + 1]);
                    }
                }
                i += inputIsWide ? 2 : 1;
                parameterIndex++;
            }

            // Actual call
            ins.Add(new Instruction(calledMethod.Invoke(calledMethod, null), calledMethodRef, outputArgs.ToArray()));

            // Collect return value
            var invokeReturnType = invokePrototype.ReturnType;
            var calledReturnType = calledMethodPrototype.ReturnType;
            var needsBoxing = !invokeReturnType.Equals(calledReturnType);
            Instruction returnInstruction;

            if (calledReturnType.IsWide())
            {
                var r = body.AllocateWideRegister(RCategory.Temp);
                ins.Add(new Instruction(RCode.Move_result_wide, r.Item1));
                if (needsBoxing)
                {
                    // Box
                    var source = new RegisterSpec(r.Item1, r.Item2, calledReturnType);
                    var tmp = ins.Box(sequencePoint, source, calledMethod.ReturnType, targetPackage, body);
                    returnInstruction = new Instruction(RCode.Return_object, tmp.Result.Register);
                }
                else
                {
                    // Return wide
                    returnInstruction = new Instruction(RCode.Return_wide, r.Item1);
                }
            }
            else if (calledMethod.ReturnType.IsVoid())
            {
                // Void return
                returnInstruction = new Instruction(RCode.Return_void);
            }
            else if (calledReturnType is PrimitiveType)
            {
                // Single register return
                var r = body.AllocateRegister(RCategory.Temp, RType.Value);
                ins.Add(new Instruction(RCode.Move_result, r));
                if (needsBoxing)
                {
                    // Box
                    var source = new RegisterSpec(r, null, invokeReturnType);
                    var tmp = ins.Box(sequencePoint, source, calledMethod.ReturnType, targetPackage, body);
                    returnInstruction = new Instruction(RCode.Return_object, tmp.Result.Register);
                }
                else
                {
                    // Return 
                    returnInstruction = new Instruction(RCode.Return, r);
                }
            }
            else
            {
                var r = body.AllocateRegister(RCategory.Temp, RType.Object);
                ins.Add(new Instruction(RCode.Move_result_object, r));
                if (needsBoxing)
                {
                    // Box
                    var source = new RegisterSpec(r, null, invokeReturnType);
                    var tmp = ins.Box(sequencePoint, source, invokeMethod.ReturnType, targetPackage, body);
                    returnInstruction = new Instruction(RCode.Return_object, tmp.Result.Register);
                }
                else
                {
                    // Return 
                    returnInstruction = new Instruction(RCode.Return_object, r);
                }
            }

            // Call next delegate (if any)
            var next = body.AllocateRegister(RCategory.Temp, RType.Object);
            var multicastDelegateType = new ClassReference(targetPackage.NameConverter.GetConvertedFullName("System.MulticastDelegate"));
            var nextReference = new FieldReference(multicastDelegateType, "next", multicastDelegateType);
            ins.Add(new Instruction(RCode.Iget_object, nextReference, new[] { next, rthis })); // load this.next
            var afterCallNext = new Instruction(RCode.Nop);
            ins.Add(new Instruction(RCode.If_eqz, afterCallNext, new[] { next })); // if next == null, continue
            ins.Add(new Instruction(RCode.Check_cast, delegateClass, new[] { next }));
            var nextInvokeMethod = new MethodReference(delegateClass, "Invoke", invokePrototype);
            var nextInvokeArgs = new[] { next }.Concat(incomingMethodArgs.Skip(1)).ToArray();
            ins.Add(new Instruction(RCode.Invoke_virtual, nextInvokeMethod, nextInvokeArgs));
            ins.Add(afterCallNext);

            // Add return instructions
            ins.Add(returnInstruction);

            return body;
        }

        /// <summary>
        /// Create the body of the equals method.
        /// </summary>
        private static MethodBody CreateEqualsBody(ISourceLocation sequencePoint, AssemblyCompiler compiler, DexTargetPackage targetPackage, XMethodDefinition equalsMethod, Prototype equalsPrototype, FieldDefinition instanceField, ClassReference delegateClass)
        {
            MethodBody body = new MethodBody(null);

            // This pointer and method argument.
            Register rthis = body.AllocateRegister(RCategory.Argument, RType.Object);
            Register rother = body.AllocateRegister(RCategory.Argument, RType.Object);

            // Create code.
            var ins = body.Instructions;

            // Temporary parameter result.
            Register result = body.AllocateRegister(RCategory.Temp, RType.Value);

            // Prepare the return instruction.
            Instruction returnInstruction = new Instruction(RCode.Return, result);

            // Check if other object can be casted.
            ins.Add(new Instruction(RCode.Instance_of, delegateClass, new[] { result, rother }));
            ins.Add(new Instruction(RCode.If_eqz, returnInstruction, new[] { result })); // compare instance members

            // Cast of the other object.
            ins.Add(new Instruction(RCode.Check_cast, delegateClass, new[] { rother }));

            // Get instance fields of this and other.
            var thisInstance = body.AllocateRegister(RCategory.Temp, RType.Object);
            var otherInstance = body.AllocateRegister(RCategory.Temp, RType.Object);

            // Load the instance fields.
            ins.Add(new Instruction(RCode.Iget_object, thisInstance, rthis) { Operand = instanceField });
            ins.Add(new Instruction(RCode.Iget_object, otherInstance, rother) { Operand = instanceField });

            // Compare the instance fields.
            ins.Add(new Instruction(RCode.If_eq, returnInstruction, new[] { thisInstance, otherInstance })); // compare instance members

            // Set result to false if not equal.
            ins.Add(new Instruction(RCode.Const, 0, new[] { result }));

            // Add return instructions
            ins.Add(returnInstruction);

            return body;
        }

        /// <summary>
        /// Create the body of the equals method.
        /// </summary>
        private static MethodBody CreateEqualsBody()
        {
            MethodBody body = new MethodBody(null);

            // This pointer and method argument.
            Register rthis = body.AllocateRegister(RCategory.Argument, RType.Object);
            Register rother = body.AllocateRegister(RCategory.Argument, RType.Object);

            // Create code.
            var ins = body.Instructions;

            // Temporary parameter result.
            Register result = body.AllocateRegister(RCategory.Temp, RType.Value);

            // Set result to false.
            ins.Add(new Instruction(RCode.Const, 0, new[] { result }));

            // Add return instructions
            ins.Add(new Instruction(RCode.Return, result));

            return body;
        }
    }
}
