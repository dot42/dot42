using System;
using System.Collections.Generic;

namespace Dot42.DexLib
{
    public class Parameter : FreezableBase, IAnnotationProvider, ICloneable, IEquatable<Parameter>
    {
        private TypeReference _type;
        private List<Annotation> _annotations;
        private string _name;

        public Parameter()
        {
            Annotations = new List<Annotation>();
        }

        public Parameter(TypeReference type, string name) : this()
        {
            Type = type;
            Name = name;
        }

        public TypeReference Type { get { return _type; } set { ThrowIfFrozen(); _type = value; } }
        public string Name { get { return _name; } set { ThrowIfFrozen(); _name = value; } }

        #region IAnnotationProvider Members

        public IList<Annotation> Annotations
        {
            get {  return _annotations;}
            set { _annotations = new List<Annotation>(value); }
            //get { return IsFrozen ? (ICollection<Annotation>)_annotations.AsReadOnly() : _annotations; }
            //set { ThrowIfFrozen(); _annotations = new List<Annotation>(value); }
        }

        #endregion

        public override string ToString()
        {
            return Type.ToString();
        }

        object ICloneable.Clone()
        {
            var result = new Parameter();
            result.Type = Type;
            result.Name = Name;

            return result;
        }

        internal Parameter Clone()
        {
            return (Parameter) (this as ICloneable).Clone();
        }


        public bool Equals(Parameter other)
        {
            // do not check annotations at this time.
            return Type.Equals(other.Type);
        }

        #region Freezable
        public override bool Freeze()
        {
            bool gotFrozen = base.Freeze();
            if (gotFrozen)
            {
                if(_type != null)
                    _type.Freeze();

                // TODO: do we want to freeze the annotations as well?
                //       may this is not neccessary, since we do no optimizations
                //       on them.
            }
                
            return gotFrozen;
        }

        public override bool Unfreeze()
        {
            bool thawed = base.Unfreeze();
            if (thawed)
            {
                if (_type != null)
                    _type.Unfreeze();
            }

            return thawed;
        }
        #endregion
    }
}