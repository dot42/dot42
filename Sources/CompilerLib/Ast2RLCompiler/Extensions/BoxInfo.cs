using System;
using System.Linq;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.RL;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using MethodReference = Dot42.DexLib.MethodReference;
namespace Dot42.CompilerLib.Ast2RLCompiler.Extensions
{
    internal class BoxInfo
    {
        private static readonly BoxInfo[] Infos = new[] {
                                                            new BoxInfo(XTypeReferenceKind .Byte,
                                                                        new ClassReference("java/lang/Byte"),
                                                                        PrimitiveType.Byte, "UnboxSByte",
                                                                        RCode.Nop),
                                                            new BoxInfo(XTypeReferenceKind .SByte,
                                                                        new ClassReference("java/lang/Byte"),
                                                                        PrimitiveType.Byte, "UnboxSByte",
                                                                        RCode.Nop),
                                                            new BoxInfo(XTypeReferenceKind .Bool,
                                                                        new ClassReference("java/lang/Boolean"),
                                                                        PrimitiveType.Boolean, "UnboxBoolean",
                                                                        RCode.Nop),
                                                            new BoxInfo(XTypeReferenceKind .Char,
                                                                        new ClassReference("java/lang/Character"),
                                                                        PrimitiveType.Char, "UnboxCharacter",
                                                                        RCode.Nop),
                                                            new BoxInfo(XTypeReferenceKind .Short,
                                                                        new ClassReference("java/lang/Short"),
                                                                        PrimitiveType.Short, "UnboxShort",
                                                                        RCode.Nop),
                                                            new BoxInfo(XTypeReferenceKind.UShort,
                                                                        new ClassReference("java/lang/Short"),
                                                                        PrimitiveType.Short, "UnboxShort",
                                                                        RCode.Nop),
                                                            new BoxInfo(XTypeReferenceKind .Int,
                                                                        new ClassReference("java/lang/Integer"),
                                                                        PrimitiveType.Int, "UnboxInteger",
                                                                        RCode.Nop),
                                                            new BoxInfo(XTypeReferenceKind.UInt,
                                                                        new ClassReference("java/lang/Integer"),
                                                                        PrimitiveType.Int, "UnboxInteger",
                                                                        RCode.Nop),
                                                            new BoxInfo(XTypeReferenceKind .Long,
                                                                        new ClassReference("java/lang/Long"),
                                                                        PrimitiveType.Long, "UnboxLong",
                                                                        RCode.Nop),
                                                            new BoxInfo(XTypeReferenceKind .ULong,
                                                                        new ClassReference("java/lang/Long"),
                                                                        PrimitiveType.Long, "UnboxLong",
                                                                        RCode.Nop),
                                                            new BoxInfo(XTypeReferenceKind .Float,
                                                                        new ClassReference("java/lang/Float"),
                                                                        PrimitiveType.Float, "UnboxFloat",
                                                                        RCode.Nop),
                                                            new BoxInfo(XTypeReferenceKind.Double,
                                                                        new ClassReference("java/lang/Double"),
                                                                        PrimitiveType.Double, "UnboxDouble",
                                                                        RCode.Nop),
                                                        };

        private readonly XTypeReferenceKind metadataType;
        private readonly ClassReference boxedClass;
        private readonly PrimitiveType primitiveType;
        private readonly string unboxMethodName;
        private readonly RCode convertAfterCode;

        /// <summary>
        /// Default ctor
        /// </summary>
        private BoxInfo(XTypeReferenceKind metadataType, ClassReference boxedClass, PrimitiveType primitiveType,
                        string unboxMethodName, RCode convertAfterCode)
        {
            this.metadataType = metadataType;
            this.boxedClass = boxedClass;
            this.primitiveType = primitiveType;
            this.unboxMethodName = unboxMethodName;
            this.convertAfterCode = convertAfterCode;
        }

        /// <summary>
        /// Gets the box information for the given type.
        /// </summary>
        private static BoxInfo Get(XTypeReference type)
        {
            var info = Infos.FirstOrDefault(x => type.Is(x.metadataType));
            if (info != null)
                return info;
            throw new ArgumentException(string.Format("No box information for for type {0}", type.FullName));
        }

        /// <summary>
        /// Gets a box method for the given primitive type.
        /// </summary>
        internal static MethodReference GetBoxValueOfMethod(XTypeReference type)
        {
            var info = Get(type);
            var cref = info.boxedClass;
            return new MethodReference(cref, "valueOf", new Prototype(cref, new Parameter(info.primitiveType, "value")));
        }

        /// <summary>
        /// Gets a unbox method for the given primitive type.
        /// </summary>
        internal static MethodReference GetUnboxValueMethod(XTypeReference type, AssemblyCompiler compiler, DexTargetPackage targetPackage, out RCode convertAfterCode)
        {
            var info = Get(type);
            convertAfterCode = info.convertAfterCode;
            var boxingClass = compiler.GetDot42InternalType("Boxing").GetClassReference(targetPackage);
            return new MethodReference(boxingClass, info.unboxMethodName, new Prototype(info.primitiveType, new Parameter(FrameworkReferences.Object, "value")));
        }

        /// <summary>
        /// Gets a unbox method for the given primitive type.
        /// </summary>
        internal static ClassReference GetBoxedType(XTypeReference type)
        {
            return Get(type).boxedClass;
        }
    }
}
