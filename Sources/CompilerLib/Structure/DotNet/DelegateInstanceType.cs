using Dot42.CompilerLib.XModel;
using Dot42.DexLib;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Holder for info regarding a single .NET delegate method.
    /// </summary>
    internal sealed class DelegateInstanceType
    {
        private readonly XMethodDefinition calledMethod;
        private readonly ClassDefinition instanceDefinition;
        private readonly Dot42.DexLib.MethodDefinition instanceCtor;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DelegateInstanceType(XMethodDefinition calledMethod, ClassDefinition instanceDefinition, Dot42.DexLib.MethodDefinition instanceCtor)
        {
            this.calledMethod = calledMethod;
            this.instanceDefinition = instanceDefinition;
            this.instanceCtor = instanceCtor;
        }

        /// <summary>
        /// The ctor of the instance definition.
        /// </summary>
        public Dot42.DexLib.MethodDefinition InstanceCtor
        {
            get { return instanceCtor; }
        }

        /// <summary>
        /// Gets the class that implements the delegate interface to call our called method.
        /// </summary>
        public ClassDefinition InstanceDefinition
        {
            get { return instanceDefinition; }
        }

        /// <summary>
        /// Gets the .NET method being called in this method
        /// </summary>
        public XMethodDefinition CalledMethod
        {
            get { return calledMethod; }
        }

        public bool ConstructorNeedsInstanceArgument
        {
            get { return !calledMethod.IsStatic; }
        }
    }
}
