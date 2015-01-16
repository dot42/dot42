using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Dot42.DebuggerLib.Model;
using ICSharpCode.NRefactory;
using ICSharpCode.SharpDevelop.Debugging;
using ICSharpCode.SharpDevelop.Gui.Pads;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.TreeView;
using TallComponents.Common.Extensions;

namespace Dot42.SharpDevelop.Pads.Controls
{
	internal sealed class DebugValueProperty : DebugProperty
	{
		private const int MaxArrayValues = 1024 * 4;
		private readonly DalvikValue value;
		
		public DebugValueProperty(DalvikValue value)
		{
			if (value == null)
				throw new ArgumentNullException("value");
			this.value = value;
			LazyLoading = true;
		}

		protected override string GetValueAsString()
		{
			if (value.IsString && !value.ObjectReference.IsNull)
			{
				return value.ObjectReference.GetStringValueAsync().Await(DalvikProcess.VmTimeout);
			}
			else if (value.IsArray && !value.ObjectReference.IsNull)
			{
				return "(array)";
			}
			else
			{
				return value.Value.ToString();
			}
		}
		
		
		/// <summary>
		/// Gets the type of the value to display.
		/// </summary>
		protected override string GetTypeAsString() {
			if (value.IsPrimitive)
			{
				return value.Value.GetType().Name;
			}
			else if (value.IsString)
			{
				return typeof (string).Name;
			}
			else if (!value.ObjectReference.IsNull)
			{
				return value.ObjectReference.GetReferenceTypeAsync().Select(t => t.GetNameAsync()).Unwrap().Await(DalvikProcess.VmTimeout);
			}
			else
			{
				return typeof (object).Name;
			}
		}

		protected override string GetName() {
			return value.Name; 
		}

		protected override bool HasChildren {
			get { return !(value.IsPrimitive || value.IsString); }
		}
		
		protected override void LoadChildren()
		{
			if (value.IsPrimitive || value.ObjectReference.IsNull)
				return;

			if (value.IsArray)
			{
				// Get length
				var length = value.ObjectReference.GetArrayLengthAsync().Await(DalvikProcess.VmTimeout);
				Children.Add(new DebugArrayLengthProperty(length));

				// Get elements
				var firstIndex = 0;
				while (length > 0)
				{
					var chunkLength = Math.Min(length, MaxArrayValues);
					var elements = value.ObjectReference.GetArrayValuesAsync(firstIndex, chunkLength).Await(DalvikProcess.VmTimeout);
					Children.AddRange(elements.Select(x => new DebugValueProperty(x)));
					firstIndex += chunkLength;
					length -= chunkLength;
				}
			}
			else
			{
				// Get instance field values
				var refType = value.ObjectReference.GetReferenceTypeAsync().Await(DalvikProcess.VmTimeout);
				var fieldValues = refType.GetInstanceFieldValuesAsync(value.ObjectReference).Await(DalvikProcess.VmTimeout);
				Children.AddRange(fieldValues.Select(x => new DebugValueProperty(x)));

				// Get base class
				var superClass = refType.GetSuperClassAsync().Await(DalvikProcess.VmTimeout);
				if (superClass != null)
				{
					Children.Insert(0, new DebugBaseClassProperty(value.ObjectReference, superClass));
				}
			}
		}
	}
}
