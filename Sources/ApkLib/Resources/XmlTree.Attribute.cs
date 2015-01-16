using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace Dot42.ApkLib.Resources
{
    partial class XmlTree 
    {
        [DebuggerDisplay("{@Name}={@RawValue}")]
        internal sealed class Attribute
        {
            private readonly XmlTree tree;
            private int resourceId = -1;

            /// <summary>
            /// Creation ctor
            /// </summary>
            internal Attribute(XmlTree tree, XAttribute attr)
            {
                this.tree = tree;
                Namespace = string.IsNullOrEmpty(attr.Name.NamespaceName) ? null : attr.Name.NamespaceName;
                Name = attr.Name.LocalName;
                RawValue = attr.Value;
            }

            /// <summary>
            /// Read ctor
            /// </summary>
            internal Attribute(ResReader reader, XmlTree tree)
            {
                this.tree = tree;
                Namespace = StringPoolRef.Read(reader, tree.StringPool);
                Name = StringPoolRef.Read(reader, tree.StringPool);
                RawValue = StringPoolRef.Read(reader, tree.StringPool);
                TypedValue = new Value(reader);
            }

            public string Namespace { get; set; }
            public string Name { get; set; }
            public string RawValue { get; set; }
            public Value TypedValue { get; set; }

            /// <summary>
            /// Gets my name as XName
            /// </summary>
            internal XName XName { get { return XName.Get(Name, Namespace ?? string.Empty); } }

            /// <summary>
            /// Build an XML document part for this node.
            /// </summary>
            internal void BuildTree(Stack<XContainer> documentStack)
            {
                var value = RawValue;
                if ((value == null) && (TypedValue != null))
                {
                    value = TypedValue.GetValue(tree);
                }
                var attr = new XAttribute(XName, value ?? string.Empty);
                documentStack.Peek().Add(attr);
            }

            /// <summary>
            /// Assign resource ID's to attributes.
            /// </summary>
            internal void AssignResourceIds(ResourceIdMap resourceIdMap)
            {
                if (string.IsNullOrEmpty(Namespace))
                    return;

                int id;
                Value.Types valueType;
                if (resourceIdMap.TryGetId(XName, out id, out valueType))
                {
                    // Set resource id
                    resourceId = id;

                    // Change value type (if needed)
                    switch (valueType)
                    {
                        case Value.Types.TYPE_FIRST_INT:
                            TypedValue = new Value(valueType, int.Parse(RawValue));
                            break;
                    }
                }
            }

            /// <summary>
            /// Prepare this chunk for writing
            /// </summary>
            internal void PrepareForWrite()
            {
                StringPoolRef.Prepare(tree.StringPool, Namespace);
                StringPoolRef.Prepare(tree.StringPool, Name, resourceId);
                StringPoolRef.Prepare(tree.StringPool, RawValue);                
            }            

            /// <summary>
            /// Write this attribute.
            /// </summary>
            internal void Write(ResWriter writer)
            {
                StringPoolRef.Write(writer, tree.StringPool, Namespace);
                StringPoolRef.Write(writer, tree.StringPool, Name, resourceId);
                StringPoolRef.Write(writer, tree.StringPool, RawValue);
                var value = TypedValue ?? new Value(Value.Types.TYPE_STRING, tree.StringPool.Get(RawValue, -1));
                value.Write(writer);
            }
        }
    }
}
