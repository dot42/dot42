
using System;
using System.Reflection;
using ICSharpCode.TreeView;

namespace Dot42.SharpDevelop.Pads.Controls
{
	/// <summary>
	/// Description of DebugProperty.
	/// </summary>
	public abstract class DebugProperty : SharpTreeNode
	{	
		/// <summary>
		/// Value of the property
		/// </summary>
        [Obfuscation(Feature = "@Xaml")]
		public object Value { get { return GetValueAsString(); } }
		
		/// <summary>
		/// Type of the property value
		/// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public string Type { get { return GetTypeAsString(); } }

        /// <summary>
        /// Name of the property
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public override object Text { get { return GetName(); } }
        
        [Obfuscation(Feature = "@Xaml")]
		public virtual bool CanSetText { get{return false; }}
		
        [Obfuscation(Feature = "@Xaml")]
		public override object Icon {
			get { return null; /* Node.ImageSource; */}
		}
		
        [Obfuscation(Feature = "@Xaml")]
		public override sealed bool ShowExpander {
			get { return HasChildren; }
		}
		
        /// <summary>
        /// Does this property have children
        /// </summary>
        protected abstract bool HasChildren { get; }

        /// <summary>
        /// Is deleting this node possible?
        /// </summary>
		public override bool CanDelete()
		{
			return false;
		}
		
		/// <summary>
		/// Remove this node from it's parent.
		/// </summary>
		public override void Delete()
		{
			Parent.Children.Remove(this);
		}
        
        /// <summary>
        /// Gets the value to display.
        /// </summary>
        protected abstract string GetValueAsString();
        
        /// <summary>
        /// Gets the type of the value to display.
        /// </summary>
        protected abstract string GetTypeAsString();
        
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        protected abstract string GetName();
	}
}
