using System.Windows.Forms;

namespace Dot42.ApkSpy
{
    public partial class SearchDialog : Form
    {
        public SearchDialog()
        {
            InitializeComponent();
        }

        public string SearchText 
        { 
            get { return tbSearchText.Text; }
            set
            {
                tbSearchText.Text = value ?? ""; 
                tbSearchText.SelectAll();
            }
        }
    }
}
