using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Appwidget;
using Dot42.Manifest;

namespace AppWidget1
{
    [AppWidgetProvider(MinWidth = "150dp", MinHeight = "100dp", InitialLayout = "WidgetLayout")]
    public class WidgetProvider : AppWidgetProvider
    {
    }
}
