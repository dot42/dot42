using System;
using Dot42.DebuggerLib.Model;
using Dot42.JvmClassLib;
using Dot42.Mapping;

namespace Dot42.DebuggerLib
{
    public static class Extensions
    {
        /// <summary>
        /// Is this a primivite value?
        /// If not it is an object reference.
        /// </summary>
        public static bool IsPrimitive(this Jdwp.Tag tag)
        {
            switch (tag)
            {
                case Jdwp.Tag.Array:
                case Jdwp.Tag.Object:
                case Jdwp.Tag.String:
                case Jdwp.Tag.Thread:
                case Jdwp.Tag.ThreadGroup:
                case Jdwp.Tag.ClassLoader:
                case Jdwp.Tag.ClassObject:
                    return false;
                case Jdwp.Tag.Byte:
                case Jdwp.Tag.Char:
                case Jdwp.Tag.Float:
                case Jdwp.Tag.Double:
                case Jdwp.Tag.Int:
                case Jdwp.Tag.Long:
                case Jdwp.Tag.Short:
                case Jdwp.Tag.Void:
                case Jdwp.Tag.Boolean:
                    return true;
                default:
                    throw new ArgumentException("Unknown tag " + (int) tag);
            }
        }

        /// <summary>
        /// Convert a class signature to a name.
        /// </summary>
        public static string SignatureToClrName(this DalvikProcess process, string signature)
        {
            var typeReference = Descriptors.ParseClassType(signature);
            var typeMap = process.Debugger.FrameworkTypeMap;
            if (typeMap!= null)
            {
                FrameworkTypeMap.TypeEntry entry;
                if (typeMap.TryGetFromClassName(typeReference.ClassName, out entry))
                {
                    return entry.FullName;
                }
            }

            var e = process.MapFile.GetTypeBySignature(signature);
            if (e != null)
                return e.Name;

            return typeReference.ClrTypeName;
        }

        /// <summary>
        /// Convert a class signature to a name.
        /// </summary>
        public static string ClrNameToSignature(this DalvikProcess process, string clrName)
        {
            string className = null;

            var typeMap = process.Debugger.FrameworkTypeMap;
            if (typeMap != null)
            {
                FrameworkTypeMap.TypeEntry entry;
                if (typeMap.TryGetFromClrName(clrName, out entry))
                {
                    className = entry.ClassName;
                }
            }

            if (className == null)
            {
                var entry = process.MapFile.GetTypeBySignature(clrName);
                if (entry != null && entry.DexSignature != null)
                    return entry.DexSignature;
            }

            // I'm sure this name conversion is somewhere centralized. but where?
            if (className == null)
            {
                className = char.ToLower(clrName[0]) + clrName.Substring(1);
            }

            className = className.Replace("+", "$").Replace(".", "/");
            return "L" + className + ";";
        }
    }
}
