using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Dot42.Ide.Extenders
{
    public abstract class StringListUITypeEditor : UITypeEditor
    {
        private readonly string[] list;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected StringListUITypeEditor(string[] list)
        {
            this.list = list;
        }

        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService editorService = null;
            if (provider != null)
            {
                editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            }

            if (editorService != null)
            {
                var control = new ListBox();
                control.Items.Add(ResourceExtenderBase.AnyText);
                control.Items.AddRange(list);
                control.Height = 200;
                control.BorderStyle = BorderStyle.FixedSingle;                
                if (string.IsNullOrEmpty(value as string))
                {
                    control.SelectedIndex = 0;
                }
                else
                {
                    control.SelectedItem = value;
                }
                control.PreviewKeyDown += (s, x) => { if (x.KeyCode == Keys.Enter) editorService.CloseDropDown(); };
                control.Leave += (s, x) => editorService.CloseDropDown();
                control.Click += (s, x) => editorService.CloseDropDown();
                
                editorService.DropDownControl(control);

                if (control.SelectedIndex == 0)
                    return null;
                return control.SelectedItem;
            }
            return base.EditValue(context, provider, value);
        }
    }
}
