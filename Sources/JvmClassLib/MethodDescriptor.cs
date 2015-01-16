using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dot42.JvmClassLib
{
    /// <summary>
    /// Descriptor of a method containing parameter type information and return type information.
    /// </summary>
    public class MethodDescriptor
    {
        private readonly ReadOnlyCollection<TypeReference> parameters;
        private readonly TypeReference returnType;
        private int? slots;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal MethodDescriptor(TypeReference returnType, IEnumerable<TypeReference> parameters)
        {
            this.returnType = returnType;
            this.parameters = parameters.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the parameter types
        /// </summary>
        public ReadOnlyCollection<TypeReference> Parameters { get { return parameters; } }

        /// <summary>
        /// Gets the number of local variable slots (or indexes) are needed to store the parameters of this method.
        /// </summary>
        /// <remarks>
        /// The result does not include a slot for the "this" parameter, only for the normal parameters.
        /// </remarks>
        public int GetLocalVariableSlots()
        {
            if (!slots.HasValue)
            {
                slots = parameters.Sum(x => x.IsWide ? 2 : 1);
            }
            return slots.Value;
        }

        /// <summary>
        /// Gets the return type
        /// </summary>
        public TypeReference ReturnType { get { return returnType; } }
    }
}
