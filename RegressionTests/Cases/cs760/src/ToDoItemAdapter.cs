using Android.App;
using Android.Content;
using Android.View;
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
				LayoutInflater inflater = ((Activity) mContext).GetLayoutInflater();
				row = inflater.Inflate(mLayoutResourceId, parent, false);
			}
	
			row.SetTag(currentItem);
			CheckBox checkBox = (CheckBox) row.FindViewById(R.Ids.checkToDoItem);
			checkBox.SetText(currentItem.GetText());
			checkBox.SetChecked(false);
			checkBox.SetEnabled(true);

		    checkBox.Click += (sender, args) =>
		        {
		            if (checkBox.IsChecked())
		            {
		                checkBox.SetEnabled(false);
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