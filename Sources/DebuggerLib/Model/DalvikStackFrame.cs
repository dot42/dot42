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
        private List<DalvikStackFrameValue> registers;
        private List<DalvikStackFrameValue> parameters;
        private List<VariableInfo> validVariables;

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
            return Task.Factory.StartNew(() =>
            {
                var validVariableTable = GetValidVariableTableAsync().Await(DalvikProcess.VmTimeout);

                var process = thread.Manager.Process;
                if (validVariableTable.Count > 0)
                {
                    var slotRequests = validVariableTable.Select(x => new SlotRequest(x.Slot, x.Tag))
                                                         .ToArray();
                    var frameValues = Debugger.StackFrame.GetValuesAsync(Thread.Id, Id, slotRequests).Await(DalvikProcess.VmTimeout);
                    var list = new List<DalvikValue>();
                    for (var i = 0; i < slotRequests.Length; i++)
                    {
                        list.Add(new DalvikStackFrameValue(frameValues[i], validVariableTable[i], false, process));
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

        private Task<List<VariableInfo>> GetValidVariableTableAsync()
        {
            if (validVariables != null) return validVariables.AsTask();
            return Task.Factory.StartNew(() =>
            {
                var meth = GetMethodAsync().Await(DalvikProcess.VmTimeout);
                var allVariableTable = meth.GetVariableTableAsync().Await(DalvikProcess.VmTimeout);
                var validVariableTable = allVariableTable.Where(x => x.IsValidAt((int) Location.Index)).ToList();
                return validVariables = validVariables ?? validVariableTable;
            });
        }

        /// <summary>
        /// Get all local registers for this frame.
        /// </summary>
        /// <returns></returns>
        public Task<List<DalvikStackFrameValue>> GetRegistersAsync(bool parametersOnly = false)
        {
            if (parametersOnly && parameters != null)
                return parameters.AsTask();
            if (!parametersOnly && registers != null) 
                return registers.AsTask();

            return Task.Factory.StartNew(() =>
            {
                var ret = new List<DalvikStackFrameValue>();

                var loc = GetDocumentLocationAsync().Await(DalvikProcess.VmTimeout);

                var methodDiss = thread.Manager.Process.DisassemblyProvider.GetFromLocation(loc);

                if (methodDiss == null)
                    return ret;

                var process = thread.Manager.Process;
                var body = methodDiss.Method.Body;
                var regDefs = (parametersOnly ? body.Registers.Where(r=>body.IsComing(r))
                                              : body.Registers)
                              .OrderBy(p=>p.Index)
                              .ToList();


                var requests = regDefs.Select(reg =>
                {
                    //var dataType = reg.IsBits4 ? Jdwp.Tag.Int : Jdwp.Tag.Long; // this doesn't work for unknown reasons.
                    var dataType = Jdwp.Tag.Int;
                    return new SlotRequest(reg.Index, dataType);
                }).ToList();
   
                var regValues = Debugger.StackFrame.GetValuesAsync(thread.Id, Id, requests.ToArray())
                                                   .Await(DalvikProcess.VmTimeout);

                for (int i = 0; i < regDefs.Count && i < regValues.Count; ++i)
                {
                    var reg = regDefs[i];
                    bool isParam = body.IsComing(reg);

                    string regName = MethodDisassembly.FormatRegister(reg, body);
                    var valInfo = new VariableInfo(0, regName, null, null, body.Instructions.Count, reg.Index);

                    var val = new DalvikStackFrameValue(regValues[i], valInfo, isParam, process);
                    ret.Add(val);
                }

                if(parametersOnly)
                    parameters = parameters ?? ret;
                else
                    registers = registers ?? ret;
                return ret;
            });
        }

        /// <summary>
        /// Makes the stackframe to invalidate is variable-value cache.
        /// </summary>
        public void InvalidateVariablesValueCache()
        {
            values = null;
            registers = null;
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
