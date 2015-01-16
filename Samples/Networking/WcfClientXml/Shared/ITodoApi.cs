using System.ServiceModel;
using System.ServiceModel.Web;

using Dot42.TodoApi.Version_1_0;

namespace Dot42.TodoApi
{
    [ServiceContract]
    [XmlSerializerFormat]
    public interface ITodoApi
    {
        [OperationContract]
        [WebGet(UriTemplate = "/Version")]
        VersionType GetVersion();

        [OperationContract]
        [WebGet(UriTemplate = "/Todo", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        TodosType GetTodos();

        [OperationContract]
        [WebGet(UriTemplate = "/Todo?IsDone={isDone}")]
        TodosType GetTodosFiltered(bool isDone);

        [OperationContract]
        [WebGet(UriTemplate = "/Todo/{todoId}")]
        TodoType GetTodo(string todoId);

        [OperationContract]
        [WebInvoke(UriTemplate = "/Todo")]
        int CreateTodo(TodoType todo);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/Todo/{todoId}")]
        void UpdateTodo(string todoId, TodoType todo);

        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/Todo/{todoId}")]
        void DeleteTodo(string todoId);

    }
}
