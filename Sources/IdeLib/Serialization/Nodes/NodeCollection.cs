using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Dot42.Ide.Editors;
using Dot42.Utility;

namespace Dot42.Ide.Serialization.Nodes
{
    /// <summary>
    /// Generic node collection
    /// </summary>
    [Obfuscation(Feature = "@SerializableNode")]
    public abstract class NodeCollection<T> : ObservableCollection<T>, INodeCollection, IEnumerable<SerializationNode>
        where T : SerializationNode
    {
        private readonly string header;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected NodeCollection(string header)
        {
            this.header = header;
        }

        /// <summary>
        /// Notify property changes from children
        /// </summary>
        public new event PropertyChangedEventHandler PropertyChanged
        {
            add { base.PropertyChanged += value; }
            remove { base.PropertyChanged -= value; }
        }

        /// <summary>
        /// Are the no items in this collection?
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public bool IsEmpty { get { return (Count == 0); } }

        /// <summary>
        /// Are the any items in this collection?
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public bool IsNotEmpty { get { return (Count > 0); } }

        /// <summary>
        /// Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param><param name="item">The object to insert.</param>
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            Attach(item);
        }

        /// <summary>
        /// Removes the item at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            Detach(this[index]);
            base.RemoveItem(index);
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param><param name="item">The new value for the element at the specified index.</param>
        protected override void SetItem(int index, T item)
        {
            Detach(this[index]);
            base.SetItem(index, item);
            Attach(this[index]);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged"/> event with the provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            OnPropertyChanged(new PropertyChangedExEventArgs(ReflectionHelper.PropertyOf(() => Header).Name, false));
        }

        /// <summary>
        /// Add the given child to this container.
        /// </summary>
        SerializationNode ISerializationNodeContainer.Add(SerializationNode child)
        {
            Add((T)child);
            return child;
        }

        /// <summary>
        /// Remove the given child from this container.
        /// </summary>
        void ISerializationNodeContainer.Remove(SerializationNode child)
        {
            Remove((T)child);
        }

        /// <summary>
        /// Gets a node at the given index.
        /// </summary>
        SerializationNode ISerializationNodeList.this[int index]
        {
            get { return base[index]; }
        }

        /// <summary>
        /// Gets the index of the given child in this container.
        /// </summary>
        public int IndexOf(SerializationNode child)
        {
            return base.IndexOf((T)child);
        }

        /// <summary>
        /// Move the given child to a new index.
        /// </summary>
        public void MoveTo(SerializationNode child, int newIndex)
        {
            var oldIndex = base.IndexOf((T) child);
            Move(oldIndex, newIndex);
        }

        /// <summary>
        /// Gets all children.
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public IEnumerable<SerializationNode> Children
        {
            get { return this; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        IEnumerator<SerializationNode> IEnumerable<SerializationNode>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Attach the given child to this collection
        /// </summary>
        private void Attach(T child)
        {
            if (child != null)
            {
                child.Container = this;
                child.PropertyChanged += ChildOnPropertyChanged;
            }
        }

        /// <summary>
        /// Detach the given child to this collection
        /// </summary>
        private void Detach(T child)
        {
            if (child != null)
            {
                child.Container = null;
                child.PropertyChanged -= ChildOnPropertyChanged;
            }
        }

        /// <summary>
        /// A property of a child has changed.
        /// </summary>
        private void ChildOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(new PropertyChangedExEventArgs("child." + e.PropertyName, e.IsModelProperty()));
        }

        /// <summary>
        /// Format the count according to the configured header.
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public string Header
        {
            get
            {
                if (Count == 0)
                    return header;
                return string.Format("{0} ({1})", header, Count);
            }
        }
    }
}
