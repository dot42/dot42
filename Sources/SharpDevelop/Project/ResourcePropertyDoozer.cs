using System;
using System.ComponentModel;
using System.Linq;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace Dot42.SharpDevelop.Project
{
	/// <summary>
	/// Description of ResourcePropertyDoozer.
	/// </summary>
	public class ResourcePropertyDoozer : IDoozer
	{
		public ResourcePropertyDoozer()
		{
		}
		
		public bool HandleConditions {
			get {
				return false;
			}
		}
		
		public object BuildItem(BuildItemArgs args)
		{
			var codon = args.Codon;
			var name = codon.Properties["name"];
			var extender = new ResourceExtender((FileProjectItem)args.Caller);
			var descriptor = TypeDescriptor.GetProperties(extender).Cast<PropertyDescriptor>().FirstOrDefault(x => x.Name == name);
			if (descriptor == null)
				throw new ArgumentException("Unknown ResourceExtender property " + name);
			return new ResourceExtenderProperty(descriptor, extender);
		}
		
		private class ResourceExtenderProperty : PropertyDescriptor {
			
			private readonly PropertyDescriptor descriptor;
			private readonly ResourceExtender extender;
			
			internal ResourceExtenderProperty(PropertyDescriptor descriptor, ResourceExtender extender) : base(descriptor) {
				this.descriptor = descriptor;
				this.extender = extender;
			}
			
			public override bool ShouldSerializeValue(object component)
			{
				return descriptor.ShouldSerializeValue(extender);
			}
			
			public override void SetValue(object component, object value)
			{
				descriptor.SetValue(extender, value);
			}
			
			public override void ResetValue(object component)
			{
				descriptor.ResetValue(extender);
			}
			
			public override Type PropertyType {
				get {
					return descriptor.PropertyType;
				}
			}
			
			public override bool IsReadOnly {
				get {
					return descriptor.IsReadOnly;
				}
			}
			
			public override object GetValue(object component)
			{
				return descriptor.GetValue(extender);
			}
			
			public override Type ComponentType {
				get {
					return descriptor.ComponentType;
				}
			}
			
			public override bool CanResetValue(object component)
			{
				return descriptor.CanResetValue(extender);
			}
		}
	}
}
