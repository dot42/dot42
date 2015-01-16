
using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace Dot42.SharpDevelop.Pads
{
	/// <summary>
	/// Description of ListViewExtensions.
	/// </summary>
	internal static class ListViewExtensions
	{
		public static void ClearColumns(this ListView view)
		{
			if (view == null)
				throw new ArgumentNullException("view");
			if (view.View is GridView)
				((GridView)view.View).Columns.Clear();
		}
		
		public static void AddColumn(this ListView view, string header, Binding binding, double width)
		{
			if (view == null)
				throw new ArgumentNullException("view");
			if (view.View is GridView) {
				GridViewColumn column = new GridViewColumn {
					Width = width,
					DisplayMemberBinding = binding,
					Header = header };
				((GridView)view.View).Columns.Add(column);
			}
		}
	}
}
