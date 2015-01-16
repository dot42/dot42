using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dot42.DebuggerLib.Model;
using Microsoft.VisualStudio.Debugger.Interop;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Debugger
{
    internal sealed class DebugValueProperty : DebugProperty
    {
        private const int MaxArrayValues = 1024 * 4;
        private readonly DalvikValue value;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DebugValueProperty(DalvikValue value, DebugProperty parent) : base(parent)
        {
            this.value = value;
        }

        /// <summary>
        /// Does this property have children
        /// </summary>
        protected override bool HasChildren
        {
            get { return !value.IsPrimitive && !value.IsString; }
        }

        /// <summary>
        /// Create all child properties
        /// </summary>
        protected override List<DebugProperty> CreateChildren()
        {
            if (value.IsPrimitive || value.ObjectReference.IsNull)
                return new List<DebugProperty>();

            List<DebugProperty> list;
            if (value.IsArray)
            {
                // Get array info
                list = new List<DebugProperty>();

                // Get length
                var length = value.ObjectReference.GetArrayLengthAsync().Await(DalvikProcess.VmTimeout);
                list.Add(new DebugArrayLengthProperty(length, this));

                // Get elements
                var firstIndex = 0;
                while (length > 0)
                {
                    var chunkLength = Math.Min(length, MaxArrayValues);
                    var elements = value.ObjectReference.GetArrayValuesAsync(firstIndex, chunkLength).Await(DalvikProcess.VmTimeout);
                    list.AddRange(elements.Select(x => new DebugValueProperty(x, this)));
                    firstIndex += chunkLength;
                    length -= chunkLength;
                }
            }
            else
            {
                // Get instance field values
                var refType = value.ObjectReference.GetReferenceTypeAsync().Await(DalvikProcess.VmTimeout);
                var fieldValues = refType.GetInstanceFieldValuesAsync(value.ObjectReference).Await(DalvikProcess.VmTimeout);
                list = fieldValues.Select(x => new DebugValueProperty(x, this)).Cast<DebugProperty>().ToList();

                // Get base class
                var superClass = refType.GetSuperClassAsync().Await(DalvikProcess.VmTimeout);
                if (superClass != null)
                {
                    list.Insert(0, new DebugBaseClassProperty(value.ObjectReference, superClass, this));
                }
            }

            return list;
        }

        /// <summary>
        /// Construct a DEBUG_PROPERTY_INFO representing this local or parameter.
        /// </summary>
        /// <param name="dwFields"></param>
        /// <returns></returns>
        internal override DEBUG_PROPERTY_INFO ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields)
        {
            var info = new DEBUG_PROPERTY_INFO();

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME))
            {
                /*var sb = new StringBuilder(m_variableInformation.m_name);
                info.bstrFullName = sb.ToString();
                info.dwFields = (enum_DEBUGPROP_INFO_FLAGS)((uint)info.dwFields | (uint)(DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME));*/
            }

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME))
            {
                info.bstrName = value.Name;
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
            }

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE))
            {
                if (value.IsPrimitive)
                {
                    info.bstrType = value.Value.GetType().Name;
                }
                else if (value.IsString)
                {
                    info.bstrType = typeof (string).Name;
                }
                else if (!value.ObjectReference.IsNull)
                {
                    info.bstrType = value.ObjectReference.GetReferenceTypeAsync().Select(t => t.GetNameAsync()).Unwrap().Await(DalvikProcess.VmTimeout);
                }
                else
                {
                    info.bstrType = typeof (object).Name;
                }
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
            }

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE))
            {
                if (value.IsString && !value.ObjectReference.IsNull)
                {
                    info.bstrValue = value.ObjectReference.GetStringValueAsync().Await(DalvikProcess.VmTimeout);
                }
                else if (value.IsArray && !value.ObjectReference.IsNull)
                {
                    info.bstrValue = "(array)";
                }
                else
                {
                    info.bstrValue = value.Value.ToString();
                }
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
            }

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB))
            {
                // The sample does not support writing of values displayed in the debugger, so mark them all as read-only.
                info.dwAttrib = enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_READONLY;
                if ((!value.IsPrimitive) && (!value.IsString) && (!value.ObjectReference.IsNull))
                {
                    // Objects can be expanded
                    info.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
                }
                if (value.IsBoolean)
                {
                    info.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_BOOLEAN;
                    if (true.Equals(value.Value))
                        info.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_BOOLEAN_TRUE;
                } else if (value.IsString)
                {
                    info.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_RAW_STRING;                    
                }
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB;
            }

            // If the debugger has asked for the property, or the property has children (meaning it is a pointer in the sample)
            // then set the pProperty field so the debugger can call back when the chilren are enumerated.
            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP) || (!value.IsPrimitive))
            {
                info.pProperty = this;
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP;
            }

            return info;
        }
    }
}
