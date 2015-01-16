using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib.Model
{
    /// <summary>
    /// Maintain stack frame information
    /// </summary>
    public class DalvikStackFrame
    {
        public readonly FrameId Id;
        public readonly Location Location;
        private readonly DalvikThread thread;
        private DocumentLocation documentLocation;
        private DalvikMethod method;
        private DalvikReferenceType referenceType;
        private List<DalvikValue> values;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DalvikStackFrame(FrameId id, Location location, DalvikThread thread)
        {
            Id = id;
            Location = location;
            this.thread = thread;
        }

        /// <summary>
        /// Exception (if any) that occurred in this frame
        /// </summary>
        public TaggedObjectId Exception { get; set; }

        /// <summary>
        /// Convert the dalvik location into a document location.
        /// </summary>
        public Task<DocumentLocation> GetDocumentLocationAsync()
        {
            if (documentLocation != null) return documentLocation.AsTask();
            return thread.Manager.Process.ResolveAsync(Location).SaveAndReturn(x => documentLocation = x);
        }

        /// <summary>
        /// Gets a description of my location (class name + method name)
        /// </summary>
        public Task<string> GetDescriptionAsync()
        {
            return GetDocumentLocationAsync().Select(x => x.Description);
        }

        /// <summary>
        /// Get the local variables for this frame.
        /// </summary>
        /// <returns></returns>
        public Task<List<DalvikValue>> GetValuesAsync()
        {
            if (values != null) return values.AsTask();
            return Task.Factory.StartNew(() => {
                var meth = GetMethodAsync().Await(DalvikProcess.VmTimeout);
                var allVariableTable = meth.GetVariableTableAsync().Await(DalvikProcess.VmTimeout);
                var validVariableTable = allVariableTable.Where(x => x.IsValidAt((int) Location.Index)).ToList();
                var process = thread.Manager.Process;
                if (validVariableTable.Count > 0)
                {
                    var slotRequests = validVariableTable.Select(x => new SlotRequest(x.Slot, x.Tag)).ToArray();
                    var frameValues = Debugger.StackFrame.GetValuesAsync(Thread.Id, Id, slotRequests).Await(DalvikProcess.VmTimeout);
                    var list = new List<DalvikValue>();
                    for (var i = 0; i < slotRequests.Length; i++)
                    {
                        list.Add(new DalvikStackFrameValue(frameValues[i], validVariableTable[i], process));
                    }
                    values = list;
                }
                values = values ?? new List<DalvikValue>();
                if (Exception != null)
                {
                    values.Insert(0, new DalvikExceptionValue(new Value(Exception), process));
                }
                return values;
            });
        }

        /// <summary>
        /// Load the class that this frame refers to.
        /// </summary>
        public DalvikReferenceType GetReferenceType()
        {
            if (referenceType != null) return referenceType;
            referenceType = thread.Manager.Process.ReferenceTypeManager[Location.Class];
            return referenceType;
        }

        /// <summary>
        /// Load the method that this frame refers to.
        /// </summary>
        public Task<DalvikMethod> GetMethodAsync()
        {
            if (method != null) return method.AsTask();
            var refType = GetReferenceType();
            return refType.GetMethodsAsync().Select(x => x.FirstOrDefault(m => m.Id.Equals(Location.Method))).SaveAndReturn(x => method = x);
        }

        /// <summary>
        /// Provide access to the thread.
        /// </summary>
        protected DalvikThread Thread { get { return thread; } }

        /// <summary>
        /// Provide access to the low level debugger.
        /// </summary>
        protected Debugger Debugger { get { return thread.Manager.Debugger; } }
    }
}
