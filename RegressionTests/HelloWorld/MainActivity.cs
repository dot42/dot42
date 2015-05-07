using System;
using Android.App;using Android.OS;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

[assembly: Application("HelloWorldTestApp")]

namespace Dot42Application1
{
    [Activity(Label ="Hello world MainActivity")]
    public class MainActivity : Activity
    {
        private Button button;
        private EditText editor;

        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            /*button = new Button(this);
            button.Text = "Click me";
            button.Click += OnButtonClick;
            SetContentView(button);*/
            SetContentView(R.Layout.MainActivityLayout);
            editor = (EditText) FindViewById(R.Id.editor);
            button = (Button) FindViewById(R.Id.button);
            button.Click += OnButtonClick;
            button.Text = this.GetString(R.String.testString);
        }

        public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
        {
            MenuInflater.Inflate(R.Menu.Menu1, menu);
            return true;
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            for (var i = 0; i < 10; i++)
            {
                button.Text = "Text = " + editor.Text;
            }
        }
    }
}
