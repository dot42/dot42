using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Net;

using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using Dot42;
using log4net.Config;
using log4net.Ext.EventID;

using Dot42.TodoApi;
using Dot42.TodoApi.Version_1_0;

namespace RestService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    [RestServiceLogging("Todo Service")]
    public class TodoService : ITodoApi
    {
        /// <summary>
        /// Logging component 
        /// </summary>
        private static readonly IEventIDLog _log = EventIDLogManager.GetLogger(typeof(TodoService));

        private readonly List<TodoType> _todos = new List<TodoType>();

        static TodoService()
        {
            var folder = Thread.GetDomain().BaseDirectory;
            var filename = Path.GetFileName(Assembly.GetExecutingAssembly().Location) + ".config";
            var fullPath = Path.Combine(folder, filename);

            XmlConfigurator.ConfigureAndWatch(new FileInfo(fullPath));
        }


        public TodoService()
        {
            Init();
        }

        private void Init()
        {
            _todos.Clear();

            _todos.Add(new TodoType { id = 1, idSpecified = true, IsDone = false, Name = "First Item", Description = "This items should be fixed!", ExpirationDate = DateTime.Now.AddHours(2) });
            _todos.Add(new TodoType { id = 2, idSpecified = true, IsDone = false, Name = "Second Item", Description = "This items should be fixed!", ExpirationDate = DateTime.Now.AddDays(1) });
            _todos.Add(new TodoType { id = 3, idSpecified = true, IsDone = true, Name = "Third Item", Description = "This items should be fixed!", ExpirationDate = DateTime.Now.AddHours(2) });


        }

        #region Implementation of ITodoApi

        public VersionType GetVersion()
        {
            try
            {
                Init();

                const VersionType version = VersionType.v1_0;
                
                SetHttpStatus(HttpStatusCode.OK, HttpHeaderMessages.VersionReturned);
                return version;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error retrieving version. Details: {0}", ex);
                SetHttpStatus(HttpStatusCode.InternalServerError, HttpHeaderMessages.InternalServerError);
                return VersionType.Unknown;
            }
        }


        public TodosType GetTodos()
        {
           try
            {
                var todos = new TodosType { Todo = _todos.ToArray() };
                
               // Return success
                SetHttpStatus(HttpStatusCode.OK, HttpHeaderMessages.TodoReturned);

                return todos;

            }
           catch (Exception ex)
           {
               _log.ErrorFormat("Error retrieving version. Details: {0}", ex);
               SetHttpStatus(HttpStatusCode.InternalServerError, HttpHeaderMessages.InternalServerError);
               return null;
           }
        }

        public TodosType GetTodosFiltered(bool isDone)
        {
            try
            {
                var todos = new TodosType { Todo = _todos.Where(td =>td.IsDone == isDone).ToArray() };

                // Return success
                SetHttpStatus(HttpStatusCode.OK, HttpHeaderMessages.TodoReturned);

                return todos;

            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error retrieving version. Details: {0}", ex);
                SetHttpStatus(HttpStatusCode.InternalServerError, HttpHeaderMessages.InternalServerError);
                return null;
            }
        }

        public TodoType GetTodo(string todoId)
        {
            try
            {
                int todoIdAsInt;
                if (!int.TryParse(todoId, out todoIdAsInt))
                {
                    SetHttpStatus(HttpStatusCode.BadRequest, HttpHeaderMessages.InvalidIdSpecified);
                    return null;
                }

                var existing = _todos.FirstOrDefault(td => td.id == todoIdAsInt);
                if (existing == null)
                {
                    SetHttpStatus(HttpStatusCode.NotFound, HttpHeaderMessages.TodoNotFound);
                    return null;
                }

                // Return success
                SetHttpStatus(HttpStatusCode.OK, HttpHeaderMessages.TodoReturned);

                return existing;

            }
            catch (Exception exception)
            {
                _log.ErrorFormat("Error processing DeleteTodo : {0}", exception);
                SetHttpStatus(HttpStatusCode.InternalServerError, HttpHeaderMessages.InternalServerError);
                return null;
            }
        }

        public int CreateTodo(TodoType todo)
        {
            try
            {
                #region Validate XML message

                if (!IsValidMessage())
                {
                    SetHttpStatus(HttpStatusCode.BadRequest, HttpHeaderMessages.XsdValidationFailed);
                    return 0;
                }

                #endregion

                if (todo.idSpecified)
                {
                    SetHttpStatus(HttpStatusCode.BadRequest, HttpHeaderMessages.IdSpecified);
                    return 0;
                }

                var newId = _todos.Max(td => td.id) + 1;

                todo.id = newId;
                _todos.Add(todo);

                // Return success
                SetHttpStatus(HttpStatusCode.OK, HttpHeaderMessages.TodoUpdatedOrCreated);

                return newId;
            }
            catch (Exception exception)
            {
                _log.ErrorFormat("Error processing CreateTodo : {0}", exception);
                SetHttpStatus(HttpStatusCode.InternalServerError, HttpHeaderMessages.InternalServerError);
                return 0;
            }
        }

        public void UpdateTodo(string todoId, TodoType todo)
        {
            try
            {
                #region Validate XML message

                if (!IsValidMessage())
                {
                    SetHttpStatus(HttpStatusCode.BadRequest, HttpHeaderMessages.XsdValidationFailed);
                    return;
                }

                #endregion

                int todoIdAsInt;
                if( !int.TryParse(todoId, out todoIdAsInt))
                {
                    SetHttpStatus(HttpStatusCode.BadRequest, HttpHeaderMessages.InvalidIdSpecified);
                    return;
                }

                if (!todo.idSpecified || todo.id != todoIdAsInt)
                {
                    SetHttpStatus(HttpStatusCode.BadRequest, HttpHeaderMessages.InvalidIdSpecified);
                    return;
                }

                var existing = _todos.First(td => td.id == todoIdAsInt);
                _todos.Remove(existing);

                _todos.Add(todo);

                // Return success
                SetHttpStatus(HttpStatusCode.OK, HttpHeaderMessages.TodoUpdatedOrCreated);
            }
            catch (Exception exception)
            {
                _log.ErrorFormat("Error processing CreateTodo : {0}", exception);
                SetHttpStatus(HttpStatusCode.InternalServerError, HttpHeaderMessages.InternalServerError);
            }
        }

        public void DeleteTodo(string todoId)
        {
            try
            {
                int todoIdAsInt;
                if (!int.TryParse(todoId, out todoIdAsInt))
                {
                    SetHttpStatus(HttpStatusCode.BadRequest, HttpHeaderMessages.InvalidIdSpecified);
                    return;
                }

                var existing = _todos.FirstOrDefault(td => td.id == todoIdAsInt);
                if(existing!=null) _todos.Remove(existing);

                // Return success
                SetHttpStatus(HttpStatusCode.OK, HttpHeaderMessages.TodoDeleted);
            }
             catch (Exception exception)
             {
                 _log.ErrorFormat("Error processing DeleteTodo : {0}", exception);
                 SetHttpStatus(HttpStatusCode.InternalServerError, HttpHeaderMessages.InternalServerError);
             }
        }

        public void Foo()
        {
            
        }

        #endregion

        #region REST related

        private static void SetHttpStatus(HttpStatusCode statusCode, string message)
        {
            if (WebOperationContext.Current == null)
            {
                throw new Exception("Unable to get current WebOperationContext.");
            }

            WebOperationContext.Current.OutgoingResponse.StatusCode = statusCode;
            WebOperationContext.Current.OutgoingResponse.StatusDescription = message;
        }

        private bool IsValidMessage()
        {
            var currentOperationContext = OperationContext.Current;

            if (currentOperationContext == null)
            {
                throw new Exception("Failed to determine current WebOperationContext");
            }

            if (currentOperationContext.RequestContext == null || currentOperationContext.RequestContext.RequestMessage == null || currentOperationContext.RequestContext.RequestMessage.IsEmpty)
            {
                // This message does not contain a body
                return false;
            }

            string xmlMessage = currentOperationContext.RequestContext.RequestMessage.ToString();

            if (string.IsNullOrEmpty(xmlMessage))
            {
                // The message is empty
                return false;
            }

            //TODO: add code which checks the xmlMessage according the XSD.
            return true;
        }

        #endregion
    }
}
