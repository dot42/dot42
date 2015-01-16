using System;
using System.Threading.Tasks;
using Dot42.DebuggerLib.Events.Jdwp;
using Dot42.Utility;
using Exception = Dot42.DebuggerLib.Events.Jdwp.Exception;

namespace Dot42.DebuggerLib
{
    /// <summary>
    /// Event Command Set (64)
    /// 
    /// Composite Command (100)
    /// 
    /// Several events may occur at a given time in the target VM. For example, there may be more than one breakpoint request for a given location or you might single step to the same location as a breakpoint request. These events are delivered together as a composite event. For uniformity, a composite event is always used to deliver events, even if there is only one event to report.
    /// The events that are grouped in a composite event are restricted in the following ways:
    /// 
    /// Only with other thread start events for the same thread:
    /// - Thread Start Event
    /// 
    /// Only with other thread death events for the same thread:
    /// - Thread Death Event
    /// 
    /// Only with other class prepare events for the same class:
    /// - Class Prepare Event}
    /// 
    /// Only with other class unload events for the same class:
    /// - Class Unload Event
    /// 
    /// Only with other access watchpoint events for the same field access:
    /// - Access Watchpoint Event
    /// 
    /// Only with other modification watchpoint events for the same field modification:
    /// - Modification Watchpoint Event
    /// 
    /// Only with other ExceptionEvents for the same exception occurrance:
    /// - ExceptionEvent
    /// 
    /// Only with other members of this group, at the same location and in the same thread:
    /// - Breakpoint Event
    /// - Step Event
    /// - Method Entry Event
    /// - Method Exit Event
    /// 
    /// The VM Start Event and VM Death Event are automatically generated events. This means they do not need to be requested using the EventRequest.Set command. The VM Start event signals the completion of VM initialization. The VM Death event signals the termination of the VM.If there is a debugger connected at the time when an automatically generated event occurs it is sent from the target VM. Automatically generated events may also be requested using the EventRequest.Set command and thus multiple events of the same event kind will be sent from the target VM when an event occurs.Automatically generated events are sent with the requestID field in the Event Data set to 0. The value of the suspendPolicy field in the Event Data depends on the event. For the automatically generated VM Start Event the value of suspendPolicy is not defined and is therefore implementation or configuration specific. In the Sun implementation, for example, the suspendPolicy is specified as an option to the JDWP agent at launch-time.The automatically generated VM Death Event will have the suspendPolicy set to NONE.
    /// </summary>
    partial class Debugger
    {
        private void HandleCompositeCommand(JdwpPacket packet)
        {
            var data = packet.Data;
            var suspendPolicy = (Jdwp.SuspendPolicy) data.GetByte();
            var count = data.GetInt();

            for (var i = 0; i < count; i++)
            {
                var eventKind = (Jdwp.EventKind) data.GetByte();
                JdwpEvent evt;
                switch (eventKind)
                {
                    case Jdwp.EventKind.VmInit:
                        evt = new VmStart(data);
                        break;
                    case Jdwp.EventKind.SingleStep:
                        evt = new SingleStep(data);
                        break;
                    case Jdwp.EventKind.BreakPoint:
                        evt = new Breakpoint(data);
                        break;
                    case Jdwp.EventKind.MethodEntry:
                        evt = new MethodEntry(data);
                        break;
                    case Jdwp.EventKind.MethodExit:
                        evt = new MethodExit(data);
                        break;
                    case Jdwp.EventKind.Exception:
                        evt = new Exception(data);
                        break;
                    case Jdwp.EventKind.ThreadStart:
                        evt = new ThreadStart(data);
                        break;
                    case Jdwp.EventKind.ThreadEnd:
                        evt = new ThreadDeath(data);
                        break;
                    case Jdwp.EventKind.ClassPrepare:
                        evt = new ClassPrepare(data);
                        break;
                    case Jdwp.EventKind.ClassUnload:
                        evt = new ClassUnload(data);
                        break;
                    case Jdwp.EventKind.FieldAccess:
                        evt = new FieldAccess(data);
                        break;
                    case Jdwp.EventKind.FieldModification:
                        evt = new FieldModification(data);
                        break;
                    case Jdwp.EventKind.VmDeath:
                        evt = new VmDeath(data);
                        break;
                    default:
                        throw new ArgumentException("Unknown event kind in compositive command " + (int)eventKind);
                }
                DLog.Debug(DContext.DebuggerLibDebugger, "JDWP event {0} {1}", eventKind, evt);
                Task.Factory.StartNew(() => {
                    evt.Accept(compositeCommandProcessor, suspendPolicy);
                });
            }
        }
    }
}
