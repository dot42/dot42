using System;
using Android.App;
using Android.Appwidget;using Android.OS;
using Android.Widget;
using Dot42;
using Dot42.Manifest;

namespace Dot42Application1
{
    [AppWidgetProvider(Label = "Hello World Widget", MinWidth = "60dp", MinHeight = "60dp", 
		PreviewImage = "AppWidget", InitialLayout = "AppWidget1_Layout")]
    public class AppWidget1 : AppWidgetProvider
    {
    }
}
