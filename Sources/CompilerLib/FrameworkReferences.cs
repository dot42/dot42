using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.FrameworkDefinitions;

namespace Dot42.CompilerLib
{
    /// <summary>
    /// References to common framework classes / members
    /// </summary>
    internal static class FrameworkReferences
    {

        /// <summary>
        /// Reference to java.lang.Object
        /// </summary>
        internal static readonly ClassReference Object = new ClassReference("java/lang/Object");

        /// <summary>
        /// Reference to java.lang.Class
        /// </summary>
        internal static readonly ClassReference Class = new ClassReference("java/lang/Class");

        /// <summary>
        /// Reference to java.lang.reflect.Array
        /// </summary>
        internal static readonly ClassReference Array = new ClassReference("java/lang/reflect/Array");

        /// <summary>
        /// Reference to java.lang.String
        /// </summary>
        internal static readonly ClassReference String = new ClassReference("java/lang/String");

        /// <summary>
        /// Reference to java.lang.Runnable
        /// </summary>
        internal static readonly ClassReference Runnable = new ClassReference("java/lang/Runnable");

        /// <summary>
        /// Reference to java.util.Arrays
        /// </summary>
        internal static readonly ClassReference Arrays = new ClassReference("java/util/Arrays");

        /// <summary>
        /// Reference to java.lang.Cloneable
        /// </summary>
        internal static readonly ClassReference Cloneable = new ClassReference("java/lang/Cloneable");

        /// <summary>
        /// Reference to java.lang.reflect.Array.getLength(object)
        /// </summary>
        internal static readonly MethodReference ArrayGetLength = new MethodReference(Array, "getLength", new Prototype(PrimitiveType.Int, new Parameter(Object, "a")));

        /// <summary>
        /// Reference to java.lang.reflect.Array.newInstance(Class, int)
        /// </summary>
        internal static readonly MethodReference ArrayNewInstance = new MethodReference(Array, "newInstance", new Prototype(Object, new Parameter(Class, "c"), new Parameter(PrimitiveType.Int, "l")));

        /// <summary>
        /// Reference to java.lang.reflect.Array.newInstance(Class, int[])
        /// </summary>
        internal static readonly MethodReference ArrayNewInstance2 = new MethodReference(Array, "newInstance", new Prototype(Object, new Parameter(Class, "c"), new Parameter(new ArrayType(PrimitiveType.Int), "l")));

        /// <summary>
        /// Reference to java.lang.Class[]
        /// </summary>
        internal static readonly ArrayType ClassArray = new ArrayType(Class);

        /// <summary>
        /// Reference to int[]
        /// </summary>
        internal static readonly ArrayType IntArray = new ArrayType(PrimitiveType.Int);

        /// <summary>
        /// Reference to java.lang.Object[]
        /// </summary>
        internal static readonly ArrayType ObjectArray = new ArrayType(Object);

        /// <summary>
        /// Reference to java.lang.String.equals(string)
        /// </summary>
        internal static XMethodReference StringEquals(XTypeSystem typeSystem)
        {
            return new XMethodReference.Simple("equals", true, typeSystem.Bool, typeSystem.String, XParameter.Create("other", typeSystem.Object));
        }

        /// <summary>
        /// Reference to java.util.Arrays.fill(Object[], object)
        /// </summary>
        internal static readonly MethodReference ArraysFillObject = new MethodReference(Arrays, "fill", new Prototype(PrimitiveType.Void, new Parameter(ObjectArray, "array"), new Parameter(Object, "value")));
    }
}
