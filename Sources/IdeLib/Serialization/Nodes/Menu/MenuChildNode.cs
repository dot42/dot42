using System;
using System.ComponentModel;
using System.Reflection;
using Dot42.ApkLib;
using Dot42.Ide.TypeConverters;
using Dot42.ResourcesLib;
using Dot42.Utility;

namespace Dot42.Ide.Serialization.Nodes.Menu
{
    /// <summary>
    /// menu/item,
    /// menu/group
    /// </summary>
    [Obfuscation(Feature = "@SerializableNode")]
    public abstract class MenuChildNode : SerializationNode
    {
        private readonly PropertyInfo idProperty;
        private readonly PropertyInfo visibleProperty;
        private readonly PropertyInfo enabledProperty;
        private readonly PropertyInfo menuCategoryProperty;
        private readonly PropertyInfo orderInCategoryProperty;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected MenuChildNode()
        {
            idProperty = ReflectionHelper.PropertyOf(() => Id);
            visibleProperty = ReflectionHelper.PropertyOf(() => Visible);
            enabledProperty = ReflectionHelper.PropertyOf(() => Enabled);
            menuCategoryProperty = ReflectionHelper.PropertyOf(() => MenuCategory);
            orderInCategoryProperty = ReflectionHelper.PropertyOf(() => OrderInCategory);
        }

        /// <summary>
        /// Gets the containing for adding child items to.
        /// </summary>
        public abstract IMenuChildNodeContainer ChildContainer { get; }

        /// <summary>
        /// Gets the parent of this node (if any).
        /// </summary>
        internal MenuChildNode Parent
        {
            get
            {
                var container = Container as IMenuChildNodeContainer;
                return (container != null) ? container.Parent : null;
            }
        }

        /// <summary>
        /// android:id
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("id", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        public string Id
        {
            get { return Get(idProperty); }
            set
            {
                string errorMessage;
                if (!ResourceValidators.ValidateIdNameValue(ref value, out errorMessage))
                {
                    if (IsEditing)
                        throw new ArgumentException(errorMessage);
                }
                Set(idProperty, value);
            }
        }

        /// <summary>
        /// android:visible
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("visible", AndroidConstants.AndroidNamespace)]
        [DefaultValue("true")]
        [Obfuscation(Feature = "@Xaml")]
        [TypeConverter(typeof(BoolTypeConverter))]
        public string Visible
        {
            get { return Get(visibleProperty); }
            set
            {
                string errorMessage;
                if (!ResourceValidators.ValidateBoolValue(ref value, out errorMessage))
                {
                    if (IsEditing)
                        throw new ArgumentException(errorMessage);
                }
                Set(visibleProperty, value);
            }
        }

        /// <summary>
        /// android:enabled
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("enabled", AndroidConstants.AndroidNamespace)]
        [DefaultValue("true")]
        [Obfuscation(Feature = "@Xaml")]
        [TypeConverter(typeof(BoolTypeConverter))]
        public string Enabled
        {
            get { return Get(enabledProperty); }
            set
            {
                string errorMessage;
                if (!ResourceValidators.ValidateBoolValue(ref value, out errorMessage))
                {
                    if (IsEditing)
                        throw new ArgumentException(errorMessage);
                }
                Set(enabledProperty, value);
            }
        }

        /// <summary>
        /// android:menuCategory
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("menuCategory", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        [TypeConverter(typeof(MenuCategoryTypeConverter))]
        public string MenuCategory
        {
            get { return Get(menuCategoryProperty); }
            set
            {
                string errorMessage;
                if (!ResourceValidators.ValidateMenuCategoryValue(ref value, out errorMessage))
                {
                    if (IsEditing)
                        throw new ArgumentException(errorMessage);
                }
                Set(menuCategoryProperty, value);
            }
        }

        /// <summary>
        /// android:orderInCategory
        /// </summary>
        [Category(Categories.Design)]
        [AttributeName("orderInCategory", AndroidConstants.AndroidNamespace)]
        [Obfuscation(Feature = "@Xaml")]
        public string OrderInCategory
        {
            get { return Get(orderInCategoryProperty); }
            set
            {
                string errorMessage;
                if (!ResourceValidators.ValidateIntegerValue(ref value, out errorMessage))
                {
                    if (IsEditing)
                        throw new ArgumentException(errorMessage);
                }
                Set(orderInCategoryProperty, value);
            }
        }
    }
}
