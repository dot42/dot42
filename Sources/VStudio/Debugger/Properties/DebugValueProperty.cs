using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dot42.DebuggerLib;
using Dot42.DebuggerLib.Model;
using Microsoft.VisualStudio.Debugger.Interop;
using TallComponents.Common.Extensions;

namespace Dot42.VStudio.Debugger
{
    internal class DebugValueProperty : DebugProperty
    {
        private const int MaxArrayValues = 1024 * 4;
        protected readonly DalvikValue Value;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DebugValueProperty(DalvikValue value, DebugProperty parent) : base(parent)
        {
            this.Value = value;
        }

        /// <summary>
        /// Does this property have children
        /// </summary>
        protected override bool HasChildren
        {
            get { return !Value.IsPrimitive && !Value.IsString; }
        }

        /// <summary>
        /// Create all child properties
        /// </summary>
        protected override List<DebugProperty> CreateChildren()
        {
            if (Value.IsPrimitive || Value.ObjectReference.IsNull)
                return new List<DebugProperty>();

            List<DebugProperty> list;
            if (Value.IsArray)
            {
                // Get array info
                list = new List<DebugProperty>();

                // Get length
                var length = Value.ObjectReference.GetArrayLengthAsync().Await(DalvikProcess.VmTimeout);
                list.Add(new DebugArrayLengthProperty(length, this));

                // Get elements
                var firstIndex = 0;
                while (length > 0)
                {
                    var chunkLength = Math.Min(length, MaxArrayValues);
                    var elements = Value.ObjectReference.GetArrayValuesAsync(firstIndex, chunkLength).Await(DalvikProcess.VmTimeout);
                    list.AddRange(elements.Select(x => new DebugValueProperty(x, this)));
                    firstIndex += chunkLength;
                    length -= chunkLength;
                }
            }
            else
            {
                // Get instance field values
                var refType = Value.ObjectReference.GetReferenceTypeAsync().Await(DalvikProcess.VmTimeout);
                var fieldValues = refType.GetInstanceFieldValuesAsync(Value.ObjectReference).Await(DalvikProcess.VmTimeout);
                list = fieldValues.Select(x => new DebugValueProperty(x, this)).Cast<DebugProperty>().ToList();

                // Get base class
                var superClass = refType.GetSuperClassAsync().Await(DalvikProcess.VmTimeout);
                if (superClass != null)
                {
                    list.Insert(0, new DebugBaseClassProperty(Value.ObjectReference, superClass, this));
                }
            }

            return list;
        }

