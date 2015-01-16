using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Dot42.Graphics;
using Dot42.Ide.Serialization;
using Dot42.Ide.Serialization.Nodes.Menu;
using Dot42.Utility;

namespace Dot42.Ide.Editors.Menu
{
    /// <summary>
    /// Root control for XML menu designer
    /// </summary>
    public partial class XmlMenuDesignerControl : IDesignerControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private SerializationNode editingNode;
        private TextBox editTextBox;
        private string oldText;
        private readonly IIdeSelectionContainer selectionContainer;

        /// <summary>
        /// Default ctor
        /// </summary>
        public XmlMenuDesignerControl()
            : this(null, null, null)
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public XmlMenuDesignerControl(IIde ide, IServiceProvider serviceProvider, IMenuViewModel viewModel)
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
        }

        /// <summary>
        /// Some property in the view model has changed.
        /// </summary>
        void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DoIdle(true);
        }

        /// <summary>
        /// Is this control is edit mode?
        /// </summary>
        public bool IsEditing
        {
            get { return (editingNode != null); }
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
        /// Set node in edit mode
        /// </summary>
        private void OnTitlePreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var block = sender as TextBlock;
            if ((e.LeftButton == MouseButtonState.Pressed) && (e.ClickCount == 2) && (block != null))
            {
                // Edit node
                var node = block.Tag as SerializationNode;
                if (BeginEditing(node))
                {
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handle treeview key events
        /// </summary>
        private void MenuTreeView_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                var item = MenuTreeView.SelectedItem as SerializationNode;
                BeginEditing(item);
                e.Handled = true;
            }
            else if (((e.Key == Key.Escape) || (e.Key == Key.Cancel)) && (editingNode != null))
            {
                editingNode.CancelEdit();
                editingNode = null;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Commit changes on Enter
        /// </summary>
        private void OnEditableTitleKeyDown(object sender, KeyEventArgs e)
        {
            var box = sender as TextBox;
            if (box != null)
            {
                if ((e.Key == Key.Enter) || (e.Key == Key.Return))
                {
                    EndEditNode(true);
                }
                else if ((e.Key == Key.Cancel) || (e.Key == Key.Escape))
                {
                    EndEditNode(false);
                }
            }
        }

        /// <summary>
        /// Focus edit textbox on becoming visible.
        /// </summary>
        private void OnEditableTitleIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var box = sender as TextBox;
            if ((box != null) && box.IsVisible)
            {
                editTextBox = box;
                oldText = box.Text;
                box.Focus();
                box.SelectAll();
            }
        }

        /// <summary>
        /// End editing when selection changes
        /// </summary>
        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            EndEditNode(true);
            selectionContainer.SelectedObject = e.NewValue;
            UpdateToolbar();
        }

        /// <summary>
        /// End editing on lost focus
        /// </summary>
        private void OnTreeViewLostFocus(object sender, RoutedEventArgs e)
        {
            EndEditNode(true);
        }

        /// <summary>
        /// End editing the currently editing node.
        /// </summary>
        private bool BeginEditing(SerializationNode node)
        {
            EndEditNode(false);
            if (node != null)
            {
                node.BeginEdit();
                editingNode = node;
                return true;
            }
            return false;
        }

        /// <summary>
        /// End editing the currently editing node.
        /// </summary>
        private void EndEditNode(bool commit)
        {
            if (editingNode != null)
            {
                if (commit)
                    editingNode.EndEdit();
                else
                {
                    if (editTextBox != null)
                        editTextBox.Text = oldText;
                    editingNode.CancelEdit();
                }
                editingNode = null;
                editTextBox = null;
                oldText = null;
            }
        }

        /// <summary>
        /// Add a new menu item as sibling
        /// </summary>
        private void OnAddSiblingItemClick(object sender, RoutedEventArgs e)
        {
            var container = GetSiblingContainer();
            var newItem = new MenuItemNode { Title = "Menu item" };
            newItem.InitializeWithDefaultValues();
            container.Add(newItem);
            FocusItem(newItem, true);
        }

        /// <summary>
        /// Add a new menu item as child
        /// </summary>
        private void OnAddChildItemClick(object sender, RoutedEventArgs e)
        {
            var container = GetChildContainer();
            var newItem = new MenuItemNode { Title = "Menu item" };
            newItem.InitializeWithDefaultValues();
            container.Add(newItem);
            FocusItem(newItem, true);
        }

        /// <summary>
        /// Add a new menu group as sibling
        /// </summary>
        private void OnAddSiblingGroupClick(object sender, RoutedEventArgs e)
        {
            var container = GetSiblingContainer();
            var newItem = new MenuGroupNode();
            newItem.InitializeWithDefaultValues();
            container.Add(newItem);
            FocusItem(newItem, false);
        }

        /// <summary>
        /// Add a new menu group as child
        /// </summary>
        private void OnAddChildGroupClick(object sender, RoutedEventArgs e)
        {
            var container = GetChildContainer();
            var newItem = new MenuGroupNode();
            newItem.InitializeWithDefaultValues();
            container.Add(newItem);
            FocusItem(newItem, false);
        }

        /// <summary>
        /// Remove the selected node
        /// </summary>
        private void OnRemoveClick(object sender, RoutedEventArgs e)
        {
            var selection = MenuTreeView.SelectedItem as MenuChildNode;
            if ((selection == null) || (selection.Container == null))
                return;
            // Now remove
            selection.Container.Remove(selection);
            UpdateToolbar();
        }

        /// <summary>
        /// Move selected item up.
        /// </summary>
        private void OnUpClick(object sender, RoutedEventArgs e)
        {
            var selection = SelectedNode;
            var container = GetSiblingContainer();
            if ((selection == null) || (container == null))
                return;
            var oldIndex = container.IndexOf(selection);
            if (oldIndex > 0)
            {
                container.MoveTo(selection, oldIndex - 1);
                FocusItem(selection, false);
            }
        }

        /// <summary>
        /// Move selected item down.
        /// </summary>
        private void OnDownClick(object sender, RoutedEventArgs e)
        {
            var selection = SelectedNode;
            var container = GetSiblingContainer();
            if ((selection == null) || (container == null))
                return;
            var oldIndex = container.IndexOf(selection);
            if (oldIndex < container.Count - 1)
            {
                container.MoveTo(selection, oldIndex + 1);
                FocusItem(selection, false);
            }
        }

        /// <summary>
        /// Move selected item left.
        /// </summary>
        private void OnLeftClick(object sender, RoutedEventArgs e)
        {
            var selection = SelectedNode;
            var container = (selection != null) ? selection.Container : null;
            var parent = (selection != null) ? selection.Parent : null;
            var parentContainer = (parent != null) ? parent.Container as IMenuChildNodeContainer : null;
            if ((selection == null) || (container == null) || (parent == null) || (parentContainer == null) || !parentContainer.CanAdd(selection))
                return;
            var parentIndex = parentContainer.IndexOf(parent);
            container.Remove(selection);
            parentContainer.Add(selection);
            parentContainer.MoveTo(selection, parentIndex + 1);
            FocusItem(selection, false);
        }

        /// <summary>
        /// Move selected item right.
        /// </summary>
        private void OnRightClick(object sender, RoutedEventArgs e)
        {
            var selection = SelectedNode;
            var container = (selection != null) ? selection.Container as IMenuChildNodeContainer : null;
            var index = (container != null) ? container.IndexOf(selection) : -1;
            if (index <= 0)
                return;
            var newParent = container[index - 1] as MenuChildNode;
            if ((newParent == null) || (!newParent.ChildContainer.CanAdd(selection)))
                return;
            container.Remove(selection);
            newParent.ChildContainer.Add(selection);
            FocusItem(selection, false);
        }

        /// <summary>
        /// Can the current item be removed?
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public bool CanRemoveItem
        {
            get
            {
                var selection = SelectedNode;
                return
                    (selection is MenuItemNode) ||
                    (selection is MenuGroupNode);
            }
        }

        /// <summary>
        /// Can a group node be added as sibling of the current item?
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public bool CanAddSiblingGroupItem
        {
            get
            {
                var container = GetSiblingContainer();
                return (container != null) && container.CanAddGroupNodes;
            }
        }

        /// <summary>
        /// Can a group node be added as child of the current item?
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public bool CanAddChildGroupItem
        {
            get
            {
                var container = GetChildContainer();
                return (container != null) && container.CanAddGroupNodes;
            }
        }

        /// <summary>
        /// Can the selected node be moved up?
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public bool CanMoveUp
        {
            get
            {
                var selection = SelectedNode;
                var container = GetSiblingContainer();
                if ((selection == null) || (container == null))
                    return false;
                var oldIndex = container.IndexOf(selection);
                return (oldIndex > 0);
            }
        }

        /// <summary>
        /// Can the selected node be moved down?
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public bool CanMoveDown
        {
            get
            {
                var selection = SelectedNode;
                var container = GetSiblingContainer();
                if ((selection == null) || (container == null))
                    return false;
                var oldIndex = container.IndexOf(selection);
                return (oldIndex < container.Count - 1);
            }
        }

        /// <summary>
        /// Can the selected node be moved left?
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public bool CanMoveLeft
        {
            get
            {
                var selection = SelectedNode;
                var container = (selection != null) ? selection.Container : null;
                var parent = (selection != null) ? selection.Parent : null;
                var parentContainer = (parent != null) ? parent.Container as IMenuChildNodeContainer : null;
                return ((selection != null) && (container != null) && (parent != null) && (parentContainer != null) && parentContainer.CanAdd(selection));
            }
        }

        /// <summary>
        /// Can the selected node be moved right?
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public bool CanMoveRight
        {
            get
            {
                var selection = SelectedNode;
                var container = (selection != null) ? selection.Container as IMenuChildNodeContainer : null;
                var index = (container != null) ? container.IndexOf(selection) : -1;
                if (index <= 0)
                    return false;
                var newParent = container[index - 1] as MenuChildNode;
                return ((newParent != null) && newParent.ChildContainer.CanAdd(selection));
            }
        }

        /// <summary>
        /// Gets the selected node.
        /// </summary>
        private MenuChildNode SelectedNode
        {
            get { return (MenuTreeView != null) ? MenuTreeView.SelectedItem as MenuChildNode : null; }
        }

        /// <summary>
        /// Gets the containing to add sibling nodes to.
        /// </summary>
        private IMenuChildNodeContainer GetSiblingContainer()
        {
            var selection = SelectedNode;
            var viewModel = (IMenuViewModel)DataContext;
            return (selection != null) ? selection.Container as IMenuChildNodeContainer : viewModel.Menu.Children;
        }

        /// <summary>
        /// Gets the containing to add child nodes to.
        /// </summary>
        private IMenuChildNodeContainer GetChildContainer()
        {
            var selection = SelectedNode;
            var viewModel = (IMenuViewModel)DataContext;
            return (selection != null) ? selection.ChildContainer : viewModel.Menu.Children;
        }

        /// <summary>
        /// Set focus to the given item and optional start editing.
        /// </summary>
        private void FocusItem(SerializationNode node, bool edit)
        {
            // Select node
            node.IsSelected = true;
            // Focus treeview
            Keyboard.Focus(MenuTreeView);
            UpdateToolbar();
            if (edit)
            {
                Action action = () => BeginEditing(node);
                Dispatcher.BeginInvoke(action, DispatcherPriority.ApplicationIdle);
            }
        }

        /// <summary>
        /// Update the state of the toolbar.
        /// </summary>
        private void UpdateToolbar()
        {
            OnPropertyChanged(ReflectionHelper.PropertyOf(() => CanRemoveItem).Name, false);
            OnPropertyChanged(ReflectionHelper.PropertyOf(() => CanAddChildGroupItem).Name, false);
            OnPropertyChanged(ReflectionHelper.PropertyOf(() => CanAddSiblingGroupItem).Name, false);
            OnPropertyChanged(ReflectionHelper.PropertyOf(() => CanMoveUp).Name, false);
            OnPropertyChanged(ReflectionHelper.PropertyOf(() => CanMoveDown).Name, false);
            OnPropertyChanged(ReflectionHelper.PropertyOf(() => CanMoveLeft).Name, false);
            OnPropertyChanged(ReflectionHelper.PropertyOf(() => CanMoveRight).Name, false);
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

        public static ImageSource ArrowUp { get { return WpfIcons16.ArrowUp; } }
        public static ImageSource ArrowDown { get { return WpfIcons16.ArrowDown; } }
        public static ImageSource ArrowLeft { get { return WpfIcons16.ArrowLeft; } }
        public static ImageSource ArrowRight { get { return WpfIcons16.ArrowRight; } }
    }
}
