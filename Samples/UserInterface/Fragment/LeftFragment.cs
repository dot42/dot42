using Android.App;using Android.OS;using Android.Views;

namespace SimpleFragment
{
    public class LeftFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(R.Layout.LeftFragment, container, false);
        }
    }
}
