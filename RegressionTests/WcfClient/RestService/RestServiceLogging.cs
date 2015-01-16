using System;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;

using log4net.Ext.EventID;

namespace Dot42
{
    public class RestServiceLogging : IDispatchMessageInspector
    {
        private static readonly IEventIDLog m_Log = EventIDLogManager.GetLogger(typeof(RestServiceLogging));

        private const string m_Unknown = "unknown";
        private const int m_MaxLengthOfMessagesToLog = 8192;
        private const string m_TruncateIndicator = "[...]";

        private readonly string m_ServiceName;

        public RestServiceLogging(string serviceName)
        {
            if(serviceName == null) throw new ArgumentNullException("serviceName");
            m_ServiceName = serviceName;
        }

        #region IDispatchMessageInspector Members

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (m_Log.IsDebugEnabled)
            {
                // Aggregate all data if debug logging level is enabled only.
                 m_Log.Debug(CreateIncomingMessage(request, m_ServiceName));
            }
            
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (m_Log.IsDebugEnabled)
            {
                // Aggregate all data if debug logging level is enabled only.
                m_Log.Debug(CreateOutgoingMessage(ref reply, m_ServiceName));
            } 
        }

        #endregion

        #region Private static helper methods

        private static string CreateIncomingMessage(Message request, string serviceName)
        {
            #region Aggregate Data

            var messageProperties = OperationContext.Current.IncomingMessageProperties;
            var remoteEndpointMessageProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            var sourceAddress = remoteEndpointMessageProperty != null ? remoteEndpointMessageProperty.Address : m_Unknown;
            var sourcePort = remoteEndpointMessageProperty != null ? remoteEndpointMessageProperty.Port.ToString() : m_Unknown;
            
            var targetUri = request.Headers.To.ToString();

            var webOperationContext = WebOperationContext.Current;
            var method = webOperationContext != null ? webOperationContext.IncomingRequest.Method : m_Unknown;
            var authorizationKey = webOperationContext != null ? webOperationContext.IncomingRequest.Headers[HttpRequestHeader.Authorization] : null;

            #endregion

            #region Compose logging message

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(string.Format("-------- Incoming message received on {0}. Details:", serviceName));
            stringBuilder.AppendLine(string.Format("Source address:           {0} ({1})", sourceAddress, sourcePort));
            stringBuilder.AppendLine(string.Format("Target URI:               {0}", targetUri));
            if (method != null) stringBuilder.AppendLine(string.Format("HTTP method:              {0}", method));
            if (authorizationKey != null) stringBuilder.AppendLine(string.Format("Authorization key:        {0}", authorizationKey));
            stringBuilder.AppendLine("Incoming Message:");
            stringBuilder.AppendLine(string.IsNullOrEmpty(request.ToString()) ? "<No incoming message>" : request.ToString());
            stringBuilder.AppendLine("--------");

            #endregion

            return stringBuilder.ToString();
        }

        private static string CreateOutgoingMessage(ref Message reply, string serviceName)
        {
            #region Aggregate Data

            string httpStatusCode;
            string httpStatusDescription;
            var httpResponseMessageProperty = (reply.Properties.ContainsKey(HttpResponseMessageProperty.Name) ? reply.Properties[HttpResponseMessageProperty.Name] : null) as HttpResponseMessageProperty;
            if (httpResponseMessageProperty != null)
            {
                httpStatusCode = ((int) httpResponseMessageProperty.StatusCode).ToString();
                httpStatusDescription = httpResponseMessageProperty.StatusDescription ?? httpResponseMessageProperty.StatusCode.ToString();
            }
            else
            {
                // Do the old behaviour
                var webOperationContext = WebOperationContext.Current;
                httpStatusCode = webOperationContext != null ? ((int)webOperationContext.OutgoingResponse.StatusCode).ToString() : m_Unknown;
                httpStatusDescription = (webOperationContext != null ? webOperationContext.OutgoingResponse.StatusDescription : m_Unknown) ?? webOperationContext.OutgoingResponse.StatusCode.ToString();
            }

            var message = ExtractOutgoingMessage(ref reply);

            if (message.Length > m_MaxLengthOfMessagesToLog)
            {
                //too large to log completely, truncate
                message =  message.Substring(0,m_MaxLengthOfMessagesToLog-m_TruncateIndicator.Length) + m_TruncateIndicator;
            }

            #endregion

            #region Compose logging message

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine();
            stringBuilder.AppendLine(string.Format("-------- Outgoing message sent by {0}. Details:", serviceName));
            stringBuilder.AppendLine(string.Format("HTTP status code:         {0}", httpStatusCode));
            if (httpStatusDescription != null) stringBuilder.AppendLine(string.Format("HTTP status description:  {0}", httpStatusDescription));
            stringBuilder.AppendLine("Outgoing Message:");
            stringBuilder.AppendLine(string.IsNullOrEmpty(message) ? "<No outgoing message>" : message);
            stringBuilder.AppendLine("--------");

            #endregion

            return stringBuilder.ToString();
        }

        private static string ExtractOutgoingMessage(ref Message reply)
        {
            string message;
            try
            {
                var msgBuffer = reply.CreateBufferedCopy(int.MaxValue);
                var copy = msgBuffer.CreateMessage();

                var webBodyFormatMessageProperty = (copy.Properties.ContainsKey(WebBodyFormatMessageProperty.Name) ? copy.Properties[WebBodyFormatMessageProperty.Name] : null) as WebBodyFormatMessageProperty;
                if (webBodyFormatMessageProperty != null)
                {
                    switch (webBodyFormatMessageProperty.Format)
                    {
                        case WebContentFormat.Xml:
                            using (var stringWriter = new StringWriter())
                            using (var xmlTextWriter = new XmlTextWriter(stringWriter))
                            {
                                xmlTextWriter.Formatting = Formatting.Indented;
                                copy.WriteMessage(xmlTextWriter);
                                message = stringWriter.ToString();
                            }
                            break;
                        case WebContentFormat.Raw:
                            using (var xmlDictionaryReader = copy.GetReaderAtBodyContents())
                            {
                                var bytes = xmlDictionaryReader.ReadElementContentAsBase64();
                                message = GetStringFromByteArray(bytes, Encoding.UTF8);
                            }
                            break;
                        case WebContentFormat.Default:
                        case WebContentFormat.Json:
                            message = "<Message format not supported in logging component>";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    // Do the old behaviour
                    using (var stringWriter = new StringWriter())
                    using (var xmlTextWriter = new XmlTextWriter(stringWriter))
                    {
                        xmlTextWriter.Formatting = Formatting.Indented;
                        copy.WriteMessage(xmlTextWriter);
                        message = stringWriter.ToString();
                    }
                }

                reply = msgBuffer.CreateMessage();
                msgBuffer.Close();
            }
            catch (Exception)
            {
                message = string.Empty;
            }

            return message;
        }

        private static string GetStringFromByteArray(byte[] byteArray, Encoding encoding)
        {
            byte[] bom = encoding.GetPreamble();

            if ((bom.Length == 0) || (byteArray.Length < bom.Length))
            {
                return encoding.GetString(byteArray);
            }

            for (var index = 0; index < bom.Length; index++)
            {
                if (byteArray[index] != bom[index])
                {
                    return encoding.GetString(byteArray);
                }
            }

            return encoding.GetString(byteArray, bom.Length, byteArray.Length - bom.Length);
        }

        #endregion
    }
}
