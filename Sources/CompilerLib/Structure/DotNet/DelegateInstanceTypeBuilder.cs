using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.Ast2RLCompiler.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.FrameworkDefinitions;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for existing uses of .NET delegate types.
    /// </summary>
    internal sealed class DelegateInstanceTypeBuilder
    {
        private readonly ISourceLocation sequencePoint;
        private readonly AssemblyCompiler compiler;
        private readonly DexTargetPackage targetPackage;
        private readonly ClassDefinition delegateClass;
                
        private readonly XMethodDefinition invokeMethod;
        private readonly Prototype invokePrototype;
                
        private readonly XMethodDefinition calledMethod;

        private FieldDefinition instanceField = null;
        private readonly List<FieldDefinition> genericInstanceTypeFields = new List<FieldDefinition>();
        private readonly List<FieldDefinition> genericMethodTypeFields = new List<FieldDefinition>();

        private FieldDefinition methodInfoField = null;
        private readonly ClassReference multicastDelegateClass;

        private IEnumerable<FieldDefinition> GenericTypeFields { get { return genericInstanceTypeFields.Concat(genericMethodTypeFields); } }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        internal DelegateInstanceTypeBuilder(
            ISourceLocation sequencePoint,
            AssemblyCompiler compiler, DexTargetPackage targetPackage,
            ClassDefinition delegateClass,
            XMethodDefinition invokeMethod, Prototype invokePrototype,
            XMethodDefinition calledMethod)
        {
            this.sequencePoint = sequencePoint;
            this.compiler = compiler;
            this.targetPackage = targetPackage;
            this.delegateClass = delegateClass;
            this.invokeMethod = invokeMethod;
            this.invokePrototype = invokePrototype;
            this.calledMethod = calledMethod;
            this.multicastDelegateClass = compiler.GetDot42InternalType("System", "MulticastDelegate").GetClassReference(targetPackage);
        }

        public DelegateInstanceType Create()
        {
            instanceField = null; // actually at the momennt, we are not called multiple times...
            genericMethodTypeFields.Clear();
            genericInstanceTypeFields.Clear();

            // Prepare called method
            var target = targetPackage.DexFile;
            var owner = target.GetClass(calledMethod.DeclaringType.GetClassReference(targetPackage).Fullname)
                        ?? targetPackage.GetOrCreateGeneratedCodeClass();
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

            var @class = new ClassDefinition
            {
                Name = CreateInstanceTypeName(owner),
                Namespace = owner.Namespace,
                AccessFlags = AccessFlags.Public | AccessFlags.Final,
                MapFileId = compiler.GetNextMapFileId(),
            };
            owner.AddInnerClass(@class);

            // Set super class
            @class.SuperClass = delegateClass;

            // Implement delegate interface
            //@class.Interfaces.Add(delegateInterface);

            // Get type of instance 
            XTypeDefinition instanceType = calledMethod.DeclaringType;
            TypeReference instanceTypeRef = instanceType.GetReference(targetPackage);

            // Add ctor
            var ctor = new MethodDefinition
            {
                Owner = @class,
                Name = "<init>",
                AccessFlags = AccessFlags.Public | AccessFlags.Constructor,
                Prototype = new Prototype(PrimitiveType.Void),
            };

            ctor.Prototype.Unfreeze();
            if (!calledMethod.IsStatic)
            {
                ctor.Prototype.Parameters.Add(new Parameter(instanceTypeRef, "this"));
            }

            PrototypeBuilder.AddGenericParameters(compiler, targetPackage, calledMethod, ctor.Prototype);
            ctor.Prototype.Freeze();
            @class.Methods.Add(ctor);

            // Add methodInfo field
            methodInfoField = new FieldDefinition();
            methodInfoField.Name = "methodInfo";
            methodInfoField.Owner = @class;
            methodInfoField.Type = compiler.GetDot42InternalType("System.Reflection", "MethodInfo").GetReference(targetPackage);
            methodInfoField.AccessFlags = AccessFlags.Private | AccessFlags.Final | AccessFlags.Static;
            @class.Fields.Add(methodInfoField);

            // Add instance field & getTargetImpl method
            if (!calledMethod.IsStatic)
            {
                instanceField = new FieldDefinition();
                instanceField.Name = "instance";
                instanceField.Owner = @class;
                instanceField.Type = instanceTypeRef;
                instanceField.AccessFlags = AccessFlags.Private | AccessFlags.Final;
                @class.Fields.Add(instanceField);

                AddMethod(@class, "GetTargetImpl", new Prototype(FrameworkReferences.Object), AccessFlags.Protected,
                          CreateGetTargetImplBody());
            }

            // Add generic instance type and method fields
            var gtpa = compiler.GetDot42InternalType(InternalConstants.GenericTypeParameterAnnotation).GetClassReference(targetPackage);
            var gmpa = compiler.GetDot42InternalType(InternalConstants.GenericMethodParameterAnnotation).GetClassReference(targetPackage);
            foreach (var parameter in ctor.Prototype.Parameters)
            {
                bool isGtpa = parameter.Annotations.Any(a => a.Type.Equals(gtpa));
                bool isGmpa = parameter.Annotations.Any(a => a.Type.Equals(gmpa));
                if (isGmpa || isGtpa)
                {
                    var list = isGtpa ? genericInstanceTypeFields : genericMethodTypeFields;
                    var field = new FieldDefinition();
                    field.Name = isGtpa ? "$git" : "$gmt";
                    if (parameter.Type.Equals(FrameworkReferences.Class))
                        field.Name += list.Count + 1;
                    field.Owner = @class;
                    field.Type = parameter.Type;
                    field.AccessFlags = AccessFlags.Private | AccessFlags.Final;
                    @class.Fields.Add(field);
                    list.Add(field);
                }
            }

            // Create ctor body
            var ctorBody = CreateCtorBody();
            targetPackage.Record(new CompiledMethod() { DexMethod = ctor, RLBody = ctorBody });

            // add class static ctor
            AddMethod(@class, "<clinit>", new Prototype(PrimitiveType.Void),
                      AccessFlags.Public | AccessFlags.Constructor | AccessFlags.Static,
                      CreateCctorBody());

            // Add Invoke method
            AddMethod(@class, "Invoke", invokePrototype, AccessFlags.Public, CreateInvokeBody(calledMethodPrototype));

            // Add Equals method
            var typeOnlyEqualsSuffices = calledMethod.IsStatic && !calledMethod.NeedsGenericInstanceTypeParameter && !calledMethod.NeedsGenericInstanceMethodParameter;
            var equalsBody = typeOnlyEqualsSuffices ? CreateEqualsCheckTypeOnlyBody(@class) : CreateEqualsBody(@class);

            var equalsPrototype = new Prototype(PrimitiveType.Boolean, new Parameter(multicastDelegateClass, "other"));
            AddMethod(@class, "EqualsWithoutInvocationList", equalsPrototype, AccessFlags.Protected, equalsBody);

            if (!typeOnlyEqualsSuffices)
            {
                var hashCodePrototype = new Prototype(PrimitiveType.Int);
                AddMethod(@class, "HashCodeWithoutInvocationList", hashCodePrototype, AccessFlags.Protected, CreateHashCodeBody(@class));
            }

            var clonePrototype = new Prototype(multicastDelegateClass, new Parameter(new ArrayType(multicastDelegateClass), "invocationList"), new Parameter(PrimitiveType.Int, "invocationListLength"));
            AddMethod(@class, "CloneWithNewInvocationList", clonePrototype, AccessFlags.Protected, 
                        CreateCloneBody(ctor, @class));

            AddMethod(@class, "GetMethodInfoImpl", new Prototype(methodInfoField.Type),AccessFlags.Protected, 
                      CreateGetMethodInfoImplBody());

            return new DelegateInstanceType(calledMethod, @class, ctor);            
        }

        private void AddMethod(ClassDefinition @class, string methodName, Prototype prototype, AccessFlags accessFlags, MethodBody body)
        {
            var method = new MethodDefinition(@class, methodName, prototype);
            method.AccessFlags = accessFlags;
            @class.Methods.Add(method);
            targetPackage.Record(new CompiledMethod { DexMethod = method, RLBody = body});
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
        private MethodBody CreateCtorBody()
        {
            var body = new MethodBody(null);
            // Create code
            var ins = body.Instructions;
            var rthis = body.AllocateRegister(RCategory.Argument, RType.Object);
            // Call base ctor
            var baseCtorRef = new MethodReference(delegateClass, "<init>", new Prototype(PrimitiveType.Void));
            ins.Add(new Instruction(RCode.Invoke_direct, rthis) { Operand = baseCtorRef });


            if (instanceField != null)
            {
                // load instance into field
                var rInstanceArg = body.AllocateRegister(RCategory.Argument, RType.Object);
                ins.Add(new Instruction(RCode.Iput_object, rInstanceArg, rthis) {Operand = instanceField});
            }

            foreach (var field in GenericTypeFields)
            {
                var rArg = body.AllocateRegister(RCategory.Argument, RType.Object);
                ins.Add(new Instruction(RCode.Iput_object, rArg, rthis) { Operand = field });
            }

            ins.Add(new Instruction(RCode.Return_void));
            return body;
        }

        /// <summary>
        /// Create the body of the invoke method.
        /// </summary>
        /// <param name="calledMethodPrototype"></param>
        private MethodBody CreateInvokeBody(Prototype calledMethodPrototype)
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
            Register instanceReg = null;
            if (!calledMethod.IsStatic)
            {
                // load instance
                instanceReg = body.AllocateRegister(RCategory.Temp, RType.Object);
                ins.Add(new Instruction(RCode.Iget_object, instanceReg, rthis) { Operand = instanceField });
            }

            List<Register> genericTypeParameterRegs = new List<Register>(); 
            foreach(var field in GenericTypeFields)
            {
                var r = body.AllocateRegister(RCategory.Temp, RType.Object);
                ins.Add(new Instruction(RCode.Iget_object, r, rthis) { Operand = field });
                genericTypeParameterRegs.Add(r);
            }

            // Invoke
            var calledMethodRef = calledMethod.GetReference(targetPackage);
            var inputArgs = calledMethod.IsStatic ? incomingMethodArgs.Skip(1).ToArray() : incomingMethodArgs;
            
            // Cast arguments (if needed)
            var outputArgs = new List<Register>();
            if (!calledMethod.IsStatic)
            {
                outputArgs.Add(instanceReg);
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

            outputArgs.AddRange(genericTypeParameterRegs);

            // Actual call
            ins.Add(new Instruction(calledMethod.Invoke(calledMethod, null), calledMethodRef, outputArgs.ToArray()));

            // Collect return value
            var invokeReturnType = invokePrototype.ReturnType;
            var calledReturnType = calledMethodPrototype.ReturnType;
            var needsBoxing = !invokeReturnType.Equals(calledReturnType);
            Instruction returnInstruction;
            Instruction nextMoveResultInstruction = null;

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
                    nextMoveResultInstruction = new Instruction(RCode.Move_result_object, tmp.Result.Register);
                }
                else
                {
                    // Return wide
                    returnInstruction = new Instruction(RCode.Return_wide, r.Item1);
                    nextMoveResultInstruction = new Instruction(RCode.Move_result_wide, r.Item1);
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
                    nextMoveResultInstruction = new Instruction(RCode.Move_result_object, tmp.Result.Register);
                }
                else
                {
                    // Return 
                    returnInstruction = new Instruction(RCode.Return, r);
                    nextMoveResultInstruction = new Instruction(RCode.Move_result, r);
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
                    nextMoveResultInstruction = new Instruction(RCode.Move_result_object, tmp.Result.Register);
                }
                else
                {
                    // Return 
                    returnInstruction = new Instruction(RCode.Return_object, r);
                    nextMoveResultInstruction = new Instruction(RCode.Move_result_object, r);
                }
            }

            // Call delegate list
            var multicastDelegateType = new ClassReference(targetPackage.NameConverter.GetConvertedFullName("System.MulticastDelegate"));
            var invListLengthReference = new FieldReference(multicastDelegateType, "InvocationListLength", PrimitiveType.Int);
            var multicastDelegateArray = new ArrayType(multicastDelegateType);
            var invListReference = new FieldReference(multicastDelegateType, "InvocationList", multicastDelegateArray);

            var index = body.AllocateRegister(RCategory.Temp, RType.Value);
            var count = body.AllocateRegister(RCategory.Temp, RType.Value);
            var next = body.AllocateRegister(RCategory.Temp, RType.Object);
            var invList = body.AllocateRegister(RCategory.Temp, RType.Object);

            var done = new Instruction(RCode.Nop);

            var nextInvokeMethod = new MethodReference(delegateClass, "Invoke", invokePrototype);
            var nextInvokeArgs = new[] { next }.Concat(incomingMethodArgs.Skip(1)).ToArray();

            ins.Add(new Instruction(RCode.Iget, invListLengthReference, new[] {count, rthis}));
            ins.Add(new Instruction(RCode.If_eqz, done, new[] { count }));
            ins.Add(new Instruction(RCode.Const, 0, new[] { index }));
            ins.Add(new Instruction(RCode.Iget_object, invListReference, new[] {invList, rthis}));

            var getNext = new Instruction(RCode.Aget_object, null, new[] { next, invList, index });
            ins.Add(getNext);
            ins.Add(new Instruction(RCode.Check_cast, delegateClass, new [] { next }));
            ins.Add(new Instruction(RCode.Invoke_virtual, nextInvokeMethod, nextInvokeArgs));
            
            if (nextMoveResultInstruction != null)
                ins.Add(nextMoveResultInstruction);

            ins.Add(new Instruction(RCode.Add_int_lit8, 1, new[] { index, index }));
            ins.Add(new Instruction(RCode.If_lt, getNext, new[] { index, count }));

            ins.Add(done);

            // Add return instructions
            ins.Add(returnInstruction);

            return body;
        }

        /// <summary>
        /// Create the body of the equals method.
        /// </summary>
        private MethodBody CreateEqualsBody(ClassReference delegateInstance)
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
            Instruction returnFalseInstruction = new Instruction(RCode.Return, result);
            
            
            // Check if other object can be casted.
            ins.Add(new Instruction(RCode.Instance_of, delegateInstance, new[] { result, rother }));
            ins.Add(new Instruction(RCode.If_eqz, returnFalseInstruction, new[] { result }));

            // Set result to false on default.
            ins.Add(new Instruction(RCode.Const, 0, new[] { result }));

            // Cast of the other object.
            ins.Add(new Instruction(RCode.Check_cast, delegateInstance, new[] { rother }));

            // Get instance fields of this and other.
            var rThisValue = body.AllocateRegister(RCategory.Temp, RType.Object);
            var rOtherValue = body.AllocateRegister(RCategory.Temp, RType.Object);

            if (instanceField != null)
            {
                // Load the instance fields.
                ins.Add(new Instruction(RCode.Iget_object, rThisValue, rthis) {Operand = instanceField});
                ins.Add(new Instruction(RCode.Iget_object, rOtherValue, rother) {Operand = instanceField});

                // Compare the instance fields.
                ins.Add(new Instruction(RCode.If_ne, returnFalseInstruction, new[] {rThisValue, rOtherValue}));
            }

            foreach (var field in GenericTypeFields)
            {
                if (field.Type.Equals(FrameworkReferences.Class))
                {
                    // simply load and compare the fields.
                    ins.Add(new Instruction(RCode.Iget_object, rThisValue, rthis) { Operand = field });
                    ins.Add(new Instruction(RCode.Iget_object, rOtherValue, rother) { Operand = field });
                    ins.Add(new Instruction(RCode.If_ne, returnFalseInstruction, new[] { rThisValue, rOtherValue }));
                }
                else // array
                {
                    CreateCompareArrayInstructions(ins, body, rthis, rother, field, rThisValue, rOtherValue, returnFalseInstruction);    
                }
            }

            // return true, if we made it so far
            ins.Add(new Instruction(RCode.Const, 1, new[] { result }));

            // Add return instructions
            ins.Add(returnFalseInstruction);

            return body;
        }

        /// <summary>
        /// Create the body of the equals method.
        /// </summary>
        private MethodBody CreateHashCodeBody(ClassReference delegateInstance)
        {
            MethodBody body = new MethodBody(null);

            // This pointer and method argument.
            Register rthis = body.AllocateRegister(RCategory.Argument, RType.Object);
            Register tempObj = body.AllocateRegister(RCategory.Temp, RType.Object);
            Register tempInt = body.AllocateRegister(RCategory.Temp, RType.Value);
            
            Register tempArray = body.AllocateRegister(RCategory.Temp, RType.Object);
            Register tempIdx = body.AllocateRegister(RCategory.Temp, RType.Value);
            Register tempArrayLength = body.AllocateRegister(RCategory.Temp, RType.Value);

            // Create code.
            var ins = body.Instructions;

            // Temporary parameter result.
            Register result = body.AllocateRegister(RCategory.Temp, RType.Value);

            var hashCodeMethod = compiler.GetDot42InternalType("Java.Lang", "System")
                                         .Resolve()
                                         .Methods.First(m => m.Name == "IdentityHashCode")
                                         .GetReference(targetPackage);
            
            // Check if other object can be casted.
            ins.Add(new Instruction(RCode.Const_class, delegateInstance, new[] { tempObj }));
            ins.Add(new Instruction(RCode.Invoke_static, hashCodeMethod, new[] { tempObj }));
            ins.Add(new Instruction(RCode.Move_result, null, new[] { result }));

            if (instanceField != null)
            {
                ins.Add(new Instruction(RCode.Mul_int_lit, 397, new[] { result, result }));
                ins.Add(new Instruction(RCode.Iget_object, tempObj, rthis) { Operand = instanceField });
                ins.Add(new Instruction(RCode.Invoke_static, tempObj) { Operand = hashCodeMethod });
                ins.Add(new Instruction(RCode.Move_result, tempInt));
                ins.Add(new Instruction(RCode.Xor_int_2addr, result, tempInt));
            }

            foreach (var field in GenericTypeFields)
            {
                if (field.Type.Equals(FrameworkReferences.Class))
                {
                    
                    ins.Add(new Instruction(RCode.Iget_object, tempObj, rthis) { Operand = field });
                    ins.Add(new Instruction(RCode.Invoke_static, hashCodeMethod, new[] { tempObj }));
                    ins.Add(new Instruction(RCode.Move_result, null, new[] { tempInt }));
                    ins.Add(new Instruction(RCode.Xor_int_2addr, null, new[] { result, tempInt }));
                }
                else // array
                {
                    ins.Add(new Instruction(RCode.Iget_object, tempArray, rthis) { Operand = field });
                    ins.Add(new Instruction(RCode.Array_length, tempArrayLength, rthis));
                    ins.Add(new Instruction(RCode.Const, tempIdx){ Operand = 0});

                    Instruction loop;
                    ins.Add(loop = new Instruction());
                    ins.Add(new Instruction(RCode.Mul_int_lit, 397, new[] {result, result}));
                    ins.Add(new Instruction(RCode.Aget_object, tempObj, tempArray, tempIdx));
                    ins.Add(new Instruction(RCode.Invoke_static, hashCodeMethod, new[] { tempObj }));
                    ins.Add(new Instruction(RCode.Move_result, null, new[] { tempInt }));
                    ins.Add(new Instruction(RCode.Xor_int_2addr, null, new[] { result, tempInt }));

                    ins.Add(new Instruction(RCode.Add_int_lit8, 1, new[] { tempIdx, tempIdx }));
                    ins.Add(new Instruction(RCode.If_ltz, tempIdx, tempArrayLength) { Operand = loop});
                }
            }

            // Add return instructions
            ins.Add(new Instruction(RCode.Return, null, new[] { result }));

            return body;
        }

        private static void CreateCompareArrayInstructions(InstructionList ins, MethodBody body, Register rthis, Register rother, FieldDefinition field, Register rThisValue, Register rOtherValue, Instruction returnFalseInstruction)
        {
            Instruction done = new Instruction(RCode.Nop);
            // Load the instance fields.
            ins.Add(new Instruction(RCode.Iget_object, rThisValue, rthis) {Operand = field});
            ins.Add(new Instruction(RCode.Iget_object, rOtherValue, rother) {Operand = field});

            var rThisLen = body.AllocateRegister(RCategory.Temp, RType.Value);
            var rOtherLen = body.AllocateRegister(RCategory.Temp, RType.Value);

            // load length
            ins.Add(new Instruction(RCode.Array_length, rThisLen, rThisValue));
            ins.Add(new Instruction(RCode.Array_length, rOtherLen, rOtherValue));

            // Compare the length
            ins.Add(new Instruction(RCode.If_ne, returnFalseInstruction, new[] { rThisLen, rOtherLen }));
            ins.Add(new Instruction(RCode.If_eqz, done, new[] {rThisLen}));

            // now iterate over all elements in the array.
            var thisType = body.AllocateRegister(RCategory.Temp, RType.Object);
            var otherType = body.AllocateRegister(RCategory.Temp, RType.Object);
            var counter = body.AllocateRegister(RCategory.Temp, RType.Object);
            ins.Add(new Instruction(RCode.Const, 0, new[] {counter}));

            var loadThisVal = new Instruction(RCode.Aget_object, thisType, rThisValue, counter);
            ins.Add(loadThisVal);
            ins.Add(new Instruction(RCode.Aget_object, otherType, rOtherValue, counter));

            // compare types.
            ins.Add(new Instruction(RCode.If_ne, returnFalseInstruction, new[] {thisType, otherType}));

            ins.Add(new Instruction(RCode.Add_int_lit8, 1, new[] {counter, counter}));
            ins.Add(new Instruction(RCode.If_ne, loadThisVal, new[] {counter, rThisLen}));
            
            ins.Add(done);
        }

        /// <summary>
        /// Create the body of the equals method.
        /// </summary>
        private static MethodBody CreateEqualsCheckTypeOnlyBody(ClassReference delegateClass)
        {
            MethodBody body = new MethodBody(null);

            // This pointer and method argument.
            Register rthis = body.AllocateRegister(RCategory.Argument, RType.Object);
            Register rother = body.AllocateRegister(RCategory.Argument, RType.Object);

            // result register
            Register result = body.AllocateRegister(RCategory.Temp, RType.Value);

            var ins = body.Instructions;

            // Check if other object can be casted.
            ins.Add(new Instruction(RCode.Instance_of, delegateClass, new[] { result, rother }));

            // Add return instructions
            ins.Add(new Instruction(RCode.Return, result));

            return body;
        }

        private MethodBody CreateCloneBody(MethodDefinition ctor, ClassDefinition @class)
        {
            MethodBody body = new MethodBody(null);
            var ins = body.Instructions;

            Register rthis       = body.AllocateRegister(RCategory.Argument, RType.Object);
            Register rInvList    = body.AllocateRegister(RCategory.Argument, RType.Object);
            Register rInvListLen = body.AllocateRegister(RCategory.Argument, RType.Value);

            Register result     = body.AllocateRegister(RCategory.Temp, RType.Object);

            List<Register> ctorArgs = new List<Register> { result };

            if (instanceField != null)
            {
                var rInstance = body.AllocateRegister(RCategory.Temp, RType.Object);
                ins.Add(new Instruction(RCode.Iget_object, instanceField, new[] {rInstance, rthis}));
                ctorArgs.Add(rInstance);
            }

            foreach (var field in GenericTypeFields)
            {
                var r = body.AllocateRegister(RCategory.Temp, RType.Object);
                ins.Add(new Instruction(RCode.Iget_object, field, new[] { r, rthis }));
                ctorArgs.Add(r);
            }

            ins.Add(new Instruction(RCode.New_instance, @class, new[] {result}));
            ins.Add(new Instruction(RCode.Invoke_direct, ctor, ctorArgs.ToArray()));

            var invListLengthReference = new FieldReference(multicastDelegateClass, "InvocationListLength", PrimitiveType.Int);
            var multicastDelegateArray = new ArrayType(multicastDelegateClass);
            var invListReference = new FieldReference(multicastDelegateClass, "InvocationList", multicastDelegateArray);

            ins.Add(new Instruction(RCode.Iput_object, invListReference, new []{ rInvList, result}));
            ins.Add(new Instruction(RCode.Iput, invListLengthReference, new[] { rInvListLen, result }));
            
            ins.Add(new Instruction(RCode.Return_object, null, new []{result}));
            return body;
        }

        private MethodBody CreateGetTargetImplBody()
        {
            MethodBody body = new MethodBody(null);
            var ins = body.Instructions;
            Register rthis = body.AllocateRegister(RCategory.Argument, RType.Object);
            Register result = body.AllocateRegister(RCategory.Temp, RType.Object);
            ins.Add(new Instruction(RCode.Iget_object, instanceField, new[] { result, rthis }));
            ins.Add(new Instruction(RCode.Return_object, null, new[] { result }));
            return body;
        }

        private MethodBody CreateGetMethodInfoImplBody()
        {
            // TODO: For methods taking generic parameters we want to lazy initialize a 
            //       non-static volatile MethodInfo register.

            MethodBody body = new MethodBody(null);
            var ins = body.Instructions;
            Register rthis = body.AllocateRegister(RCategory.Argument, RType.Object);
            Register result = body.AllocateRegister(RCategory.Temp, RType.Object);
            if(methodInfoField.IsStatic)
                ins.Add(new Instruction(RCode.Sget_object, methodInfoField, new[] { result }));
            else
                ins.Add(new Instruction(RCode.Iget_object, methodInfoField, new[] { result, rthis }));
            ins.Add(new Instruction(RCode.Return_object, null, new[] { result }));
            return body;
        }

        private MethodBody CreateCctorBody()
        {
            MethodBody body = new MethodBody(null);

            Register methodInfoR = CallGetMethodInfo(body, calledMethod.GetReference(targetPackage));
            
            var ins = body.Instructions;
            ins.Add(new Instruction(RCode.Sput_object, methodInfoField, new[] { methodInfoR }));
            ins.Add(new Instruction(RCode.Return_void, null));
            return body;
        }

        private Register CallGetMethodInfo(MethodBody body, MethodReference methodRef)
        {
            // >> var method = TypeHelper.GetMethodInfo(typeof(CalledClass), methodName, new[] { paramType1, ...}, null, null);

            var ins = body.Instructions;

            Register classR        = body.AllocateRegister(RCategory.Temp, RType.Object);
            Register methodNameR   = body.AllocateRegister(RCategory.Temp, RType.Object);
            Register methodParamsR = body.AllocateRegister(RCategory.Temp, RType.Object);
            Register idxR          = body.AllocateRegister(RCategory.Temp, RType.Value);
            var      resultR       = body.AllocateRegister(RCategory.Temp, RType.Object);
            var      nullR         = body.AllocateRegister(RCategory.Temp, RType.Object);

            var parameters = methodRef.Prototype.Parameters;

            ins.Add(new Instruction(RCode.Const_class, methodRef.Owner, new[] {classR}));
            ins.Add(new Instruction(RCode.Const_string, methodRef.Name, new[] { methodNameR }));

            ins.Add(new Instruction(RCode.Const, parameters.Count, new[] {idxR}));
            ins.Add(new Instruction(RCode.New_array, FrameworkReferences.ClassArray, new[] {methodParamsR, idxR}));

            for (int i = 0; i < parameters.Count; ++i)
            {
                ins.Add(new Instruction(RCode.Const, i, new[] {idxR}));
                ins.Add(new Instruction(RCode.Const_class, parameters[i].Type, new[] {resultR}));
                ins.Add(new Instruction(RCode.Aput_object, null, new[] { resultR, methodParamsR, idxR }));
            }

            ins.Add(new Instruction(RCode.Const, 0, new[] { nullR }));

            var xGetMethodInfo = compiler.GetDot42InternalType(InternalConstants.TypeHelperName).Resolve().Methods.First(m => m.Name == "GetMethodInfo");
            var getMethodInfo = xGetMethodInfo.GetReference(targetPackage);

            // TODO: make this work with generic parameters as well.
            ins.Add(new Instruction(RCode.Invoke_virtual, getMethodInfo, new[] { classR, methodNameR, methodParamsR, nullR, nullR }));
            ins.Add(new Instruction(RCode.Move_result_object, null, new[] {resultR}));
            return resultR;
        }
    }
}
