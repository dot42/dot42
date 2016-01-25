using System;
using System.Linq;
using Dot42.CompilerLib.Ast2RLCompiler.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.FrameworkDefinitions;

namespace Dot42.CompilerLib.RL
{
    /// <summary>
    /// RL builder extension methods.
    /// </summary>
    internal static class RLBuilder
    {
        /// <summary>
        /// Ensure the given value register is a temp register.
        /// If it is not, allocate a temp register an copy to it.
        /// </summary>
        public static RLRange EnsureTemp(this IRLBuilder builder, ISourceLocation sequencePoint, RegisterSpec value, IRegisterAllocator frame)
        {
            // Is temp?
            if (value.Register.Category == RCategory.Temp)
                return new RLRange(value);
            // No, allocate temp
            var tmp = frame.AllocateTemp(value.Type);
            Instruction ins;
            switch (value.Register.Type)
            {
                case RType.Object:
                    ins = builder.Add(sequencePoint, RCode.Move_object, tmp.Register, value.Register);
                    break;
                case RType.Wide:
                    ins = builder.Add(sequencePoint, RCode.Move_wide, tmp.Register, value.Register);
                    break;
                case RType.Value:
                    ins = builder.Add(sequencePoint, RCode.Move, tmp.Register, value.Register);
                    break;
                default:
                    throw new ArgumentException("Unknown register type " + (int)value.Register.Type);
            }
            return new RLRange(ins, tmp);
        }

        /// <summary>
        /// Create and add an instruction.
        /// 
        /// This seems to be a dangerous overload, since it may hide the one below if operand is null.
        /// We can't remove it without checking all ~170 usages though...
        /// </summary>
        public static Instruction Add(this IRLBuilder builder, ISourceLocation sequencePoint, RCode opcode)
        {
            return builder.Add(sequencePoint, opcode, (object)null, new Register[0]);
        }

        /// <summary>
        /// Create and add an instruction.
        /// 
        /// This seems to be a dangerous overload, since it may hide the one below if operand is null.
        /// We can't remove it without checking all ~170 usages though...
        /// </summary>
        public static Instruction Add(this IRLBuilder builder, ISourceLocation sequencePoint, RCode opcode, params Register[] registers)
        {
            if (registers.Any(r => r == null))
                throw new InvalidOperationException("Register must not be null. Wrong overload?");

            return builder.Add(sequencePoint, opcode, (object)null, registers);
        }

        /// <summary>
        /// Create and add an instruction.
        /// </summary>
        public static Instruction Add(this IRLBuilder builder, ISourceLocation sequencePoint, RCode opcode, object operand, params Register[] registers)
        {
            if(operand is Register)
                throw new InvalidOperationException("Wrong overload. Please fix me..."); // see above.
            if(operand is RegisterSpec || operand is RLRange)
                throw new InvalidOperationException("Wrong kind of argument. Please fix me..."); // see above.

            return builder.Add(sequencePoint, opcode, operand, registers);
        }

        /// <summary>
        /// Create code to box the given source value into the given type.
        /// </summary>
        public static RLRange Box(this IRLBuilder builder, ISourceLocation sequencePoint, RegisterSpec source, XTypeReference type, DexTargetPackage targetPackage, IRegisterAllocator frame)
        {
            if (type.IsPrimitive)
            {
                if (type.IsByte()) builder.Add(sequencePoint, RCode.Int_to_byte, source.Register, source.Register);
                else if (type.IsUInt16()) builder.Add(sequencePoint, RCode.Int_to_short, source.Register, source.Register);
                // Call appropriate valueOf method
                var boxedType = type.Module.TypeSystem.Object;
                var r = frame.AllocateTemp(boxedType.GetReference(targetPackage));
                var call = builder.Add(sequencePoint, RCode.Invoke_static, type.GetBoxValueOfMethod(), source.Registers);
                var last = builder.Add(sequencePoint, RCode.Move_result_object, r);
                return new RLRange(call, last, r);
            }
            if (type.IsGenericParameter)
            {
                var nop = builder.Add(sequencePoint, RCode.Nop);
                return new RLRange(nop, source);
            }
            XTypeDefinition typeDef ;
            if (type.TryResolve(out typeDef) && (typeDef.IsEnum))
            {
                // Call appropriate valueOf method
                /*var boxedType = type.Module.TypeSystem.Object;
                var r = frame.AllocateTemp(boxedType.GetReference(target, nsConverter));
                var call = builder.Add(sequencePoint, RCode.Invoke_static, typeDef.GetEnumUnderlyingType().GetBoxValueOfMethod(), source.Registers);
                var last = builder.Add(sequencePoint, RCode.Move_result_object, r);
                return new RLRange(call, last, r);*/
            }

            // Just cast
            var checkCast = builder.Add(sequencePoint, RCode.Check_cast, type.GetReference(targetPackage), source);
            return new RLRange(checkCast, source);
        }

