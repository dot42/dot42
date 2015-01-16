using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Dot42.VStudio.Debugger
{
    internal class BaseEnum<T, I> where I : class
    {
        private readonly List<T> data;
        private uint position;

        public BaseEnum(IEnumerable<T> data)
        {
            this.data = data.ToList();
            position = 0;
        }

        public int Clone(out I ppEnum)
        {
            ppEnum = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetCount(out uint pcelt)
        {
            pcelt = (uint) data.Count;
            return VSConstants.S_OK;
        }

        public int Next(uint celt, T[] rgelt, out uint celtFetched)
        {
            return Move(celt, rgelt, out celtFetched);
        }

        public int Reset()
        {
            lock (this)
            {
                position = 0;

                return VSConstants.S_OK;
            }
        }

        public int Skip(uint celt)
        {
            uint celtFetched;

            return Move(celt, null, out celtFetched);
        }

        private int Move(uint celt, T[] rgelt, out uint celtFetched)
        {
            lock (this)
            {
                int hr = VSConstants.S_OK;
                celtFetched = (uint) data.Count - position;

                if (celt > celtFetched)
                {
                    hr = VSConstants.S_FALSE;
                }
                else if (celt < celtFetched)
                {
                    celtFetched = celt;
                }

                if (rgelt != null)
                {
                    for (int c = 0; c < celtFetched; c++)
                    {
                        rgelt[c] = data[(int) (position + c)];
                    }
                }

                position += celtFetched;

                return hr;
            }
        }
    }

    internal class PortEnum : BaseEnum<IDebugPort2, IEnumDebugPorts2>, IEnumDebugPorts2
    {
        public PortEnum(IEnumerable<IDebugPort2> data)
            : base(data)
        {
        }

        public int Next(uint celt, IDebugPort2[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }

    internal class ProcessEnum : BaseEnum<IDebugProcess2, IEnumDebugProcesses2>, IEnumDebugProcesses2
    {
        public ProcessEnum(IEnumerable<IDebugProcess2> data)
            : base(data)
        {
        }

        public int Next(uint celt, IDebugProcess2[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }

    internal class ProgramEnum : BaseEnum<IDebugProgram2, IEnumDebugPrograms2>, IEnumDebugPrograms2
    {
        public ProgramEnum(IEnumerable<IDebugProgram2> data)
            : base(data)
        {
        }

        public int Next(uint celt, IDebugProgram2[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }

    internal class FrameInfoEnum : BaseEnum<FRAMEINFO, IEnumDebugFrameInfo2>, IEnumDebugFrameInfo2
    {
        public FrameInfoEnum(IEnumerable<FRAMEINFO> data)
            : base(data)
        {
        }

        public int Next(uint celt, FRAMEINFO[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }

    internal class PropertyInfoEnum : BaseEnum<DEBUG_PROPERTY_INFO, IEnumDebugPropertyInfo2>, IEnumDebugPropertyInfo2
    {
        public PropertyInfoEnum(IEnumerable<DEBUG_PROPERTY_INFO> data)
            : base(data)
        {
        }
    }

    internal class ThreadEnum : BaseEnum<IDebugThread2, IEnumDebugThreads2>, IEnumDebugThreads2
    {
        public ThreadEnum(IEnumerable<IDebugThread2> threads)
            : base(threads)
        {

        }

        public int Next(uint celt, IDebugThread2[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }

    internal class ModuleEnum : BaseEnum<IDebugModule2, IEnumDebugModules2>, IEnumDebugModules2
    {
        public ModuleEnum(params IDebugModule2[] modules)
            : base(modules)
        {

        }

        public int Next(uint celt, IDebugModule2[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }

    internal class PropertyEnum : BaseEnum<DEBUG_PROPERTY_INFO, IEnumDebugPropertyInfo2>, IEnumDebugPropertyInfo2
    {
        public PropertyEnum(IEnumerable<DEBUG_PROPERTY_INFO> properties)
            : base(properties)
        {

        }
    }

    internal class CodeContextEnum : BaseEnum<IDebugCodeContext2, IEnumDebugCodeContexts2>, IEnumDebugCodeContexts2
    {
        public CodeContextEnum(IEnumerable<IDebugCodeContext2> codeContexts)
            : base(codeContexts)
        {

        }

        public int Next(uint celt, IDebugCodeContext2[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }

    internal class BoundBreakpointsEnum : BaseEnum<IDebugBoundBreakpoint2, IEnumDebugBoundBreakpoints2>, IEnumDebugBoundBreakpoints2
    {
        public BoundBreakpointsEnum(IEnumerable<IDebugBoundBreakpoint2> breakpoints)
            : base(breakpoints)
        {

        }

        public int Next(uint celt, IDebugBoundBreakpoint2[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }
}
