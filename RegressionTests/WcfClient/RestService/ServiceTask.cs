using System;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace RestService
{
	[TaskName("rest-service")]
    public class ServiceTask : Task
    {
        [TaskAttribute("action", Required = true)]
        public string Action { get; set; }

        protected override void ExecuteTask()
        {
            switch (Action)
            {
                case "start":
                    ServiceContainer.Start();
					Console.WriteLine("Service started");
                    break;
                case "stop":
                    ServiceContainer.Stop();
					Console.WriteLine("Service stopped");
                    break;
                default:
                    throw new ArgumentException("Unknown action: " + Action);
            }
        }
    }
}