        /// <summary>
        /// Create code to unbox the given source value into the given type.
        /// </summary>
        public static RLRange Unbox(this IRLBuilder builder, ISourceLocation sequencePoint, RegisterSpec source, XTypeReference type, AssemblyCompiler compiler, DexTargetPackage targetPackage, IRegisterAllocator frame)
        {
            if (type.IsPrimitive)
            {
                RCode convertAfterCode;
                var rUnboxed = frame.AllocateTemp(type.GetReference(targetPackage));
                var unboxValueMethod = type.GetUnboxValueMethod(compiler, targetPackage, out convertAfterCode);
                var first = builder.Add(sequencePoint, RCode.Invoke_static, unboxValueMethod, source);
                var last = builder.Add(sequencePoint, type.MoveResult(), rUnboxed);
                if (convertAfterCode != RCode.Nop)
                {
                    last = builder.Add(sequencePoint, convertAfterCode, rUnboxed, rUnboxed);
                }

                return new RLRange(first, last, rUnboxed);
            }

            XTypeDefinition enumTypeDef;
            if (type.IsEnum(out enumTypeDef))
            {
                var rUnboxed = frame.AllocateTemp(type.GetReference(targetPackage));
                var unboxValueMethod = enumTypeDef.Methods.First(x => x.Name == NameConstants.Enum.UnboxMethodName).GetReference(targetPackage);
                var first = builder.Add(sequencePoint, RCode.Invoke_static, unboxValueMethod, source);
                var last = builder.Add(sequencePoint, type.MoveResult(), rUnboxed);
                return new RLRange(first, last, rUnboxed);                
            }

            if (!type.IsGenericParameter)
            {
                // Just cast
                var checkCast = builder.Add(sequencePoint, RCode.Check_cast, type.GetReference(targetPackage), source);
                return new RLRange(checkCast, source);
            }

            // Do nothing
            var nop = builder.Add(sequencePoint, RCode.Nop);
            return new RLRange(nop, source);
        }

        /// <summary>
        /// Emit code (if needed) to convert a value from source type to destination type.
        /// This method is used in "store" opcodes such stloc, stfld, stsfld, call
        /// </summary>
        internal static RLRange ConvertTypeBeforeStore(this IRLBuilder builder, ISourceLocation sequencePoint, XTypeReference sourceType, XTypeReference destinationType, RegisterSpec source, DexTargetPackage targetPackage, IRegisterAllocator frame, AssemblyCompiler compiler, out bool converted)
        {
            converted = false;
            if (sourceType.IsSame(destinationType))
            {
                // Unsigned conversions
                if (sourceType.IsByte())
                {
                    var tmp = builder.EnsureTemp(sequencePoint, source, frame);
                    var ins = builder.Add(sequencePoint, RCode.Int_to_byte, tmp.Result, tmp.Result);
                    converted = true;
                    return new RLRange(tmp, ins, tmp.Result);
                }
                else if (sourceType.IsUInt16())
                {
                    var tmp = builder.EnsureTemp(sequencePoint, source, frame);
                    var ins = builder.Add(sequencePoint, RCode.Int_to_short, tmp.Result, tmp.Result);
                    converted = true;
                    return new RLRange(tmp, ins, tmp.Result);
                }
                return new RLRange(source);
            }

            if (sourceType.IsArray)
            {
                var compilerHelper = compiler.GetDot42InternalType(InternalConstants.CompilerHelperName).Resolve();
                var arrayType = targetPackage.DexFile.GetClass(targetPackage.NameConverter.GetConvertedFullName(compilerHelper));
                var sourceArrayElementType = ((XArrayType)sourceType).ElementType;
                if (destinationType.ExtendsIList())
                {
                    // Use ArrayHelper.AsList to convert
                    var convertMethodName = "AsList";
                    var convertMethod = arrayType.GetMethod(convertMethodName);
                    // Add code
                    var tmp = builder.EnsureTemp(sequencePoint, source, frame);
                    builder.Add(sequencePoint, RCode.Invoke_static, convertMethod, tmp.Result.Register);
                    var last = builder.Add(sequencePoint, RCode.Move_result_object, tmp.Result.Register);
                    converted = true;
                    return new RLRange(tmp, last, tmp.Result);
                }
                if (destinationType.ExtendsICollection())
                {
                    // Use ArrayHelper.AsCollection to convert
                    var convertMethodName = "AsCollection";
                    var convertMethod = arrayType.GetMethod(convertMethodName);
                    // Add code
                    var tmp = builder.EnsureTemp(sequencePoint, source, frame);
                    builder.Add(sequencePoint, RCode.Invoke_static, convertMethod, tmp.Result.Register);
                    var last = builder.Add(sequencePoint, RCode.Move_result_object, tmp.Result.Register);
                    converted = true;
                    return new RLRange(tmp, last, tmp.Result);
                }
                if (destinationType.ExtendsIEnumerable())
                {
                    // Use ArrayHelper.As...Enumerable to convert
                    var convertMethodName = FrameworkReferences.GetAsEnumerableTMethodName(sourceArrayElementType);
                    var convertMethod = arrayType.GetMethod(convertMethodName);
                    // Add code
                    var tmp = builder.EnsureTemp(sequencePoint, source, frame);
                    builder.Add(sequencePoint, RCode.Invoke_static, convertMethod, tmp.Result.Register);
                    var last = builder.Add(sequencePoint, RCode.Move_result_object, tmp.Result.Register);
                    converted = true;
                    return new RLRange(tmp, last, tmp.Result);
                }
            }

            if (sourceType.IsGenericParameter && !destinationType.IsSystemObject())
            {
                var gp = (XGenericParameter) sourceType;
                if (gp.Constraints.Length > 0)
                {
                    // we could find the best matching constraint here, and check if we actually
                    // need to cast. This would probably allow us to skip some unneccesary casts.
                    // We would need some sort of IsAssignableFrom though, and I'm not sure we have
                    // this logic with XTypeDefinition implemented yet.
                    // Therefore, we just assume that the original compiler has done its job well,
                    // and always cast to the destination type.
                    // Apparently dex seems not to need a cast when destinationType is an interface. 
                    // Since i'm not to sure about this, we nevertheless insert the cast here. 
                    // [TODO: check if this is needed]
                    var tmp = builder.EnsureTemp(sequencePoint, source, frame);
                    var cast = builder.Add(sequencePoint, RCode.Check_cast, destinationType.GetReference(targetPackage), tmp.Result);
                    converted = true;
                    return new RLRange(tmp, cast, tmp.Result);
                }
            }

            // Do not convert
            return new RLRange(source);
        }

