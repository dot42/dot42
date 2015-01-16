using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.VisualStudio;

namespace Dot42.VStudio.ProjectBase
{
    public struct ItemId
    {
        private readonly uint _id;

        public static ItemId Root = new ItemId(VSConstants.VSITEMID_ROOT);
        public static ItemId Nil = new ItemId(VSConstants.VSITEMID_NIL);
        public static ItemId Selection = new ItemId(VSConstants.VSITEMID_SELECTION);

        public ItemId(uint id)
        {
            _id = id;
        }

        public ItemId(int id)
        {
            _id = (uint) id;
        }

        public uint Value
        {
            get { return _id; }
        }

        public override bool Equals(object obj)
        {
            return Equals((ItemId)obj);
        }

        public bool Equals(ItemId other)
        {
            return (other._id == _id);
        }

        public override int GetHashCode()
        {
            return unchecked((int)_id);
        }

        public bool IsNil
        {
            get { return _id == Nil._id; }
        }

        public bool IsRoot
        {
            get { return _id == Root._id; }
        }

        public bool IsSelection
        {
            get { return _id == Selection._id; }
        }

        public static explicit operator int(ItemId id)
        {
            return (int)id._id;
        }

        public static explicit operator uint(ItemId id)
        {
            return id._id;
        }

        public static implicit operator ItemId(int id)
        {
            return new ItemId(id);
        }

        public static implicit operator ItemId(uint id)
        {
            return new ItemId(id);
        }

        public static ItemId Get(object id)
        {
            return (ItemId) ItemIdTypeConverter.Instance.ConvertFrom(id);
        }

        public override string ToString()
        {
            if (_id == VSConstants.VSITEMID_ROOT) return "ROOT";
            if (_id == VSConstants.VSITEMID_NIL) return "NIL";
            if (_id == VSConstants.VSITEMID_NIL) return "SELECTION";
            return _id.ToString();
        }
    }

    public class ItemIdTypeConverter : TypeConverter
    {
        public static readonly ItemIdTypeConverter Instance = new ItemIdTypeConverter();

        public override bool CanConvertFrom(ITypeDescriptorContext context,
          Type sourceType)
        {
            if (sourceType == typeof(string) || sourceType == typeof(int) ||
              sourceType == typeof(uint)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context,
          Type destinationType)
        {
            if (destinationType == typeof(string) || destinationType == typeof(int) ||
              destinationType == typeof(uint)) return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
          CultureInfo culture, object value)
        {
            if (value is int) return new ItemId((int)value);
            if (value is uint) return new ItemId((uint)value);
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
          CultureInfo culture, object value, Type destinationType)
        {
            if (!(value is ItemId)) return null;
            var hierValue = (ItemId)value;
            if (destinationType == typeof(int)) return (int)hierValue;
            if (destinationType == typeof(uint)) return (uint)hierValue;
            if (destinationType == typeof(string)) return hierValue.ToString();
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
