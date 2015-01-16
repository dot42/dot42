using System;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.Java;
using Dot42.JvmClassLib;
using Dot42.JvmClassLib.Structures;

namespace Dot42.CompilerLib.Java2Ast
{
    /// <summary>
    /// Converts stack-based bytecode to variable-based bytecode by calculating use-define chains
    /// </summary>
    public sealed partial class AstBuilder
    {
        /// <summary>
        /// Make a .NET type reference for a java class file.
        /// </summary>
        private XTypeReference AsTypeReference(ClassFile classFile, XTypeUsageFlags usageFlags)
        {
            return XBuilder.AsTypeReference(module, classFile, usageFlags);
        }

        /// <summary>
        /// Make a .NET type reference for a java type reference.
        /// </summary>
        private XTypeReference AsTypeReference(JvmClassLib.TypeReference javaRef, XTypeUsageFlags usageFlags)
        {
            return XBuilder.AsTypeReference(module, javaRef, usageFlags);
        }

        /// <summary>
        /// Make a xtype reference for a java class name.
        /// </summary>
        private XTypeReference AsTypeReference(string className, XTypeUsageFlags usageFlags)
        {
            return XBuilder.AsTypeReference(module, className, usageFlags);
        }

        /// <summary>
        /// Make a .NET type reference for a given operand of anewarray.
        /// </summary>
        private XTypeReference AsTypeReference(int anewArrayOperand)
        {
            switch (anewArrayOperand)
            {
                case 4:
                    return module.TypeSystem.Bool;
                case 5:
                    return module.TypeSystem.Char;
                case 6:
                    return module.TypeSystem.Float;
                case 7:
                    return module.TypeSystem.Double;
                case 8:
                    return module.TypeSystem.SByte;
                case 9:
                    return module.TypeSystem.Short;
                case 10:
                    return module.TypeSystem.Int;
                case 11:
                    return module.TypeSystem.Long;
                default:
                    throw new ArgumentException("Unknown value for anewarray operand: " + anewArrayOperand);
            }
        }

        /// <summary>
        /// Make a .NET field reference from a java constant pool method reference.
        /// </summary>
        private XFieldReference AsFieldReference(ConstantPoolFieldRef cpField)
        {
            return XBuilder.AsFieldReference(module, cpField);
        }

        /// <summary>
        /// Make a .NET method reference from a java constant pool method reference.
        /// </summary>
        private XMethodReference AsMethodReference(ConstantPoolMethodRef cpMethod, bool hasThis)
        {
            return XBuilder.AsMethodReference(module, cpMethod, hasThis);
        }

        /// <summary>
        /// Make a .NET method reference for the MonitorEnter/Exit instruction.
        /// </summary>
        private XMethodReference MonitorMethodReference(string methodName)
        {
            var declaringType = new XTypeReference.SimpleXTypeReference(module, "System.Threading", "Monitor", null, false, null);
            var methodRef = new XMethodReference.Simple(methodName, false, module.TypeSystem.Void, declaringType, new[] { module.TypeSystem.Object }, null);
            return methodRef;
        }
    }
}
