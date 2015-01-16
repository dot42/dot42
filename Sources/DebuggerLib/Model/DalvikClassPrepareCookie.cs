using System;
using Dot42.DebuggerLib.Events.Jdwp;

namespace Dot42.DebuggerLib.Model
{
    public class DalvikClassPrepareCookie : DalvikCookie<Tuple<string, Action<ClassPrepare>>>
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public DalvikClassPrepareCookie(int value, Tuple<string, Action<ClassPrepare>> payload)
            : base(value, payload)
        {
        }

        /// <summary>
        /// Gets the signature of the class to look for.
        /// </summary>
        public string Signature { get { return Payload.Item1; } }

        /// <summary>
        /// Gets the handler to invoke on a class prepare event.
        /// </summary>
        public Action<ClassPrepare> Handler { get { return Payload.Item2; } }
    }
}
