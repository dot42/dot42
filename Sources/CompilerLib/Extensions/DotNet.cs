using System;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.DexLib;
using Dot42.LoaderLib.Extensions;
using Mono.Cecil;
using FieldReference = Dot42.DexLib.FieldReference;
using IMemberDefinition = Mono.Cecil.IMemberDefinition;
using ILMethodDefinition = Mono.Cecil.MethodDefinition;
using ILMethodReference = Mono.Cecil.MethodReference;
using ILTypeReference = Mono.Cecil.TypeReference;
using MethodReference = Dot42.DexLib.MethodReference;
using TypeReference = Dot42.DexLib.TypeReference;

namespace Dot42.CompilerLib.Extensions
{
    /// <summary>
    /// Dex related extension methods
    /// </summary>
    public partial class AssemblyCompilerExtensions
    {
        /// <summary>
        /// Gets a Dex field reference for the given field reference.
        /// </summary>
        internal static FieldReference GetReference(this Mono.Cecil.FieldReference field, DexTargetPackage targetPackage, XModule module)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            var xField = XBuilder.AsFieldReference(module, field);
            return xField.GetReference(targetPackage);
        }

        /// <summary>
        /// Gets a Dex method reference for the given type reference.
        /// </summary>
        internal static MethodReference GetReference(this Mono.Cecil.MethodReference method, DexTargetPackage targetPackage, XModule module)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            var xMethod = XBuilder.AsMethodReference(module, method);
            return xMethod.GetReference(targetPackage);
        }

        /// <summary>
        /// Gets the names out of the given DexImport/JavaImport attribute.
        /// </summary>
        internal static void GetDexOrJavaImportNames(this CustomAttribute attr, IMemberDefinition member, out string memberName, out string descriptor, out string className)
        {
            if (attr.ConstructorArguments.Count == 3)
            {
                // class, member, descriptor
                className = (string)attr.ConstructorArguments[0].Value;
                memberName = (string)attr.ConstructorArguments[1].Value;
                descriptor = (string)attr.ConstructorArguments[2].Value;
            }
            else
            {
                // member, descriptor
                memberName = (string)attr.ConstructorArguments[0].Value;
                descriptor = (string)attr.ConstructorArguments[1].Value;
                // Get className from declaring type
                attr = member.DeclaringType.GetDexImportAttribute(false);
                attr = attr ?? member.DeclaringType.GetJavaImportAttribute(true);
                className = (string)attr.ConstructorArguments[0].Value;
            }            
        }

        /// <summary>
        /// Gets a class reference for the given type reference.
        /// </summary>
        internal static TypeReference GetReference(this Mono.Cecil.TypeReference type, DexTargetPackage targetPackage, XModule module)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var xType = XBuilder.AsTypeReference(module, type);
            return xType.GetReference(targetPackage);
        }

        /// <summary>
        /// Gets a class reference for the given type reference.
        /// </summary>
        internal static ClassReference GetClassReference(this Mono.Cecil.TypeReference type, DexTargetPackage targetPackage, XModule module)
        {
            var classRef = type.GetReference(targetPackage, module) as ClassReference;
            if (classRef == null)
                throw new ArgumentException(string.Format("type {0} is not a class reference", type.FullName));
            return classRef;
        }

        /// <summary>
        /// Try to find a DllImport attribute on the given method and return it's dllName argument (if found).
        /// </summary>
        internal static bool TryGetDllImportName(this Mono.Cecil.MethodDefinition method, out string dllName)
        {
            dllName = null;
            if (!method.HasPInvokeInfo)
                return false;
            dllName = method.PInvokeInfo.Module.Name;
            return true;
        }


        /// <summary>
        /// Create a method reference for the given method using this this as declaring type.
        /// Usually the method will be returned, unless this type is a generic instance type.
        /// </summary>
        internal static ILMethodReference CreateReference(this ILTypeReference declaringType, ILMethodDefinition method)
        {
            var git = declaringType as GenericInstanceType;
            if (git == null) return method;
            var methodRef = new ILMethodReference(method.Name, method.ReturnType, git);
            methodRef.HasThis = method.HasThis;
            methodRef.ExplicitThis = method.ExplicitThis;
            foreach (var p in method.Parameters)
            {
                methodRef.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, p.ParameterType));
            }
            foreach (var gp in method.GenericParameters)
            {
                methodRef.GenericParameters.Add(new GenericParameter(gp.Name, methodRef));
            }
            return methodRef;
        }
    }
}
