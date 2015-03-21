using System.ComponentModel.Composition;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Reachable;
using Dot42.DexLib;
using Dot42.FrameworkDefinitions;
using MethodReference = Mono.Cecil.MethodReference;
using TypeReference = Mono.Cecil.TypeReference;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// Replace plain System.Enum parameter types with Dot32.Internal.Enum
    /// </summary>
    [Export(typeof (ILConverterFactory))]
    internal class SystemEnumParameterConverter : ILConverterFactory
    {
        /// <summary>
        /// Low values come first
        /// </summary>
        public int Priority
        {
            get { return 50; }
        }

        /// <summary>
        /// Create the converter
        /// </summary>
        public ILConverter Create()
        {
            return new Converter();
        }

        private class Converter : ILConverter
        {
            private const string SystemEnumName = "System.Enum";
            private const string Dot42InternalEnum = InternalConstants.Dot42InternalNamespace + "." + "Enum";

            /// <summary>
            /// Convert calls to android extension ctors.
            /// </summary>
            public void Convert(ReachableContext reachableContext)
            {
                TypeReference enumType = reachableContext.ReachableTypes.SingleOrDefault(x => x.FullName == Dot42InternalEnum);
                
                if (enumType == null) return;
                
                // check all types
                foreach (var type in reachableContext.ReachableTypes)
                {
                    foreach (var method in type.Methods.Where(p => p.IsReachable))
                    {
                        ConvertMethodDefinition(method, enumType);

                        if (method.Body == null) 
                            continue;

                        foreach (var v in method.Body.Variables)
                        {
                            if (v.VariableType.FullName == SystemEnumName)
                                v.VariableType = enumType;
                        }

                        foreach (var ins in method.Body.Instructions)
                        {
                            var methodRef = ins.Operand as MethodReference;
                            if (methodRef == null) continue;
                            ConvertMethodDefinition(methodRef, enumType);

                            if (methodRef.DeclaringType.FullName == SystemEnumName && methodRef.HasThis)
                            {
                                // redirect instance method calls as well.
                                methodRef.DeclaringType = enumType;
                            }
                        }
                    }

                    type.Fields.Where(p => p.IsReachable && p.FieldType.FullName == SystemEnumName)
                               .ForEach(field => field.FieldType = enumType);
                    type.Properties.Where(p => p.IsReachable && p.PropertyType.FullName == SystemEnumName)
                               .ForEach(field => field.PropertyType = enumType);
                }
            }

            private static void ConvertMethodDefinition(MethodReference method, TypeReference enumType)
            {
                if (method.ReturnType.FullName == SystemEnumName)
                    method.ReturnType = enumType;
                foreach (var p in method.Parameters)
                    if (p.ParameterType.FullName == SystemEnumName)
                        p.ParameterType = enumType;
            }
        }
    }
}