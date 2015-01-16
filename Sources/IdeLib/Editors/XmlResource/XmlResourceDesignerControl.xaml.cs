using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Threading;
using Dot42.Ide.Controls;
using Dot42.ResourcesLib;

namespace Dot42.Ide.Editors.XmlResource
{
	/// <summary>
	/// Root control for XML resource designer
	/// </summary>
	public partial class XmlResourceDesignerControl : IDesignerControl
	{
		/// <summary>
		/// Default ctor
		/// </summary>
		public XmlResourceDesignerControl()
			: this(null)
		{
		}

		/// <summary>
		/// Default ctor
		/// </summary>
		public XmlResourceDesignerControl(IResourceViewModel viewModel)
		{
			if (viewModel != null)
			{
				DataContext = viewModel;
			}
			InitializeComponent();
			if (viewModel != null)
			{
				viewModel.PropertyChanged += OnViewModelPropertyChanged;
			}
		}

		/// <summary>
		/// Is this control is edit mode?
		/// </summary>
		public bool IsEditing
		{
			get
			{
				return
					boolsGrid.IsEditing() ||
					colorsGrid.IsEditing() ||
					dimensionsGrid.IsEditing() ||
					idsGrid.IsEditing() ||
					integersGrid.IsEditing() ||
					integerArraysGrid.IsEditing() ||
					stringsGrid.IsEditing() ||
					stringArraysGrid.IsEditing() ||
					pluralsGrid.IsEditing() ||
					stylesGrid.IsEditing() ||
					typedArraysGrid.IsEditing();
			}
		}

		/// <summary>
		/// Some property in the view model has changed.
		/// </summary>
		void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			DoIdle(true);
		}
		
		/// <summary>
		/// Update model
		/// </summary>
		public void DoIdle()
		{
			DoIdle(false);
		}

		/// <summary>
		/// Update model
		/// </summary>
		private void DoIdle(bool force)
		{
			// only call the view model DoIdle if this control has focus
			// otherwise, we should skip and this will be called again once focus is regained
			var viewModel = DataContext as IViewModel;
			if ((viewModel != null) && (IsKeyboardFocusWithin || force) && !IsEditing)
			{
				viewModel.DoIdle();
			}
		}

		/// <summary>
		/// Gets all possible bool values
		/// </summary>
		[Obfuscation(Feature = "@Xaml")]
		public static string[] BoolValues { get { return ResourceConstants.BoolValues; } }

		/// <summary>
		/// Gets all possible bool values
		/// </summary>
		[Obfuscation(Feature = "@Xaml")]
		public static string[] PluralsQuantityValues { get { return ResourceConstants.PluralsQuantityValues; } }
	}
}
