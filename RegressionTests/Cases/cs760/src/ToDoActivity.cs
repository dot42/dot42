using Java.Lang;
using Test1;

using Java.Net;
using Java.Util;

using Android.App;
using Android.Os;
using Android.View;
using Android.Widget;

using Com.Microsoft.Windowsazure.Mobileservices;

using Dot42.Manifest;

#warning: how should I port: AllowBackup?
[assembly: Application("@string/app_name", Icon = "@drawable/ic_launcher", Theme = "@style/AppTheme" )] //, AllowBackup = true)]
[assembly: UsesPermission("android.permission.INTERNET")]

namespace com.example.stamware
{
    [Activity(Label = "@string/app_name")]
    [IntentFilter(Actions = new[] { "android.intent.action.MAIN" }, Categories = new[] { "android.intent.category.LAUNCHER" })]
    public class ToDoActivity : Activity 
    {
        private class MyUpdateTableOperationCallback : ITableOperationCallback<ToDoItem>
        {
            private ToDoActivity mToDoActivity;

            public MyUpdateTableOperationCallback(ToDoActivity toDoActivity)
            {
                mToDoActivity = toDoActivity;
            }

            public void OnCompleted (ToDoItem entity, System.Exception exception, IServiceFilterResponse response)
            {
                if (exception == null)
                {
                    if (entity.IsComplete())
                    {
                        mToDoActivity.mAdapter.Remove(entity);
                    }
                }
                else
                {
                    mToDoActivity.CreateAndShowDialog(exception, "Error");
                }
            }
        }

        private class MyInsertTableOperationCallback : ITableOperationCallback<ToDoItem>
        {
            private ToDoActivity mToDoActivity;

            public MyInsertTableOperationCallback(ToDoActivity toDoActivity)
            {
                mToDoActivity = toDoActivity;
            }

            public void OnCompleted(ToDoItem entity, System.Exception exception, IServiceFilterResponse response)
            {
                if (exception == null)
                {
                    if (!entity.IsComplete())
                    {
                        mToDoActivity.mAdapter.Add(entity);
                    }
                }
                else
                {
                    mToDoActivity.CreateAndShowDialog(exception, "Error");
                }

            }
        }

        private class MyTableQueryCallback : ITableQueryCallback<ToDoItem>
        {
            private ToDoActivity mToDoActivity;

            public MyTableQueryCallback(ToDoActivity toDoActivity)
            {
                mToDoActivity = toDoActivity;
            }

            public void OnCompleted(IList<ToDoItem> result, int count, System.Exception exception, IServiceFilterResponse response)
            {
                if (exception == null)
                {
                    mToDoActivity.mAdapter.Clear();
#warning: why isn't foreach working?
                    //foreach (ToDoItem item in result)
                    //{
                    //    mToDoActivity.mAdapter.Add(item);
                    //}
                    for (int i = 0; i < result.Count; i++)
                    {
                        mToDoActivity.mAdapter.Add(result[i]);
                    }

                }
                else
                {
                    mToDoActivity.CreateAndShowDialog(exception, "Error");
                }
            }
        }

        private class ProgressFilter : IServiceFilter 
        {
		    private class MyRunnable: IRunnable
		     {
		         private int mVisiblity;
		         private ProgressBar mProgressBar;

		         public MyRunnable(ProgressBar mProgressBar, int visibility)
		         {
		             mVisiblity = visibility;
		         }

				public void Run() {
					if (mProgressBar != null) mProgressBar.SetVisibility(mVisiblity);
				}
			}

            private class MyServiceFilterResponseCallback: IServiceFilterResponseCallback 
            {
                private ToDoActivity mToDoActivity;
                private IServiceFilterResponseCallback mResponseCallback;

                public MyServiceFilterResponseCallback(ToDoActivity toDoActivity, IServiceFilterResponseCallback responseCallback)
                {
                    mToDoActivity = toDoActivity;
                    mResponseCallback = responseCallback;
                }

                public void OnResponse(IServiceFilterResponse response, System.Exception exception)
                {
                    mToDoActivity.RunOnUiThread(new MyRunnable(mToDoActivity.mProgressBar, ProgressBar.GONE));
					
					if (mResponseCallback != null)  mResponseCallback.OnResponse(response, exception);
				}
            }

            private ToDoActivity mToDoActivity;

            public ProgressFilter(ToDoActivity toDoActivity)
            {
                mToDoActivity = toDoActivity;
            }

		    public void HandleRequest(IServiceFilterRequest request, INextServiceFilterCallback nextServiceFilterCallback,
				    IServiceFilterResponseCallback responseCallback) 
            {
                mToDoActivity.RunOnUiThread(new MyRunnable(mToDoActivity.mProgressBar, ProgressBar.VISIBLE));

                nextServiceFilterCallback.OnNext(request, new MyServiceFilterResponseCallback(mToDoActivity, responseCallback));
		    }
	    }

