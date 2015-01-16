using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Dot42
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RestServiceLoggingAttribute : Attribute, IServiceBehavior
    {
        private readonly string m_ServiceName;

        public RestServiceLoggingAttribute(string serviceName)
        {
            if (serviceName == null) throw new ArgumentNullException("serviceName");
            m_ServiceName = serviceName;
        }

        #region IServiceBehaviour Members

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            // deliberately left empty
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            // deliberately left empty
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach(ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                if (channelDispatcher != null)
                {
                    foreach (var endpointDispatcher in channelDispatcher.Endpoints)
                    {
                        endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new RestServiceLogging(m_ServiceName));
                    }
                }
            }
        }

        #endregion
    }
}