        /// <summary>
        /// Create code to box the given source array of primitive type elements into an array of boxed elements.
        /// </summary>
        public static RLRange BoxGenericArray(this IRLBuilder builder, ISourceLocation sequencePoint, RegisterSpec source,
                                               XTypeReference type, DexTargetPackage targetPackage, IRegisterAllocator frame, AssemblyCompiler compiler)
        {
            var objectArrayType = new DexLib.ArrayType(new ClassReference("java.lang.Object"));
            var boxedArray = frame.AllocateTemp(objectArrayType);
            var internalBoxingType = compiler.GetDot42InternalType("Boxing").Resolve();
            var ilBoxMethod = internalBoxingType.Methods.First(x => x.EqualsName("Box") && (x.Parameters.Count == 1) && (x.Parameters[0].ParameterType.IsSame(type, true)));
            var boxMethod = ilBoxMethod.GetReference(targetPackage);
            var call = builder.Add(sequencePoint, RCode.Invoke_static, boxMethod, source);
            var saveArray = builder.Add(sequencePoint, RCode.Move_result_object, boxedArray);
            return new RLRange(call, saveArray, boxedArray);
        }

        /// <summary>
        /// Create code to unbox the given source array of boxed type elements into an array of primitive elements.
        /// </summary>
        public static RLRange UnboxGenericArray(this IRLBuilder builder, ISourceLocation sequencePoint, RegisterSpec source, RegisterSpec boxedArray,
                                               XTypeReference type, DexTargetPackage targetPackage, IRegisterAllocator frame, AssemblyCompiler compiler)
        {
            var internalBoxingType = compiler.GetDot42InternalType("Boxing").Resolve();
            var ilUnboxMethod = internalBoxingType.Methods.First(x => x.EqualsName("UnboxTo") && (x.Parameters.Count == 2) && (x.Parameters[1].ParameterType.IsSame(type, true)));
            var unboxMethod = ilUnboxMethod.GetReference(targetPackage);
            var call = builder.Add(sequencePoint, RCode.Invoke_static, unboxMethod, boxedArray, source);
            return new RLRange(call, null);
        }

        /// <summary>
        /// Create code to unbox the given source array of boxed type elements resulting from a call into an array of primitive elements.
        /// </summary>
        public static RLRange UnboxGenericArrayResult(this IRLBuilder builder, ISourceLocation sequencePoint, RegisterSpec boxedArray,
                                               XTypeReference type, DexTargetPackage targetPackage, IRegisterAllocator frame, AssemblyCompiler compiler)
        {
            var internalBoxingType = compiler.GetDot42InternalType("Boxing").Resolve();
            var primitiveArray = frame.AllocateTemp(type.GetReference(targetPackage));
            var ilUnboxMethod = internalBoxingType.Methods.First(x => x.Name.StartsWith("Unbox") && (x.Parameters.Count == 1) && (x.ReturnType.IsSame(type, true)));
            var unboxMethod = ilUnboxMethod.GetReference(targetPackage);
            var call = builder.Add(sequencePoint, RCode.Invoke_static, unboxMethod, boxedArray);
            var saveArray = builder.Add(sequencePoint, RCode.Move_result_object, primitiveArray);
            return new RLRange(call, saveArray, primitiveArray);
        }
    }
}
