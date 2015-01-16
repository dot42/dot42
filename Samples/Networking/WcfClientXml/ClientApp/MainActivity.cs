using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Web;

using Android.App;
using Android.Os;
using Android.Widget;

using Dot42;
using Dot42.Manifest;
using Dot42.TodoApi;
using Dot42.TodoApi.Version_1_0;

namespace WcfClient
{
	[Activity(Label = "dot42 Wcf Todo Client")]
	public class MainActivity : Activity
	{
		// ***************************** A CHANGE IS NEEDED ******************************
		// Change the IP address in this URL to the IP address of the computer running the TodoServer.
		// ***************************** A CHANGE IS NEEDED ******************************
		private const string _hostAddress = "http://192.168.140.64:9222/RestService/TodoService";

		private BackgroundWorker worker;
		private TextView versionTextView;
		private ListView todoListView;
		private string version;
		private List<string> todoItems;
	    private string errorMessage = "";

		/// <summary>
		/// Create and prepare the activity.
		/// </summary>
		protected override void OnCreate(Bundle savedInstance)
		{
			base.OnCreate(savedInstance);
			SetContentView(R.Layouts.MainLayout);

			// Find all views and connect to them
			versionTextView = FindViewById<TextView>(R.Ids.Version);
			todoListView = FindViewById<ListView>(R.Ids.TodoList);
			var addButton = FindViewById<Button>(R.Ids.AddButton);
			addButton.Click += (s, x) => AddRandomTodo();

			// Prepare async worker to load items
			worker = new BackgroundWorker();
			worker.DoWork += OnLoadValues;
			worker.RunWorkerCompleted += OnUpdateView;
			
			// Start a refresh of the items
			RefreshItems();
		}
		
		/// <summary>
		/// Trigger a load request of the todo items.
		/// This is done in a background worker, since web requests cannot be made on the main thread.
		/// </summary>
		private void RefreshItems() 
		{
			worker.RunWorkerAsync();		
		}

		/// <summary>
		/// Load todo items on the background thread.
		/// </summary>
		private void OnLoadValues(object sender, DoWorkEventArgs doWorkEventArgs)
		{
			todoItems = null;
			version = null;

		    try
		    {
		        version = GetVersion().ToString();
		        var todos = GetTodos();
		        if ((todos != null) && (todos.Todo != null))
		        {
		            var strings = new List<string>();
		            foreach (var todo in todos.Todo)
		            {
		                strings.Add(string.Format("Name={0} IsDone={1}", todo.Name, todo.IsDone));
		            }
		            todoItems = strings;
		        }
		    }
		    catch (Exception ex)
		    {
		        errorMessage = ex.Message;
		    }

		}

		/// <summary>
		/// Update the UI (on the main thread) with information loaded from the server.
		/// </summary>
		private void OnUpdateView(object sender, RunWorkerCompletedEventArgs args)
		{
			if ((version != null) && (todoItems != null))
			{
				versionTextView.SetText(version);
				todoListView.SetAdapter(new ArrayAdapter<string>(this, Android.R.Layout.Simple_list_item_1, todoItems.ToArray()));
				todoListView.InvalidateViews();
			} else 
			{
				versionTextView.SetText("An error occurred: " + errorMessage );
			}
		}
		
		/// <summary>
		/// Add a random item.
		/// The actual work in done on a background thread because it involves a webrequest.
		/// </summary>
		private void AddRandomTodo()
		{
			var addWorker = new BackgroundWorker();
			addWorker.DoWork += (s, x) => AddTodos("Some name", DateTime.Now);
			addWorker.RunWorkerCompleted += (s, x) => RefreshItems();
			addWorker.RunWorkerAsync();
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
