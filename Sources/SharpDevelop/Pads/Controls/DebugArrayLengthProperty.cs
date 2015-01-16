using System;
using System.Reflection;
using ICSharpCode.TreeView;

namespace Dot42.SharpDevelop.Pads.Controls
{
	/// <summary>
	/// Description of DebugArrayLengthProperty.
	/// </summary>
	internal sealed class DebugArrayLengthProperty : DebugProperty
	{
		private readonly int length;
		
		/// <summary>
		/// Default ctor
		/// </summary>
		public DebugArrayLengthProperty(int length)
		{
			this.length = length;
		}
		
		protected override string GetValueAsString()
		{
			return length.ToString();
		}
		
		/// <summary>
		/// Gets the type of the value to display.
		/// </summary>
		protected override string GetTypeAsString() {
			return typeof(int).FullName;
		}

		protected override string GetName() {
			return "Length";
		}
		
		protected override bool HasChildren {
			get { return false;	}
		}
	}
}
