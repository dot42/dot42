using System.Windows;
using System.Windows.Controls;

namespace Dot42.Ide.Controls
{
    public static class Extensions
    {
        public static TContainer GetContainerFromIndex<TContainer>(this ItemsControl itemsControl, int index)
            where TContainer : DependencyObject
        {
            return (TContainer)itemsControl.ItemContainerGenerator.ContainerFromIndex(index);
        }

        public static bool IsEditing(this DataGrid dataGrid)
        {
            return dataGrid.GetEditingRow() != null;
        }

        public static DataGridRow GetEditingRow(this DataGrid dataGrid)
        {
            var sIndex = dataGrid.SelectedIndex;
            if (sIndex >= 0)
            {
                var selected = dataGrid.GetContainerFromIndex<DataGridRow>(sIndex);
                if (selected.IsEditing) return selected;
            }

            for (int i = 0; i < dataGrid.Items.Count; i++)
            {
                if (i == sIndex) continue;
                var item = dataGrid.GetContainerFromIndex<DataGridRow>(i);
                if ((item != null) && item.IsEditing) return item;
            }

            return null;
        }
    }
}
