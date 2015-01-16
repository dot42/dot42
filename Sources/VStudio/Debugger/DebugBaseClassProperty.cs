using System.Collections.Generic;
using System.Linq;
using Dot42.DebuggerLib.Model;
using Microsoft.VisualStudio.Debugger.Interop;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Debugger
{
    internal sealed class DebugBaseClassProperty : DebugProperty
    {
        private readonly DalvikObjectReference objectReference;
        private readonly DalvikReferenceType superClass;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DebugBaseClassProperty(DalvikObjectReference objectReference, DalvikReferenceType superClass, DebugProperty parent)
            : base(parent)
        {
            this.objectReference = objectReference;
            this.superClass = superClass;
        }

        /// <summary>
        /// Does this property have children
        /// </summary>
        protected override bool HasChildren
        {
            get { return true; }
        }

        /// <summary>
        /// Create all child properties
        /// </summary>
        protected override List<DebugProperty> CreateChildren()
        {
            var fieldValues = superClass.GetInstanceFieldValuesAsync(objectReference).Await(DalvikProcess.VmTimeout);
            var list = fieldValues.Select(x => new DebugValueProperty(x, this)).Cast<DebugProperty>().ToList();

            // Get base class
            if (superClass.GetNameAsync().Await(DalvikProcess.VmTimeout) != "java.lang.Object")
            {
                var superSuperClass = superClass.GetSuperClassAsync().Await(DalvikProcess.VmTimeout);
                if (superSuperClass != null)
                {
                    list.Insert(0, new DebugBaseClassProperty(objectReference, superSuperClass, this));
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
                info.bstrName = "[" + superClass.GetNameAsync().Await(DalvikProcess.VmTimeout) + "]";
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
            }

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE))
            {
                /*StringBuilder sb = new StringBuilder(m_variableInformation.m_typeName);
                info.bstrType = sb.ToString();
                info.dwFields = (enum_DEBUGPROP_INFO_FLAGS)((uint)info.dwFields | (uint)(DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE));*/
            }

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE))
            {
                //info.bstrValue = value.Value.ToString();
                //info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
            }

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB))
            {
                // The sample does not support writing of values displayed in the debugger, so mark them all as read-only.
                info.dwAttrib = enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_READONLY | enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB;
            }

            // If the debugger has asked for the property, or the property has children (meaning it is a pointer in the sample)
            // then set the pProperty field so the debugger can call back when the chilren are enumerated.
            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP))
            {
                info.pProperty = this;
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP;
            }

            return info;
        }
    }
}
