using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Os;
using Android.Widget;

using Dot42;
using Dot42.Manifest;
using Dot42.TodoApi;
using Dot42.TodoApi.Version_1_0;

namespace WcfClient
{
	[Activity(Label = "dot42 Wcf Todo Client Async")]
	public class MainActivity : Activity
	{
		// ***************************** A CHANGE IS NEEDED ******************************
		// Change the IP address in this URL to the IP address of the computer running the TodoServer.
		// ***************************** A CHANGE IS NEEDED ******************************
        private const string _hostAddress = "http://192.168.140.96:9222/RestService/TodoService";

		private TextView versionTextView;
		private ListView todoListView;

		/// <summary>
		/// Create and prepare the activity.
		/// </summary>
		protected async override void OnCreate(Bundle savedInstance)
		{
			base.OnCreate(savedInstance);
			SetContentView(R.Layouts.MainLayout);

			// Find all views and connect to them
			versionTextView = FindViewById<TextView>(R.Ids.Version);
			todoListView = FindViewById<ListView>(R.Ids.TodoList);
			var addButton = FindViewById<Button>(R.Ids.AddButton);
            addButton.Click += (s, x) => AddRandomTodoAsync();

            //set the this as synchronization context, see OS\AsyncAndActivityInstances sample for details
            SynchronizationContext.SetSynchronizationContext(this);

            //Use ConfigureAwait to set the this as synchronization context, see OS\AsyncAndActivityInstances sample for details
            var versionTask = Task<VersionType>.Factory.StartNew(GetVersion).ConfigureAwait(this);
            var todoTask = Task<List<string>>.Factory.StartNew(GetTodosAsStrings).ConfigureAwait(this);

		    try
		    {
                var versionType = await versionTask;
                UpdateVersionView("Version:" + versionType.ToString(), false);
		    }
		    catch (Exception ex)
		    {
                UpdateVersionView(ex.Message, true);   
		    }

            try
            {
                var todos = await todoTask;
                UpdateTodoView(todos);
            }
            catch (Exception ex)
            {
                UpdateVersionView(ex.Message, true);
            }		  		   
		}

        /// <summary>
        /// Add a random item.
        /// The actual work in done on a background thread because it involves a webrequest.
        /// </summary>
        private async void AddRandomTodoAsync()
        {
            try
            {
                await Task.Factory.StartNew(() => AddTodos("Some name", DateTime.Now));

                var todos = await Task<List<string>>.Factory.StartNew(GetTodosAsStrings).ConfigureAwait(this);;
                UpdateTodoView(todos);
            }
            catch (Exception ex)
            {
                UpdateVersionView(ex.Message, true);
            }
        }

	
		/// <summary>
		/// Update the UI (on the main thread) with information loaded from the server.
		/// </summary>
		private void UpdateVersionView(string message, bool exception)
		{
		    if (exception) message = "An error occurred: " + message;
			versionTextView.SetText(message);	
		}

        /// <summary>
        /// Update the UI (on the main thread) with information loaded from the server.
        /// </summary>
        private void UpdateTodoView(List<string> todoItems)
        {
            if (todoItems != null)
            {
                todoListView.SetAdapter(new ArrayAdapter<string>(this, Android.R.Layout.Simple_list_item_1, todoItems.ToArray()));
                todoListView.InvalidateViews();
            }
        }
	
		/// <summary>
		/// Perform a ITodoApi GetVersion request.
		/// </summary>
		private VersionType GetVersion()
		{
			using (var client = new WebChannelFactory<ITodoApi>(new WebHttpBinding(), new Uri(_hostAddress)))
			{
				client.Open();
				var channel = client.CreateChannel();

				using (new OperationContextScope((IContextChannel)channel))
				{
					return channel.GetVersion();
				}
			}
		}

        private List<string> GetTodosAsStrings()
        {
            var todos = GetTodos();
            var strings = new List<string>();
            if ((todos != null) && (todos.Todo != null))
            {
                strings.AddRange(todos.Todo.Select(todo => string.Format("Name={0} IsDone={1}", todo.Name, todo.IsDone)));
            }

            return strings;
        }

		/// <summary>
		/// Perform a ITodoApi GetTodos request.
		/// </summary>
		private TodosType GetTodos()
		{
			using (var client = new WebChannelFactory<ITodoApi>(new WebHttpBinding(), new Uri(_hostAddress)))
			{
				client.Open();
				var channel = client.CreateChannel();

				using (new OperationContextScope((IContextChannel)channel))
				{
					return channel.GetTodos();
				}
			}
		}

		/// <summary>
		/// Perform a ITodoApi AddTodos request.
		/// </summary>
		private void AddTodos(string name, DateTime expirationDate)
		{
			using (var client = new WebChannelFactory<ITodoApi>(new WebHttpBinding(), new Uri(_hostAddress)))
			{
				client.Open();
				var channel = client.CreateChannel();

				using (new OperationContextScope((IContextChannel)channel))
				{
					channel.CreateTodo(new TodoType() { Name = name, ExpirationDate = expirationDate });
				}
			}
		}
	}
}
