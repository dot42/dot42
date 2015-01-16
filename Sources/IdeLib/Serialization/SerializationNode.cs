using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Dot42.Ide.Editors;
using Dot42.Utility;

namespace Dot42.Ide.Serialization
{
    /// <summary>
    /// Base class for all serializable nodes.
    /// </summary>
    public abstract class SerializationNode : INotifyPropertyChanged, IEditableObject
    {
        /// <summary>
        /// Invoked when a property value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly PropertyInfo isEditingProperty;
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();
        private Dictionary<string, object> editValues;
        private bool isSelected;
        private bool isExpanded = true;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected SerializationNode()
        {
            isEditingProperty = ReflectionHelper.PropertyOf(() => IsEditing);
        }

        /// <summary>
        /// Gets a value.
        /// </summary>
        protected T Get<T>(PropertyInfo key)
        {
            object result;
            var dict = editValues ?? values;
            return dict.TryGetValue(key.Name, out result) ? (T)result : default(T);
        }

        /// <summary>
        /// Gets a string value.
        /// </summary>
        protected string Get(PropertyInfo key)
        {
            return Get<string>(key);
        }

        /// <summary>
        /// Sets a value.
        /// </summary>
        protected void Set<T>(PropertyInfo key, T value)
        {
            Set(key.Name, value);
        }

        /// <summary>
        /// Sets a value.
        /// </summary>
        private void Set(string key, object value)
        {
            object existing;
            var dict = editValues ?? values;
            if (dict.TryGetValue(key, out existing))
            {
                if (Equals(existing, value))
                    return;
            }
            dict[key] = value;
            OnPropertyChanged(key, true);
        }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public abstract TReturn Accept<TReturn, TData>(SerializationNodeVisitor<TReturn, TData> visitor, TData data);

        /// <summary>
        /// Notify that a property value has changed.
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName, bool isModelProperty)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedExEventArgs(propertyName, isModelProperty));
            }
        }

        /// <summary>
        /// Notify that a property value has changed.
        /// </summary>
        protected void OnPropertyChanged(string propertyName, PropertyChangedEventArgs template)
        {
            OnPropertyChanged(propertyName, template.IsModelProperty());
        }

        /// <summary>
        /// Is this node being edited?
        /// </summary>
        [Browsable(false)]
        [Obfuscation(Feature = "@Xaml")]
        public bool IsEditing
        {
            get { return (editValues != null); }
        }

        /// <summary>
        /// Is this node selected?
        /// This is a helper for when this node is used in an items control.
        /// </summary>
        [Browsable(false)]
        [Obfuscation(Feature = "@Xaml")]
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged(ReflectionHelper.PropertyOf(() => IsSelected).Name, false);
                }
            }
        }

        /// <summary>
        /// Is this node expanded?
        /// This is a helper for when this node is used in an items control.
        /// </summary>
        [Browsable(false)]
        [Obfuscation(Feature = "@Xaml")]
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    OnPropertyChanged(ReflectionHelper.PropertyOf(() => IsExpanded).Name, false);
                }
            }
        }

        /// <summary>
        /// Gets the container that holds this node (if any)
        /// </summary>
        [Browsable(false)]
        public ISerializationNodeContainer Container { get; set; }

        /// <summary>
        /// Begins an edit on an object.
        /// </summary>
        public void BeginEdit()
        {
            editValues = new Dictionary<string, object>(values);
            OnPropertyChanged(isEditingProperty.Name, false);
        }

        /// <summary>
        /// Pushes changes since the last <see cref="M:System.ComponentModel.IEditableObject.BeginEdit"/> or <see cref="M:System.ComponentModel.IBindingList.AddNew"/> call into the underlying object.
        /// </summary>
        public void EndEdit()
        {
            var commitValues = editValues;
            editValues = null;
            if (commitValues != null)
            {
                foreach (var entry in commitValues)
                {
                    Set(entry.Key, entry.Value);
                }
            }
            OnPropertyChanged(isEditingProperty.Name, false);
        }

        /// <summary>
        /// Discards changes since the last <see cref="M:System.ComponentModel.IEditableObject.BeginEdit"/> call.
        /// </summary>
        public void CancelEdit()
        {
            if (editValues != null)
            {
                var editedValues = editValues;
                editValues = null;
                OnPropertyChanged(isEditingProperty.Name, false);
                foreach (var entry in values)
                {
                    if (!Equals(editedValues[entry.Key], entry.Value))
                    {
                        OnPropertyChanged(entry.Key, true);
                    }
                }
            }
        }

        /// <summary>
        /// Set default values for all properties.
        /// </summary>
        public void InitializeWithDefaultValues()
        {
            var properties = TypeDescriptor.GetProperties(this).Cast<PropertyDescriptor>();
            foreach (var p in properties)
            {
                var attr = p.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault();
                if (attr == null)
                    continue;
                p.SetValue(this, attr.Value);
            }
        }
    }
}