        /// <summary>
        /// Construct a DEBUG_PROPERTY_INFO representing this local or parameter.
        /// </summary>
        /// <param name="dwFields"></param>
        /// <param name="dwRadix"></param>
        /// <returns></returns>
        internal override DEBUG_PROPERTY_INFO ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix)
        {
            var info = new DEBUG_PROPERTY_INFO();

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME))
            {
                info.bstrFullName = Value.Name;
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME;
                /*var sb = new StringBuilder(m_variableInformation.m_name);
                info.bstrFullName = sb.ToString();
                info.dwFields = (enum_DEBUGPROP_INFO_FLAGS)((uint)info.dwFields | (uint)(DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME));*/
            }

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME))
            {
                info.bstrName = Value.Name;
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
            }

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE))
            {
                if (Value.IsPrimitive)
                {
                    info.bstrType = Value.Value.GetType().Name;
                }
                else if (Value.IsString)
                {
                    info.bstrType = typeof (string).Name;
                }
                else if (!Value.ObjectReference.IsNull)
                {
                    info.bstrType = Value.ObjectReference.GetReferenceTypeAsync().Select(t => t.GetNameAsync()).Unwrap().Await(DalvikProcess.VmTimeout);
                }
                else
                {
                    info.bstrType = typeof (object).Name;
                }
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
            }

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE))
            {
                if (Value.IsString && !Value.ObjectReference.IsNull)
                {
                    info.bstrValue = Value.ObjectReference.GetStringValueAsync().Await(DalvikProcess.VmTimeout);
                }
                else if (Value.IsArray && !Value.ObjectReference.IsNull)
                {
                    info.bstrValue = "(array)";
                }
                else if (Value.Tag == Jdwp.Tag.ClassObject)
                {
                    info.bstrValue = "{" + Value.ObjectReference.GetClassObjectNameAsync().Await(DalvikProcess.VmTimeout) + "}";
                }
                else
                {
                    if(!Value.IsPrimitive || Value.IsBoolean)
                        info.bstrValue = Value.Value.ToString();
                    else
                    {
                        info.bstrValue = FormatPrimitive(Value, (int)dwRadix);
                    }
                }
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
            }

            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB))
            {
                // all properties readonly by default.
                info.dwAttrib = enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_READONLY;

                if ((!Value.IsPrimitive) && (!Value.IsString) && (!Value.ObjectReference.IsNull))
                {
                    // Objects can be expanded
                    info.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
                }
                if (Value.IsBoolean)
                {
                    info.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_BOOLEAN;
                    if (true.Equals(Value.Value))
                        info.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_BOOLEAN_TRUE;
                } else if (Value.IsString)
                {
                    info.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_RAW_STRING;                    
                }

                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB;
            }

            // If the debugger has asked for the property, or the property has children (meaning it is a pointer in the sample)
            // then set the pProperty field so the debugger can call back when the chilren are enumerated.
            if (dwFields.HasFlag(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP) || (!Value.IsPrimitive))
            {
                info.pProperty = this;
                info.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP;
            }

            return info;
        }

        private string FormatPrimitive(DalvikValue value, int dwRadix)
        {
            if (dwRadix != 16)
            {
                if (value.Tag == Jdwp.Tag.Double)
                    return ((double)value.Value).ToString("R");
                if (value.Tag == Jdwp.Tag.Float)
                    return ((double)value.Value).ToString("R");
                return value.Value.ToString();
            }
                

            switch (value.Tag)
            {
                case Jdwp.Tag.Byte:
                    return "0x" + Convert.ToString((byte) value.Value, dwRadix).PadLeft(2, '0');
                case Jdwp.Tag.Short:
                    return "0x" + Convert.ToString((short)value.Value, dwRadix).PadLeft(4, '0');
                case Jdwp.Tag.Int:
                    return "0x" + Convert.ToString((int)value.Value, dwRadix).PadLeft(8, '0');
                case Jdwp.Tag.Long:
                    return "0x" + Convert.ToString((long)value.Value, dwRadix).PadLeft(16, '0');
                case Jdwp.Tag.Char:
                    return "0x" + Convert.ToString((char)value.Value, dwRadix).PadLeft(16, '0');
                case Jdwp.Tag.Double:
                {
                    var bytes = BitConverter.GetBytes((double) value.Value);
                    var bits = BitConverter.ToInt64(bytes, 0);
                    return "0x" + Convert.ToString(bits, dwRadix).PadLeft(16, '0'); ;
                }
                case Jdwp.Tag.Float:
                {
                    var bytes = BitConverter.GetBytes((float) value.Value);
                    var bits = BitConverter.ToUInt32(bytes, 0);
                    return "0x" + Convert.ToString(bits, dwRadix).PadLeft(8, '0'); ;
                }
                default:
                    return value.Value.ToString();
            }
        }

        protected object ParsePrimitive(string value, Jdwp.Tag tag, int defaultRadix)
        {
            string parseVal = value;
            if (value.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
            {
                parseVal = value.Substring(2);
                defaultRadix = 16;
            }

            switch (tag)
            {
                case Jdwp.Tag.Byte:
                {
                    byte val;
                    if (byte.TryParse(parseVal, defaultRadix == 16 ? NumberStyles.HexNumber : NumberStyles.Any, CultureInfo.CurrentCulture, out val))
                        return val;
                    return sbyte.Parse(parseVal, defaultRadix == 16 ? NumberStyles.HexNumber : NumberStyles.Any, CultureInfo.CurrentCulture);
                }
                case Jdwp.Tag.Short:
                {
                    short val;
                    if (short .TryParse(parseVal, defaultRadix == 16 ? NumberStyles.HexNumber : NumberStyles.Any, CultureInfo.CurrentCulture, out val))
                        return val;
                    return (short)ushort.Parse(parseVal, defaultRadix == 16 ? NumberStyles.HexNumber : NumberStyles.Any, CultureInfo.CurrentCulture);
                }
                case Jdwp.Tag.Int:
                {
                    int val;
                    if (int.TryParse(parseVal, defaultRadix == 16 ? NumberStyles.HexNumber : NumberStyles.Any, CultureInfo.CurrentCulture, out val))
                        return val;
                    return (int)uint.Parse(parseVal, defaultRadix == 16 ? NumberStyles.HexNumber : NumberStyles.Any, CultureInfo.CurrentCulture);
                }
                case Jdwp.Tag.Long:
                {
                    long val;
                    if (long.TryParse(parseVal, defaultRadix == 16 ? NumberStyles.HexNumber : NumberStyles.Any, CultureInfo.CurrentCulture, out val))
                        return val;
                    return (long)ulong.Parse(parseVal, defaultRadix == 16 ? NumberStyles.HexNumber : NumberStyles.Any, CultureInfo.CurrentCulture);
                }
                case Jdwp.Tag.Char:
                {
                    if (value.Length == 1)
                        return value[0];
                    goto case Jdwp.Tag.Short;
                }
                case Jdwp.Tag.Double:
                {
                    return Convert.ToDouble(value);
                }
                case Jdwp.Tag.Float:
                {
                    return Convert.ToSingle(value);
                }
                case Jdwp.Tag.Boolean:
                {
                    return Convert.ToBoolean(value);
                }
                default:
                    throw new NotImplementedException("cannot parse to " + tag);
            }
        }
    }
}
