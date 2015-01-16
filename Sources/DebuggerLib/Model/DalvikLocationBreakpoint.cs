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
        private readonly DocumentPosition documentPosition;
        private readonly TypeEntry typeEntry;
        private readonly MethodEntry methodEntry;
        private DalvikClassPrepareCookie classPrepareCookie;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DalvikLocationBreakpoint(Jdwp.EventKind eventKind, DocumentPosition documentPosition, TypeEntry typeEntry, MethodEntry methodEntry)
            : base(eventKind)
        {
            this.documentPosition = documentPosition;
            this.typeEntry = typeEntry;
            this.methodEntry = methodEntry;
        }

        /// <summary>
        /// Gets the position where this breakpoint was intended to be set.
        /// </summary>
        public DocumentPosition DocumentPosition
        {
            get { return documentPosition; }
        }

        /// <summary>
        /// Try to bind this breakpoint to an actual breakpoint in the VM.
        /// This method is blocking, so make sure to call accordingly.
        /// </summary>
        internal override void TryBind(DalvikProcess process)
        {
            if (IsBound)
                return;

            // Lookup classid & methodid
            var pos = documentPosition;
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
                return;
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

            // Now set breakpoint
            var location = new Location(refType.Id, dmethod.Id, (ulong) pos.MethodOffset);
            var setTask = process.Debugger.EventRequest.SetAsync(Jdwp.EventKind.BreakPoint, Jdwp.SuspendPolicy.All,
                                                            new LocationOnlyModifier(location));
            var requestId = setTask.Await(DalvikProcess.VmTimeout);

            // Store request ID and notify listeners that we're now bound.
            OnBound(requestId, process.BreakpointManager);

            // Process any events that came before we got a chance to record property.
            process.BreakpointManager.ProcessPendingEvents(this);
        }
    }
}
