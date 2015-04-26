using System;
using Dot42.Mapping;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Information describing the source of a dalvik location.
    /// </summary>
    public class DocumentLocation
    {
        public readonly Location Location;
        public readonly Document Document; // Can be null
        public readonly DocumentPosition Position; // Can be null
        public readonly DalvikReferenceType ReferenceType; // Can be null
        public readonly DalvikMethod Method; // Can be null
        private readonly TypeEntry typeEntry;
        private readonly MethodEntry methodEntry;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DocumentLocation(Location location, Document document, DocumentPosition position, DalvikReferenceType referenceType, DalvikMethod method, TypeEntry typeEntry, MethodEntry methodEntry)
        {
            if (location == null)
                throw new ArgumentNullException("location");
            Location = location;
            Document = document;
            Position = position;
            ReferenceType = referenceType;
            Method = method;
            this.typeEntry = typeEntry;
            this.methodEntry = methodEntry;
        }

        /// <summary>
        /// Gets a description of this location.
        /// </summary>
        public string Description
        {
            get
            {
                return ClassName + "." + MethodName;
            }
        }

        private string ClassName 
        { 
            get
            {
                return (typeEntry != null)
                    ? typeEntry.Name
                    : (ReferenceType != null) ? ReferenceType.GetNameAsync().Await(DalvikProcess.VmTimeout) : "?";
            }
        }

        public string MethodName 
        { 
            get
            {
                return (MethodEntry != null) ? MethodEntry.Name : (Method != null) ? Method.Name : "?";
            }
        }


        public MethodEntry MethodEntry
        {
            get { return methodEntry; }
        }

        public TypeEntry TypeEntry { get { return typeEntry; } }
    }
}
