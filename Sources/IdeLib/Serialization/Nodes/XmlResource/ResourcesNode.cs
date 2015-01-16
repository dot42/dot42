using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dot42.ApkLib;

namespace Dot42.Ide.Serialization.Nodes.XmlResource
{
    /// <summary>
    /// resources
    /// </summary>
    [ElementName("resources", RootElement = true)]
    [AddNamespace("android", AndroidConstants.AndroidNamespace)]
    [Obfuscation(Feature = "@SerializableNode")]
    public sealed class ResourcesNode : SerializationNode, ISerializationNodeContainer
    {
        private readonly BoolNodeCollection bools = new BoolNodeCollection();
        private readonly ColorNodeCollection colors = new ColorNodeCollection();
        private readonly IntegerNodeCollection integers = new IntegerNodeCollection();
        private readonly IntegerArrayNodeCollection integerArrays = new IntegerArrayNodeCollection();
        private readonly DimensionNodeCollection dimensions = new DimensionNodeCollection();
        private readonly IdNodeCollection ids = new IdNodeCollection();
        private readonly PluralsNodeCollection plurals = new PluralsNodeCollection();
        private readonly StringNodeCollection strings = new StringNodeCollection();
        private readonly StringArrayNodeCollection stringArrays = new StringArrayNodeCollection();
        private readonly StyleNodeCollection styles = new StyleNodeCollection();
        private readonly TypedArrayNodeCollection typedArrays = new TypedArrayNodeCollection();

        /// <summary>
        /// Default ctor
        /// </summary>
        public ResourcesNode()
        {
            foreach (var tuple in Collections)
            {
                var name = tuple.Item1;
                tuple.Item2.CollectionChanged += (s, x) => OnPropertyChanged(name, false);
                tuple.Item2.PropertyChanged += (s, x) => OnPropertyChanged(name + "." + x.PropertyName, x);                
            }
        }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TReturn Accept<TReturn, TData>(SerializationNodeVisitor<TReturn, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets all bool's
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public BoolNodeCollection Bools
        {
            get { return bools; }
        }

        /// <summary>
        /// Gets all colors
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public ColorNodeCollection Colors
        {
            get { return colors; }
        }

        /// <summary>
        /// Gets all dimensions
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public DimensionNodeCollection Dimensions
        {
            get { return dimensions; }
        }

        /// <summary>
        /// Gets all id's
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public IdNodeCollection Ids
        {
            get { return ids; }
        }

        /// <summary>
        /// Gets all integers
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public IntegerNodeCollection Integers
        {
            get { return integers; }
        }

        /// <summary>
        /// Gets all integer-arrays
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public IntegerArrayNodeCollection IntegerArrays
        {
            get { return integerArrays; }
        }

        /// <summary>
        /// Gets all plurals
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public PluralsNodeCollection Plurals
        {
            get { return plurals; }
        }

        /// <summary>
        /// Gets all strings
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public StringNodeCollection Strings
        {
            get { return strings; }
        }

        /// <summary>
        /// Gets all string-array's
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public StringArrayNodeCollection StringArrays
        {
            get { return stringArrays; }
        }

        /// <summary>
        /// Gets all styles
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public StyleNodeCollection Styles
        {
            get { return styles; }
        }

        /// <summary>
        /// Gets all typed arrays
        /// </summary>
        [Obfuscation(Feature = "@Xaml")]
        public TypedArrayNodeCollection TypedArrays
        {
            get { return typedArrays; }
        }

        /// <summary>
        /// Add the given child to this container.
        /// </summary>
        SerializationNode ISerializationNodeContainer.Add(SerializationNode child)
        {
            var collection = child.Accept(GetCollectionVisitor.Instance, this);
            if (collection == null)
                throw new ArgumentException("unknown child");
            collection.Add(child);
            return child;
        }

        /// <summary>
        /// Remove the given child from this container.
        /// </summary>
        void ISerializationNodeContainer.Remove(SerializationNode child)
        {
            var collection = child.Accept(GetCollectionVisitor.Instance, this);
            if (collection == null)
                throw new ArgumentException("unknown child");
            collection.Remove(child);
        }

        /// <summary>
        /// Gets all children.
        /// </summary>
        IEnumerable<SerializationNode> ISerializationNodeContainer.Children
        {
            get { return Collections.SelectMany(x => x.Item2.Children); }
        }

        /// <summary>
        /// Gets all collections
        /// </summary>
        private IEnumerable<Tuple<string, INodeCollection>> Collections
        {
            get
            {
                yield return Tuple.Create("Bools", (INodeCollection)bools);
                yield return Tuple.Create("Colors", (INodeCollection)colors);
                yield return Tuple.Create("Dimensions", (INodeCollection)dimensions);
                yield return Tuple.Create("Id's", (INodeCollection)ids);
                yield return Tuple.Create("Integers", (INodeCollection)integers);
                yield return Tuple.Create("Integer array's", (INodeCollection)integerArrays);
                yield return Tuple.Create("Strings", (INodeCollection)strings);
                yield return Tuple.Create("String array's", (INodeCollection)stringArrays);
                yield return Tuple.Create("Quantity strings", (INodeCollection) plurals);
                yield return Tuple.Create("Styles", (INodeCollection)styles);
                yield return Tuple.Create("Typed array's", (INodeCollection)typedArrays);
            }
        }

        /// <summary>
        /// Visitor used to get the collection needed to store an item into.
        /// </summary>
        private class GetCollectionVisitor : DefaultSerializationNodeVisitor<ISerializationNodeContainer, ResourcesNode>
        {
            public static readonly GetCollectionVisitor Instance = new GetCollectionVisitor();

            public override ISerializationNodeContainer Visit(BoolNode node, ResourcesNode data)
            {
                return data.bools;
            }

            public override ISerializationNodeContainer Visit(ColorNode node, ResourcesNode data)
            {
                return data.colors;
            }

            public override ISerializationNodeContainer Visit(DimensionNode node, ResourcesNode data)
            {
                return data.dimensions;
            }

            public override ISerializationNodeContainer Visit(IdNode node, ResourcesNode data)
            {
                return data.ids;
            }

            public override ISerializationNodeContainer Visit(IntegerNode node, ResourcesNode data)
            {
                return data.integers;
            }

            public override ISerializationNodeContainer Visit(IntegerArrayNode node, ResourcesNode data)
            {
                return data.integerArrays;
            }

            public override ISerializationNodeContainer Visit(PluralsNode node, ResourcesNode data)
            {
                return data.plurals;
            }

            public override ISerializationNodeContainer Visit(StringNode node, ResourcesNode data)
            {
                return data.strings;
            }

            public override ISerializationNodeContainer Visit(StringArrayNode node, ResourcesNode data)
            {
                return data.stringArrays;
            }

            public override ISerializationNodeContainer Visit(StyleNode node, ResourcesNode data)
            {
                return data.styles;
            }

            public override ISerializationNodeContainer Visit(TypedArrayNode node, ResourcesNode data)
            {
                return data.typedArrays;
            }
        }
    }
}
