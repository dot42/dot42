using System;
using System.Linq;
using Dot42.DebuggerLib.Model;
using TallComponents.Common.Extensions;

namespace Dot42.SharpDevelop.Pads.Controls
{
	/// <summary>
	/// Description of DebugBaseClassProperty.
	/// </summary>
	internal sealed class DebugBaseClassProperty : DebugProperty
	{
		private readonly DalvikObjectReference objectReference;
		private readonly DalvikReferenceType superClass;

		/// <summary>
		/// Default ctor
		/// </summary>
		public DebugBaseClassProperty(DalvikObjectReference objectReference, DalvikReferenceType superClass)
		{
			this.objectReference = objectReference;
			this.superClass = superClass;
			LazyLoading = true;
		}

		/// <summary>
		/// Does this property have children
		/// </summary>
		protected override bool HasChildren
		{
			get { return true; }
		}
		
		protected override string GetValueAsString()
		{
			return string.Empty;
		}
		
		protected override string GetTypeAsString()
		{
			return string.Empty;
		}

		/// <summary>
		/// Create all child properties
		/// </summary>
		protected override void LoadChildren()
		{
			var fieldValues = superClass.GetInstanceFieldValuesAsync(objectReference).Await(DalvikProcess.VmTimeout);
			Children.AddRange(fieldValues.Select(x => new DebugValueProperty(x)));
			
			// Get base class
            if (superClass.GetNameAsync().Await(DalvikProcess.VmTimeout) != "java.lang.Object")
            {
                var superSuperClass = superClass.GetSuperClassAsync().Await(DalvikProcess.VmTimeout);
                if (superSuperClass != null)
                {
                    Children.Insert(0, new DebugBaseClassProperty(objectReference, superSuperClass));
                }
            }
		}
		
		/// <summary>
		/// Gets the name of this property.
		/// </summary>
		protected override string GetName() {
			return "[" + superClass.GetNameAsync().Await(DalvikProcess.VmTimeout) + "]";
		}
	}
}
