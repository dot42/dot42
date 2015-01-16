using System;
using Android.App;
using Android.Graphics;
using Android.Os;
using Android.Widget;
using Dot42.Manifest;

[assembly: Application("dot42 FontDemo")]

namespace FontDemo
{
    /// <summary>
    /// Shows the use of assets to load a different font.
    /// </summary>
    [Activity(Icon = "icon", Label = "dot42 Font demo")]
    public class FontDemo : Activity
    {
        private const int DLG_FONTFAILED = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(R.Layouts.MainLayout);

            var v = FindViewById<TextView>(R.Ids.fontView); 
            Typeface t = null;
            try
            {
                // Embedded resources are accesssible as asset.
                t = Typeface.CreateFromAsset(GetAssets(), "FontDemo.fonts.fontdemo.TTF");
            }
            catch (Exception e)
            {
                ShowDialog(DLG_FONTFAILED);
            }
            v.SetTypeface(t, Typeface.BOLD_ITALIC); 
            v.SetTextSize(200.0f);
        }

        protected override Dialog OnCreateDialog(int id, Bundle args)
        {
            switch (id)
            {
                case DLG_FONTFAILED:
                    {
                        var builder = new AlertDialog.Builder(this);

                        builder.SetCancelable(false);
                        builder.SetTitle(R.Strings.FontFailTitle);
                        builder.SetMessage(R.Strings.FontFailText);
                        builder.SetNeutralButton("OK", (s, x) => { throw new Exception("Failed to load fonts"); });                                               

                        return builder.Create();
                    }
                default:
                    return base.OnCreateDialog(id, args);
            }
        }
    }
}
