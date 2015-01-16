using Android.App;
using Android.Os;
using Android.View;

namespace SimpleFragment
{
    public class LeftFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(R.Layouts.LeftFragment, container, false);
        }
    }
}
