using System.Linq;
using Dot42.JvmClassLib.Structures;

namespace Dot42.JvmClassLib
{
    public static class Extensions
    {
        /// <summary>
        /// Is the given class an enum?
        /// </summary>
        public static bool IsEnum(this ClassFile cf)
        {
            return (cf.IsEnum || ((cf.SuperClass != null) && (cf.SuperClass.ClassName == "java/lang/Enum")));
        }

        /// <summary>
        /// Is the given method a bridge method that calls the given target method?
        /// </summary>
        public static bool IsBridgeFor(this MethodDefinition method, MethodDefinition target)
        {
            if ((!method.IsBridge) || (method == target))
                return false;
            if ((method.Name != target.Name) || (method.Parameters.Count != target.Parameters.Count))
                return false;
            if (method.Body == null)
                return false;
            return method.Body.Instructions.Select(x => x.Operand).OfType<ConstantPoolMethodRef>().Any(x => (x.Descriptor == target.Descriptor) && (x.Name == target.Name));
        }
    }
}
