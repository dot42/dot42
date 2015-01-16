using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.Utility;

namespace Dot42.DebuggerLib.Events.Dalvik
{
    /// <summary>
    /// Thread status notification (THST)
    /// </summary>
    public class ThreadStatus : DalvikEvent
    {
        /// <summary>
        /// Per thread info
        /// </summary>
        public class ThreadStatusInfo
        {
            public readonly ThreadId Id;
            public readonly ThreadStates State;

            public ThreadStatusInfo(ThreadId id, ThreadStates state)
            {
                Id = id;
                State = state;
            }
        }

        private readonly List<ThreadStatusInfo> infoList;

        public ThreadStatus(IEnumerable<ThreadStatusInfo> infos)
        {
            infoList = infos.ToList();
        }

        /// <summary>
        /// Thread id.
        /// </summary>
        public IEnumerable<ThreadStatusInfo> Infos
        {
            get { return infoList; }
        }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TResult Accept<TResult, TData>(EventVisitor<TResult, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Handle Thread Status messages
        /// </summary>
        internal static void HandleTHST(Chunk chunk, Action<DalvikEvent> onEvent)
        {
            var data = chunk.Data;
            var hdrLength = data.GetByte();
            var entryLen = data.GetByte();
            var count = data.GetInt16();
            data.Skip(hdrLength - 4);

            var list = new List<ThreadStatusInfo>();
            for (var i = 0; i < count; i++)
            {
                var id = new ThreadId(data, 4);
                var state = data.GetByte();
                var tid = data.GetInt();
                var utime = data.GetInt();
                var stime = data.GetInt();
                var isDaemon = data.GetByte();
                data.Skip(entryLen - 18);

                DLog.Debug(DContext.DebuggerLibEvent, "THST id={0}, state={1}, tid={2}, utime={3}, stime={4}", id, state, tid, utime, stime);

                list.Add(new ThreadStatusInfo(id, (ThreadStates)state));
            }
            onEvent(new ThreadStatus(list));
        }
    }
}
