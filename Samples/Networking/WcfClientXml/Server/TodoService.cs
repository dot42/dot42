using System;
using System.Collections.Generic;
using System.Linq;

using System.Net;

using System.ServiceModel;
using System.ServiceModel.Web;

using Dot42.TodoApi;
using Dot42.TodoApi.Version_1_0;

namespace RestService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TodoService : ITodoApi
    {
        private readonly List<TodoType> _todos = new List<TodoType>();
        private readonly System.Windows.Forms.TextBox message;

        public TodoService(System.Windows.Forms.TextBox message)
        {
            this.message = message;

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
                message.Text = "GetVersion";

                const VersionType version = VersionType.v1_0;
                
                SetHttpStatus(HttpStatusCode.OK, HttpHeaderMessages.VersionReturned);
                return version;
            }
            catch 
            {
                SetHttpStatus(HttpStatusCode.InternalServerError, HttpHeaderMessages.InternalServerError);
                return VersionType.Unknown;
            }
        }


        public TodosType GetTodos()
        {
           try
            {
                message.Text = "GetTodos";

                var todos = new TodosType { Todo = _todos.ToArray() };
                
               // Return success
                SetHttpStatus(HttpStatusCode.OK, HttpHeaderMessages.TodoReturned);

                return todos;

            }
           catch
           {
               SetHttpStatus(HttpStatusCode.InternalServerError, HttpHeaderMessages.InternalServerError);
               return null;
           }
        }

        public TodosType GetTodosFiltered(bool isDone)
        {
            try
            {
                message.Text = "GetTodos";

                var todos = new TodosType { Todo = _todos.Where(td =>td.IsDone == isDone).ToArray() };

                // Return success
                SetHttpStatus(HttpStatusCode.OK, HttpHeaderMessages.TodoReturned);

                return todos;

            }
            catch 
            {
                SetHttpStatus(HttpStatusCode.InternalServerError, HttpHeaderMessages.InternalServerError);
                return null;
            }
        }

        public TodoType GetTodo(string todoId)
        {
            try
            {
                message.Text = "GetTodo";

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
            catch
            {
                SetHttpStatus(HttpStatusCode.InternalServerError, HttpHeaderMessages.InternalServerError);
                return null;
            }
        }

        public int CreateTodo(TodoType todo)
        {
            try
            {
                message.Text = "CreateTodo";

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
            catch
            {
                SetHttpStatus(HttpStatusCode.InternalServerError, HttpHeaderMessages.InternalServerError);
                return 0;
            }
        }

        public void UpdateTodo(string todoId, TodoType todo)
        {
            try
            {
                message.Text = "UpdateTodo";

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
            catch 
            {
                SetHttpStatus(HttpStatusCode.InternalServerError, HttpHeaderMessages.InternalServerError);
            }
        }

        public void DeleteTodo(string todoId)
        {
            try
            {
                message.Text = "DeleteTodo";

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
             catch 
             {
                 SetHttpStatus(HttpStatusCode.InternalServerError, HttpHeaderMessages.InternalServerError);
             }
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

        #endregion
    }
}