        /**
	     * Mobile Service Client reference
	     */
	    private MobileServiceClient mClient;

	    /**
	     * Mobile Service Table used to access data
	     */
	    private MobileServiceTable<ToDoItem> mToDoTable;

	    /**
	     * Adapter to sync the items list with the view
	     */
	    private ToDoItemAdapter mAdapter;

	    /**
	     * EditText containing the "New ToDo" text
	     */
	    private EditText mTextNewToDo;

	    /**
	     * Progress spinner to use for table operations
	     */
	    private ProgressBar mProgressBar;

	    /**
	     * Initializes the activity
	     */
	    protected override void OnCreate(Bundle savedInstanceState) {
		    base.OnCreate(savedInstanceState);
		    SetContentView(R.Layouts.activity_to_do);
		
		    mProgressBar = (ProgressBar) FindViewById(R.Ids.loadingProgressBar);

		    // Initialize the progress bar
		    mProgressBar.SetVisibility(ProgressBar.GONE);
		
		    try {
			    // Create the Mobile Service Client instance, using the provided
			    // Mobile Service URL and key
			    mClient = new MobileServiceClient(
					    "https://stamware.azure-mobile.net/",
					    "uPPxchIAQYbawiaTKKfmrjsYjCNVPj40",
					    this).WithFilter(new ProgressFilter(this));

			    // Get the Mobile Service Table instance to use
                mToDoTable = mClient.GetTable<ToDoItem>(typeof(ToDoItem));

			    mTextNewToDo = (EditText) FindViewById(R.Ids.textNewToDo);

			    // Create an adapter to bind the items with the view
			    mAdapter = new ToDoItemAdapter(this, R.Layouts.row_list_to_do);
			    ListView listViewToDo = (ListView) FindViewById(R.Ids.listViewToDo);
			    listViewToDo.SetAdapter(mAdapter);
		
			    // Load the items from the Mobile Service
			    RefreshItemsFromTable();

		    } catch (MalformedURLException e) {
			    CreateAndShowDialog(new Exception("There was an error creating the Mobile Service. Verify the URL"), "Error");
		    }
	    }
	
	    /**
	     * Initializes the activity menu
	     */
	    public override bool OnCreateOptionsMenu(IMenu menu) {
		    GetMenuInflater().Inflate(R.Menus.activity_main, menu);
		    return true;
	    }
	
	    /**
	     * Select an option from the menu
	     */
	    public override bool OnOptionsItemSelected(IMenuItem item) {
		    if (item.GetItemId() == R.Ids.menu_refresh) {
			    RefreshItemsFromTable();
		    }
		
		    return true;
	    }

	    /**
	     * Mark an item as completed
	     * 
	     * @param item
	     *            The item to mark
	     */
	    public void CheckItem(ToDoItem item) {
		    if (mClient == null) {
			    return;
		    }

		    // Set the item as completed and update it in the table
		    item.SetComplete(true);
		
		    mToDoTable.Update(item, new MyUpdateTableOperationCallback(this));
	    }

	    /**
	     * Add a new item
	     * 
	     * @param view
	     *            The view that originated the call
	     */
	    public void AddItem(View view) {
		    if (mClient == null) {
			    return;
		    }

		    // Create a new item
		    ToDoItem item = new ToDoItem();

		    item.SetText(mTextNewToDo.GetText().ToString());
		    item.SetComplete(false);
		
		    // Insert the new item
		    mToDoTable.Insert(item, new MyInsertTableOperationCallback(this));

		    mTextNewToDo.SetText("");
	    }

	    /**
	     * Refresh the list with the items in the Mobile Service Table
	     */
	    private void RefreshItemsFromTable() {

		    // Get the items that weren't marked as completed and add them in the
		    // adapter9
#warning include line below:
            //mToDoTable.Where().Field("complete").Eq(MobileServiceQueryOperations.Val(false)).Execute(new MyTableQueryCallback(this));
	    }

	    /**
	     * Creates a dialog and shows it
	     * 
	     * @param exception
	     *            The exception to show in the dialog
	     * @param title
	     *            The dialog title
	     */
	    private void CreateAndShowDialog(System.Exception exception, string title) {
		    CreateAndShowDialog(exception.ToString(), title);
	    }

	    /**
	     * Creates a dialog and shows it
	     * 
	     * @param message
	     *            The dialog message
	     * @param title
	     *            The dialog title
	     */
	    private void CreateAndShowDialog(string message, string title) {
		    AlertDialog.Builder builder = new AlertDialog.Builder(this);

		    builder.SetMessage(message);
		    builder.SetTitle(title);
		    builder.Create().Show();
	    }
    }
}
