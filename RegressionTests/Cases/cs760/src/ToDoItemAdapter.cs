using Android.App;
using Android.Content;using Android.Views;
using Android.Widget;

using Test1; 

namespace com.example.stamware
	{
	/**
	 * Adapter to bind a ToDoItem List to a view
	 */
	public class ToDoItemAdapter : ArrayAdapter<ToDoItem> {
	
		/**
		 * Adapter context
		 */
		Context mContext;
	
		/**
		 * Adapter View layout
		 */
		int mLayoutResourceId;
	
		public ToDoItemAdapter(Context context, int layoutResourceId) 
			:base(context, layoutResourceId)
		{
			mContext = context;
			mLayoutResourceId = layoutResourceId;
		}
	
		/**
		 * Returns the view for a specific item on the list
		 */
		public override View GetView(int position, View convertView, ViewGroup parent) {
			View row = convertView;
	
			ToDoItem currentItem = GetItem(position);
	
			if (row == null) {
				LayoutInflater inflater = ((Activity) mContext).LayoutInflater;
				row = inflater.Inflate(mLayoutResourceId, parent, false);
			}
	
			row.Tag = currentItem;
			CheckBox checkBox = (CheckBox) row.FindViewById(R.Id.checkToDoItem);
			checkBox.Text = currentItem.GetText();
			checkBox.IsChecked = (false);
			checkBox.IsEnabled = (true);

		    checkBox.Click += (sender, args) =>
		        {
		            if (checkBox.IsChecked)
		            {
		                checkBox.IsEnabled = false;
		                if (mContext is ToDoActivity)
		                {
		                    ToDoActivity activity = (ToDoActivity) mContext;
		                    activity.CheckItem(currentItem);
		                }
		            }
		        };

	
			return row;
		}
	
	}
}