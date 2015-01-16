using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Dot42.ApkLib;
using Dot42.Ide.TypeConverters;
using Dot42.Utility;

namespace Dot42.Ide.Serialization.Nodes.Menu
{
    /// <summary>
    /// menu/item
    /// </summary>
    [ElementName("item", null, "menu", "group")]
    [Obfuscation(Feature = "@SerializableNode")]
    [Description("Menu item")]
    [Obfuscation(Feature = "@VSNode")]
    public sealed class MenuItemNode : MenuChildNode, ISerializationNodeContainer
    {
        private readonly MenuNode menu;
        private readonly PropertyInfo titleProperty;
        private readonly PropertyInfo titleCondensedProperty;
        private readonly PropertyInfo iconProperty;
        private readonly PropertyInfo onClickProperty;
        private readonly PropertyInfo showAsActionProperty;
        private readonly PropertyInfo actionLayoutProperty;
        private readonly PropertyInfo actionViewClassProperty;
        private readonly PropertyInfo actionProviderClassProperty;
        private readonly PropertyInfo alphabeticShortcutProperty;
        private readonly PropertyInfo numericShortcutProperty;
        private readonly PropertyInfo checkableProperty;

        /// <summary>
        /// Default ctor
        /// </summary>
        public MenuItemNode()
        {
            menu = new MenuNode(this);
            menu.PropertyChanged += (s, x) => OnPropertyChanged("Menu." + x.PropertyName, x);
            titleProperty = ReflectionHelper.PropertyOf(() => Title);
            titleCondensedProperty = ReflectionHelper.PropertyOf(() => TitleCondensed);
            iconProperty = ReflectionHelper.PropertyOf(() => Icon);
            onClickProperty = ReflectionHelper.PropertyOf(() => OnClick);
            showAsActionProperty = ReflectionHelper.PropertyOf(() => ShowAsAction);
            actionLayoutProperty = ReflectionHelper.PropertyOf(() => ActionLayout);
            actionViewClassProperty = ReflectionHelper.PropertyOf(() => ActionViewClass);
            actionProviderClassProperty = ReflectionHelper.PropertyOf(() => ActionProviderClass);
            alphabeticShortcutProperty = ReflectionHelper.PropertyOf(() => AlphabeticShortcut);
            numericShortcutProperty = ReflectionHelper.PropertyOf(() => NumericShortcut);
            checkableProperty = ReflectionHelper.PropertyOf(() => Checkable);
        }

        /// <summary>
        /// Gets the containing for adding child items to.
        /// </summary>
        [Browsable(false)]
        public override IMenuChildNodeContainer ChildContainer { get { return menu.Children; } }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TReturn Accept<TReturn, TData>(SerializationNodeVisitor<TReturn, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the submenu
        /// </summary>
        [Browsable(false)]
        [Obfuscation(Feature = "@Xaml")]
        public MenuNode Menu
        {
            get { return menu; }
        }

        /// <summary>
        /// android:title
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("title", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        public string Title
        {
            get { return Get(titleProperty); }
            set { Set(titleProperty, value); }
        }

        /// <summary>
        /// android:titleCondensed
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("titleCondensed", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        public string TitleCondensed
        {
            get { return Get(titleCondensedProperty); }
            set { Set(titleCondensedProperty, value); }
        }

        /// <summary>
        /// android:icon
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("icon", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        public string Icon
        {
            get { return Get(iconProperty); }
            set { Set(iconProperty, value); }
        }

        /// <summary>
        /// android:onClick
        /// </summary>
        [Category(Categories.Events)]
        [AttributeName("onClick", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        public string OnClick
        {
            get { return Get(onClickProperty); }
            set { Set(onClickProperty, value); }
        }

        /// <summary>
        /// android:showAsAction
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("showAsAction", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        [TypeConverter(typeof(ShowAsActionTypeConverter))]
        public string ShowAsAction
        {
            get { return Get(showAsActionProperty); }
            set { Set(showAsActionProperty, value); }
        }

        /// <summary>
        /// android:actionLayout
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("actionLayout", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        public string ActionLayout
        {
            get { return Get(actionLayoutProperty); }
            set { Set(actionLayoutProperty, value); }
        }

        /// <summary>
        /// android:actionViewClass
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("actionViewClass", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        public string ActionViewClass
        {
            get { return Get(actionViewClassProperty); }
            set { Set(actionViewClassProperty, value); }
        }

        /// <summary>
        /// android:actionProviderClass
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("actionProviderClass", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        public string ActionProviderClass
        {
            get { return Get(actionProviderClassProperty); }
            set { Set(actionProviderClassProperty, value); }
        }

        /// <summary>
        /// android:alphabeticShortcut
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("alphabeticShortcut", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        public string AlphabeticShortcut
        {
            get { return Get(alphabeticShortcutProperty); }
            set { Set(alphabeticShortcutProperty, value); }
        }

        /// <summary>
        /// android:numericShortcut
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("numericShortcut", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        public string NumericShortcut
        {
            get { return Get(numericShortcutProperty); }
            set { Set(numericShortcutProperty, value); }
        }

        /// <summary>
        /// android:checkable
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("checkable", AndroidConstants.AndroidNamespace)]
        [DefaultValue("false")]
        [Obfuscation(Feature = "@Xaml")]
        [TypeConverter(typeof(BoolTypeConverter))]
        public string Checkable
        {
            get { return Get(checkableProperty); }
            set { Set(checkableProperty, value); }
        }

        /// <summary>
        /// Add the given child to this container.
        /// </summary>
        SerializationNode ISerializationNodeContainer.Add(SerializationNode child)
        {
            return menu;
        }

        /// <summary>
        /// Remove the given child from this container.
        /// </summary>
        void ISerializationNodeContainer.Remove(SerializationNode child)
        {
        }

        /// <summary>
        /// Gets all children.
        /// </summary>
        IEnumerable<SerializationNode> ISerializationNodeContainer.Children
        {
            get
            {
                if (menu.Children.Count > 0)
                    yield return menu;
            }
        }
    }
}
