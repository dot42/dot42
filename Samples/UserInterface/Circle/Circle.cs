using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.View;
using Dot42;

namespace Circle
{
    [CustomView]
    public class Circle : View
    {
        /// <summary>
        /// XML constructor
        /// </summary>
        public Circle(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            var a = context.Theme.ObtainStyledAttributes(attrs, R.Styleables.Circle.AllIds, 0, 0);
            try
            {
                Color = a.GetColor(R.Styleables.Circle.Color, Android.Graphics.Color.RED);
            }
            finally
            {
                a.Recycle();
            }
        }

        public int Color { get; set; }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            var paint = new Paint();
            paint.SetColor(Color);
            canvas.DrawCircle(Width / 2, Height / 2, Width / 2, paint);
        }
    }
}
