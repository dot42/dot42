
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.TreeView;

namespace Dot42.SharpDevelop.Pads.Controls
{
	public enum WatchListType
	{
		LocalVar,
		Watch
	}
	
	/// <summary>
	/// Interaction logic for WatchList.xaml
	/// </summary>
	public partial class WatchList : UserControl
	{
		public WatchList(WatchListType type)
		{
			InitializeComponent();
			WatchType = type;
			/*if (type == WatchListType.Watch)
				myList.Root = new WatchRootNode();
			else*/
				myList.Root = new SharpTreeNode();
		}
		
		public WatchListType WatchType { get; private set; }
		
		public SharpTreeNodeCollection WatchItems {
			get { return myList.Root.Children; }
		}
		
		public DebugProperty SelectedNode {
			get { return myList.SelectedItem as DebugProperty; }
		}
		
		void OnValueTextBoxKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter && e.Key != Key.Escape) {
				e.Handled = true;
				return;
			}
			
			/*if (e.Key == Key.Enter) {
				if(SelectedNode.Node is ExpressionNode) {
					var node = (ExpressionNode)SelectedNode.Node;
					node.SetText(((TextBox)sender).Text);
				}
			}
			if (e.Key == Key.Enter || e.Key == Key.Escape) {
				myList.UnselectAll();
				if (LocalVarPad.Instance != null)
					LocalVarPad.Instance.InvalidatePad();
				if (WatchPad.Instance != null)
					WatchPad.Instance.InvalidatePad();
			}*/
		}
		
		void WatchListAutoCompleteCellCommandEntered(object sender, EventArgs e)
		{
			var selectedNode = SelectedNode;
			if (selectedNode == null) return;
			if (WatchType != WatchListType.Watch) return;
			
			/*var cell = ((WatchListAutoCompleteCell)sender);
			
			selectedNode.Node.Name = cell.CommandText;
			myList.UnselectAll();
			if (WatchType == WatchListType.Watch && WatchPad.Instance != null) {
				WatchPad.Instance.InvalidatePad();
			}
			selectedNode.IsEditing = false;*/
		}
		
		void MyListPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (SelectedNode == null) return;
			if (WatchType != WatchListType.Watch)
				return;
			SelectedNode.IsEditing = true;
		}
	}
}