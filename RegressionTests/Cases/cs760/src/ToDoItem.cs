using Com.Google.Gson.Annotations;

namespace com.example.stamware
{
	public class SerializedNameAttribute: System.Attribute
	{
	    public SerializedNameAttribute(string name)
	    {
	        Value = name;
	    }

        public string Value { get; private set; }
       
	}

	/**
	 * Represents an item in a ToDo list
	 */
	public class ToDoItem {
	
		/**
		 * Item text
		 */
		[SerializedName("text")]
		private string mText;
	
		/**
		 * Item Id
		 */
		[SerializedName("id")]
		private int mId;
	
		/**
		 * Indicates if the item is completed
		 */
		[SerializedName("complete")]
		private bool mComplete;
	
		/**
		 * ToDoItem constructor
		 */
		public ToDoItem() {
	
		}
	
	
		public override string ToString() {
			return GetText();
		}
	
		/**
		 * Initializes a new ToDoItem
		 * 
		 * @param text
		 *            The item text
		 * @param id
		 *            The item id
		 */
		public ToDoItem(string text, int id) {
			this.Text = (text);
			this.SetId(id);
		}
	
		/**
		 * Returns the item text
		 */
		public string GetText() {
			return mText;
		}
	
		/**
		 * Sets the item text
		 * 
		 * @param text
		 *            text to set
		 */
		public void SetText(string text) {
			mText = text;
		}
	
		/**
		 * Returns the item id
		 */
		public int GetId() {
			return mId;
		}
	
		/**
		 * Sets the item id
		 * 
		 * @param id
		 *            id to set
		 */
		public  void SetId(int id) {
			mId = id;
		}
	
		/**
		 * Indicates if the item is marked as completed
		 */
		public bool IsComplete() {
			return mComplete;
		}
	
		/**
		 * Marks the item as completed or incompleted
		 */
		public void SetComplete(bool complete) {
			mComplete = complete;
		}

		public override bool Equals(object o) {
			return o is ToDoItem && ((ToDoItem) o).mId == mId;
		}
	}
}