using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Reachable;
using Mono.Cecil;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// Create static ctors for types that have reachable static fields of type struct.
    /// </summary>
    [Export(typeof (ILConverterFactory))]
    internal class InitializeStaticStructFields : ILConverterFactory
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
            /// <summary>
            /// Create default ctors for reachable structs.
            /// </summary>
            public void Convert(ReachableContext reachableContext)
            {
                foreach (var type in reachableContext.ReachableTypes)
                {
                    List<FieldDefinition> structFields;
                    if (NeedsClassCtor(type, out structFields))
                    {
                        var ctor = type.GetClassCtor();
                        if (ctor == null)
                        {
                            throw new CompilerException(string.Format("Class ctor not found in type {0}", type.FullName));
                        }
                        InjectInitializationCode(ctor, structFields);
                    }
                }
            }

            /// <summary>
            /// Does the given type have any type that is a struct?
            /// </summary>
            private static bool NeedsClassCtor(TypeDefinition type, out List<FieldDefinition> structFields)
            {
                structFields = null;
                if (type.IsEnum) // Class ctor is already created automatically for enums
                    return false;
                if (type.Name.StartsWith("<PrivateImplementationDetails>"))
                    return false;
                foreach (var field in type.Fields.Where(x => x.IsReachable && x.IsStatic && StructFields.IsStructField(x)))
                {
                    if (structFields == null)
                        structFields = new List<FieldDefinition>();
                    structFields.Add(field);
                }
                return (structFields != null);
            }

            /// <summary>
            /// Inject initialization code to the given ctor
            /// </summary>
            private static void InjectInitializationCode(MethodDefinition ctor, IEnumerable<FieldDefinition> structFields)
            {
                // Create sequence
                var initSeq = StructFields.CreateInitializationCode(structFields, true);

                // Find location where to insert
                var body = ctor.Body;
                // Insert at start of method
                initSeq.InsertTo(0, body);
            }
        }
    }    
}