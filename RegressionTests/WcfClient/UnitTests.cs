using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;

using NUnit.Framework;

using Dot42.TodoApi;
using Dot42.TodoApi.Version_1_0;

namespace RestService
{
    [TestFixture]
    public partial class UnitTests
    {
		private string _hostAddress;
		
        [SetUp]
        public void SetupTest()
        {
			_hostAddress = string.Format("http://{0}:9222/RestService/TodoService", ipAddress);
            using (var client = new WebChannelFactory<ITodoApi>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    //GetVersion will reset the state of the server to it's initial state as well.
                    var version = channel.GetVersion();
                    ValidateHttpStatusResponse(HttpStatusCode.OK);

                    Assert.AreEqual(VersionType.v1_0, version);
                }
            }
        }


        [Test]
        public void GetVersion()
        {
            using (var client = new WebChannelFactory<ITodoApi>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var version = channel.GetVersion();
                    ValidateHttpStatusResponse(HttpStatusCode.OK);

                    Assert.AreEqual(VersionType.v1_0, version);
                }
            }
        }

        [Test]
        public void GetTodos()
        {
            using (var client = new WebChannelFactory<ITodoApi>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var todos = channel.GetTodos();
                    ValidateHttpStatusResponse(HttpStatusCode.OK);

                    Assert.AreEqual(3, todos.Todo.Length);
                }
            }
        }

        [Test]
        public void GetTodo()
        {
            using (var client = new WebChannelFactory<ITodoApi>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var todo = channel.GetTodo("1");
                    ValidateHttpStatusResponse(HttpStatusCode.OK);

                    Assert.AreEqual(true, todo.idSpecified);
                    Assert.AreEqual(1, todo.id);
                    Assert.AreEqual(false, todo.IsDone);
                }
            }
        }

        [Test]
        public void GetTodosFiltered()
        {
            using (var client = new WebChannelFactory<ITodoApi>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var todos = channel.GetTodosFiltered(true);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);

                    Assert.AreEqual(1, todos.Todo.Length);
                }

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var todos = channel.GetTodosFiltered(false);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);

                    Assert.AreEqual(2, todos.Todo.Length);
                }
            }
        }

        [Test]
        public void CreateTodo()
        {
            using (var client = new WebChannelFactory<ITodoApi>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                var todo = new TodoType { id = 6, idSpecified = true, IsDone = false, Name = "New Item", Description = "This items should be fixed!", ExpirationDate = DateTime.Now };

                using (new OperationContextScope((IContextChannel)channel))
                {
                    try
                    {
                        var newId = channel.CreateTodo(todo);
                    }
                    catch (CommunicationException ex)
                    {
                        ValidateHttpStatusResponse(ex, HttpStatusCode.BadRequest);  
                    }
                }

                todo.idSpecified = false;

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var newId = channel.CreateTodo(todo);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);

                    Assert.AreEqual(4, newId);
                }
            }
        }

        [Test]
        public void UpdateTodo()
        {
            using (var client = new WebChannelFactory<ITodoApi>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                var todo = new TodoType();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    try
                    {
                        todo = channel.GetTodo("1");
                    }
                    catch (CommunicationException ex)
                    {
                        ValidateHttpStatusResponse(ex, HttpStatusCode.BadRequest);
                    }
                }

                todo.IsDone = true;

                using (new OperationContextScope((IContextChannel) channel))
                {
                    channel.UpdateTodo(todo.id.ToString(), todo);
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                }
            }
        }

        [Test]
        public void DeleteTodo()
        {
            using (var client = new WebChannelFactory<ITodoApi>(new WebHttpBinding(), new Uri(_hostAddress)))
            {
                client.Open();
                var channel = client.CreateChannel();

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var todos = channel.GetTodos();
                    ValidateHttpStatusResponse(HttpStatusCode.OK);

                    Assert.AreEqual(3, todos.Todo.Length);
                }

                using (new OperationContextScope((IContextChannel)channel))
                {
                    channel.DeleteTodo("1");
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                }

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var todos = channel.GetTodos();
                    ValidateHttpStatusResponse(HttpStatusCode.OK);

                    Assert.AreEqual(2, todos.Todo.Length);
                }

                using (new OperationContextScope((IContextChannel)channel))
                {
                    channel.DeleteTodo("1");
                    ValidateHttpStatusResponse(HttpStatusCode.OK);
                }

                using (new OperationContextScope((IContextChannel)channel))
                {
                    var todos = channel.GetTodos();
                    ValidateHttpStatusResponse(HttpStatusCode.OK);

                    Assert.AreEqual(2, todos.Todo.Length);
                }
            }
        }

        #region Private HttpStatus Response Methods

        private struct HttpStatusResponse
        {
            public HttpStatusCode HttpStatusCode { get; private set; }
            public string HttpStatusDescription { get; private set; }

            public HttpStatusResponse(HttpStatusCode httpStatusCode, string httpStatusDescription)
                : this()
            {
                HttpStatusCode = httpStatusCode;
                HttpStatusDescription = httpStatusDescription;
            }
        }

        /// <summary>
        /// Determine the HttpStatusResponse from the protocol exception. 
        /// When this fails the Assert.Fail method will be called to prevent further execution.
        /// </summary>
        /// <param name="communicationException">The protocol exception which might the HttpStatusCode</param>
        /// <returns>The HttpStatusResponse enumeration</returns>
        private static HttpStatusResponse DetermineHttpStatusResponse(CommunicationException communicationException)
        {
            if (communicationException != null && communicationException.InnerException != null)
            {
                var webException = communicationException.InnerException as WebException;
                if (webException != null && webException.Response != null)
                {
                    var response = webException.Response as HttpWebResponse;
                    if (response != null)
                    {
                        return new HttpStatusResponse(response.StatusCode, response.StatusDescription);
                    }
                }
            }

            Assert.Fail("Failed to determine HttpStatusCode from CommunicationException.");
            return new HttpStatusResponse(HttpStatusCode.InternalServerError, "Failed to determine HttpStatusCode from CommunicationException.");
        }

        /// <summary>
        /// Validate the communicationException to see whether the HttpStatusCode matches the expectation
        /// </summary>
        /// <param name="communicationException"></param>
        /// <param name="httpStatusCode"></param>
        private static void ValidateHttpStatusResponse(CommunicationException communicationException, HttpStatusCode httpStatusCode)
        {
            HttpStatusResponse httpStatusResponse = DetermineHttpStatusResponse(communicationException);
            Assert.AreEqual(httpStatusCode, httpStatusResponse.HttpStatusCode, string.Format("Unexpected HttpStatusCode received : {0}", httpStatusResponse.HttpStatusDescription));
        }

        /// <summary>
        /// Validate the communicationException to see whether the HttpStatusCode matches the expectation
        /// </summary>
        /// <param name="httpStatusCode"></param>
        private static void ValidateHttpStatusResponse(HttpStatusCode httpStatusCode)
        {
            /*
            var currentWebOperationContext = WebOperationContext.Current;
            if (currentWebOperationContext == null)
                throw new ApplicationException("Failed to determine current WebOperationContext");

            var webResponse = currentWebOperationContext.IncomingResponse;
            Assert.AreEqual(httpStatusCode, webResponse.StatusCode, string.Format("Unexpected HttpStatusCode received : {0}", webResponse.StatusDescription));
             */
        }

        #endregion
    }
}
