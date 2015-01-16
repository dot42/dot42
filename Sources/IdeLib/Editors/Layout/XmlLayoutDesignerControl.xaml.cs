using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Dot42.Ide.Editors.Layout
{
    /// <summary>
    /// Root control for XML layout designer
    /// </summary>
    public partial class XmlLayoutDesignerControl : IDesignerControl, INotifyPropertyChanged, IXmlLayoutDesigner
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IIdeSelectionContainer selectionContainer;
        private readonly List<IViewNodeControl> selectedControls = new List<IViewNodeControl>();

        /// <summary>
        /// Default ctor
        /// </summary>
        public XmlLayoutDesignerControl()
            : this(null, null, null)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public XmlLayoutDesignerControl(IIde ide, IServiceProvider serviceProvider, ILayoutViewModel viewModel)
        {
            if (ide != null)
            {
                selectionContainer = ide.CreateSelectionContainer(serviceProvider);
            }
            if (viewModel != null)
            {
                DataContext = viewModel;
            }
            InitializeComponent();
            // wait until we're initialized to handle events
            if (viewModel != null)
            {
                viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
            ViewModelChanged(this, EventArgs.Empty);
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
            if ((viewModel != null) && (IsKeyboardFocusWithin || force) /*&& !IsEditing*/)
            {
                viewModel.DoIdle();
            }
        }

        /// <summary>
        /// The view model was changed. Update our contents
        /// </summary>
        private void ViewModelChanged(object sender, EventArgs e)
        {
            // this gets called when the view model is updated because the Xml Document was updated
            // since we don't get individual PropertyChanged events, just re-set the DataContext
            var viewModel = DataContext as ILayoutViewModel;
            DataContext = null; // first, set to null so that we see the change and rebind
            DataContext = viewModel;

            var rootNode = (viewModel != null) ? viewModel.Root : null;
            var rootControl = (rootNode != null) ? rootNode.Accept(ControlBuilder.Instance, this) : null;
            rootContainer.Content = rootControl;
        }

        /// <summary>
        /// Select the given control
        /// </summary>
        void IXmlLayoutDesigner.Select(IViewNodeControl control)
        {
            foreach (var c in selectedControls)
            {
                c.Node.IsSelected = false;
            }
            selectedControls.Clear();
            selectionContainer.SelectedObject = (control != null) ? control.Node : null;
            if (control != null)
            {
                selectedControls.Add(control);
                control.Node.IsSelected = true;
            }
        }

        /// <summary>
        /// Fire property changed events
        /// </summary>
        private void OnPropertyChanged(string propertyName, bool isModelProperty)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, isModelProperty ? new PropertyChangedExEventArgs(propertyName, true) : new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
