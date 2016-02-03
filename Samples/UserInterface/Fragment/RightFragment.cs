using Android.App;using Android.OS;using Android.Views;

namespace SimpleFragment
{
    public class RightFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(R.Layout.RightFragment, container, false);
        }
    }
}
