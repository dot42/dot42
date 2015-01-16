using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using Dot42.TodoApi;
using RestInterface;

namespace RestService
{
    public static class ServiceContainer
    {
        private const string _hostAddress1 = "http://localhost:9222/RestService/TodoService";
        private static WebServiceHost _webServiceHost1;

        private const string _hostAddress2 = "http://localhost:9222/RestService/DataService";
        private static WebServiceHost _webServiceHost2;

        public static void Start()
        {
            if (_webServiceHost1 != null || _webServiceHost2 != null)
            {
                Stop();
            }

            _webServiceHost1 = new WebServiceHost(new TodoService(), new Uri(_hostAddress1));
            _webServiceHost1.AddServiceEndpoint(typeof(ITodoApi), new WebHttpBinding { MaxReceivedMessageSize = int.MaxValue }, _hostAddress1);

            _webServiceHost1.Open();

            _webServiceHost2 = new WebServiceHost(new DataService(), new Uri(_hostAddress2));
            _webServiceHost2.AddServiceEndpoint(typeof(IDataTest), new WebHttpBinding { MaxReceivedMessageSize = int.MaxValue }, _hostAddress2);
            _webServiceHost2.Open();
        }

        public static void Stop()
        {
            try
            {
                _webServiceHost1.Close();
            }
            catch
            {
                //noting to do
            }
            finally
            {
                _webServiceHost1 = null;
            }

            try
            {
                _webServiceHost2.Close();
            }
            catch
            {
                //noting to do
            }
            finally
            {
                _webServiceHost2 = null;
            }
        }
    }
}
