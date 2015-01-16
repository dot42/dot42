using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.ImportJarLib.Extensions
{
    partial class Cecil
    {
        /// <summary>
        /// Create a clone of the given method definition
        /// </summary>
        public static MethodDefinition Clone(this MethodDefinition source)
        {
            var result = new MethodDefinition(source.Name, source.Attributes, source.ReturnType) {
                ImplAttributes = source.ImplAttributes,
                SemanticsAttributes = source.SemanticsAttributes,
                HasThis = source.HasThis,
                ExplicitThis = source.ExplicitThis,
                CallingConvention = source.CallingConvention
            };
            foreach (var p in source.Parameters)
            {
                result.Parameters.Add(p.Clone());
            }
            if (source.HasBody)
            {
                result.Body = source.Body.Clone(result);
            }
            CloneCustomAttributes(result, source);
            return result;
        }

        /// <summary>
        /// Create a clone of the given method body
        /// </summary>
        public static MethodBody Clone(this MethodBody source, MethodDefinition target)
        {
            var result = new MethodBody(target) { InitLocals = source.InitLocals, MaxStackSize = source.MaxStackSize };
            var worker = result.GetILProcessor();
            foreach (var i in source.Instructions)
            {
                // Poor mans clone, but sufficient for our needs
                var clone = Instruction.Create(OpCodes.Nop);
                clone.OpCode = i.OpCode;
                clone.Operand = i.Operand;
                worker.Append(clone);
            }
            return result;
        }

        /// <summary>
        /// Create a clone of the given parameter definition
        /// </summary>
        public static ParameterDefinition Clone(this ParameterDefinition source)
        {
            var result = new ParameterDefinition(source.Name, source.Attributes, source.ParameterType);
            CloneCustomAttributes(result, source);
            return result;
        }

        /// <summary>
        /// Clone all attributes from source to target.
        /// </summary>
        private static void CloneCustomAttributes(ICustomAttributeProvider target, ICustomAttributeProvider source)
        {
            if (!source.HasCustomAttributes)
                return;

            foreach (var x in source.CustomAttributes)
            {
                var clone = new CustomAttribute(x.Constructor);
                if (x.HasConstructorArguments)
                {
                    CloneCAArguments(clone.ConstructorArguments, x.ConstructorArguments);
                }
                if (x.HasFields)
                    CloneCANamedArguments(clone.Fields, x.Fields);
                if (x.HasProperties)
                    CloneCANamedArguments(clone.Properties, x.Properties);
                target.CustomAttributes.Add(clone);
            }
        }

        /// <summary>
        /// Clone all arguments from source to target.
        /// </summary>
        private static void CloneCANamedArguments(IList<CustomAttributeNamedArgument> target, IList<CustomAttributeNamedArgument> source)
        {
            foreach (var x in source)
            {
                var clone = new CustomAttributeNamedArgument(
                    x.Name,
                    CloneCAArgument(x.Argument));
                target.Add(clone);
            }
        }

        /// <summary>
        /// Clone all arguments from source to target.
        /// </summary>
        private static void CloneCAArguments(IList<CustomAttributeArgument> target, IList<CustomAttributeArgument> source)
        {
            foreach (var x in source)
            {
                target.Add(CloneCAArgument(x));
            }
        }

        /// <summary>
        /// Clone all arguments from source to target.
        /// </summary>
        private static CustomAttributeArgument CloneCAArgument(CustomAttributeArgument source)
        {
            var clonedValue = source.Value;
            return new CustomAttributeArgument(source.Type, clonedValue);
        }


    }
}
