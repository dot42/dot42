using System;
using System.Linq;
using Dot42.Mapping;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Represents a single break point in the code at a specific location.
    /// </summary>
    public class DalvikLocationBreakpoint : DalvikBreakpoint
    {
        /// <summary>
        /// The SourceCodePosition the breakpoint was orginially intended to be set.
        /// </summary>
        public readonly SourceCodePosition SourceCodePosition;

        private readonly TypeEntry typeEntry;
        private readonly MethodEntry methodEntry;
        private DalvikClassPrepareCookie classPrepareCookie;
        private Location location;
        private DocumentLocation documentLocation;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DalvikLocationBreakpoint(Jdwp.EventKind eventKind, SourceCodePosition sourcePosition, TypeEntry typeEntry, MethodEntry methodEntry)
            : base(eventKind)
        {
            this.typeEntry = typeEntry;
            this.methodEntry = methodEntry;
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public DalvikLocationBreakpoint(Location location, DocumentLocation documentLocation=null)
            : base(Jdwp.EventKind.BreakPoint)
        {
            this.location = location;
            this.documentLocation = documentLocation;
        }

        /// <summary>
        /// gets the document location is available
        /// </summary>
        public DocumentLocation DocumentLocation
        {
            get
            {
                return documentLocation;
            }
        }

        /// <summary>
        /// Get the location if available.
        /// </summary>
        public Location Location
        {
            get { return location; }
        }
        /// <summary>
        /// Try to bind this breakpoint to an actual breakpoint in the VM.
        /// This method is blocking, so make sure to call accordingly.
        /// </summary>
        internal override bool TryBind(DalvikProcess process)
        {
            if (IsBound)
                return true;

            var loc = TryGetLocation(process);
            if (loc == null)
                return false; // not yet.
            
            // Now set breakpoint
            var setTask = process.Debugger.EventRequest.SetAsync(Jdwp.EventKind.BreakPoint, Jdwp.SuspendPolicy.All,
                                                                 new LocationOnlyModifier(loc));
            var requestId = setTask.Await(DalvikProcess.VmTimeout);

            // Store request ID and notify listeners that we're now bound.
            OnBound(requestId, process.BreakpointManager);

            // Process any events that came before we got a chance to record property.
            process.BreakpointManager.ProcessPendingEvents(this);

            return true;
        }

        private Location TryGetLocation(DalvikProcess process)
        {
            if (location != null) 
                return location;

            // Lookup classid & methodid
            var signature = typeEntry.DexSignature;
            var referenceTypeManager = process.ReferenceTypeManager;

            // Register a callback for when the class is prepared.
            if (classPrepareCookie == null)
            {
                classPrepareCookie = referenceTypeManager.RegisterClassPrepareHandler(signature, evt => TryBind(process));
            }

            // Now try to find the type
            var refType = referenceTypeManager.FindBySignature(signature);
            if (refType == null)
            {
                // Not possible yet
                return null;
            }

            // We've found the type, we're no longer interested in class prepare events
            if (classPrepareCookie != null)
            {
                referenceTypeManager.Remove(classPrepareCookie);
            }

            var refTypeMethods = refType.GetMethodsAsync().Await(DalvikProcess.VmTimeout);
            var dmethod = refTypeMethods.FirstOrDefault(x => x.IsMatch(methodEntry));
            if (dmethod == null)
            {
                throw new ArgumentException(string.Format("Cannot find method {0}", methodEntry.Name));
            }

            // return location.
            var pos = SourceCodePosition;

            location = new Location(refType.Id, dmethod.Id, (ulong) pos.Position.MethodOffset);
            documentLocation = new DocumentLocation(location, pos, refType, dmethod, typeEntry, methodEntry);

            return location;
        }
    }
}
