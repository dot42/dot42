using System;
using Android.App;
using Android.Content;
using Android.Os;
using Android.Widget;
using Dot42.Manifest;
using RekenMaar.Activities;
using Rekenspel.Games;
using Rekenspel.Interfaces;

[assembly: Application("RekenMaar", Theme = "android:style/Theme.Holo.Light", Icon = "Icon")]
[assembly: Package(VersionName = "1.0.0", VersionCode = 2)]

namespace RekenMaar
{
    [Activity]
    public class MainActivity : Activity, ActionBar.IOnNavigationListener
    {
        TextView tvQuestion;
        TextView tvAnswer;
        TextView tvResult;

        IGame game;
        string gameType = "Multiply";
        GameParameterFactory gameParameterFactory = new GameParameterFactory();


        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);

            var actionBar = GetActionBar();
            actionBar.SetNavigationMode(ActionBar.NAVIGATION_MODE_LIST);

            //actionBar.SetListNavigationCallbacks(ArrayAdapter<string>.CreateFromResource(this, R.Arrays.gameType_array, R.Layouts.TextView),
            //                                     this);

            //actionBar.SetListNavigationCallbacks(ArrayAdapter<string>.CreateFromResource(this, R.Arrays.gameType_array, Android.R.Layout.Simple_spinner_item),
            //                                     this);

            var adapter = ArrayAdapter<string>.CreateFromResource(this, R.Arrays.gameType_array, Android.R.Layout.Simple_spinner_item);
            adapter.SetDropDownViewResource(Android.R.Layout.Simple_spinner_dropdown_item);
            actionBar.SetListNavigationCallbacks(adapter, this);

            tvAnswer = (TextView)FindViewById(R.Ids.answer);
            tvQuestion = (TextView)FindViewById(R.Ids.question);
            tvResult = (TextView)FindViewById(R.Ids.result);

            var btn = (Button)FindViewById(R.Ids.cmd0);
            btn.Click += new EventHandler(btn_Click);
            btn = (Button)FindViewById(R.Ids.cmd1);
            btn.Click += new EventHandler(btn_Click);
            btn = (Button)FindViewById(R.Ids.cmd2);
            btn.Click += new EventHandler(btn_Click);
            btn = (Button)FindViewById(R.Ids.cmd3);
            btn.Click += new EventHandler(btn_Click);
            btn = (Button)FindViewById(R.Ids.cmd4);
            btn.Click += new EventHandler(btn_Click);
            btn = (Button)FindViewById(R.Ids.cmd5);
            btn.Click += new EventHandler(btn_Click);
            btn = (Button)FindViewById(R.Ids.cmd6);
            btn.Click += new EventHandler(btn_Click);
            btn = (Button)FindViewById(R.Ids.cmd7);
            btn.Click += new EventHandler(btn_Click);
            btn = (Button)FindViewById(R.Ids.cmd8);
            btn.Click += new EventHandler(btn_Click);
            btn = (Button)FindViewById(R.Ids.cmd9);
            btn.Click += new EventHandler(btn_Click);
            btn = (Button)FindViewById(R.Ids.Ok);
            btn.Click += new EventHandler(btnOk_Click);
            btn = (Button)FindViewById(R.Ids.Erase);
            btn.Click += new EventHandler(btnErase_Click);

            var gamePreferences = new Preferences();
            gamePreferences.RetrievePreferences(GetSharedPreferences("GamePreferences", 0));
            gameParameterFactory.Preferences = gamePreferences;

            game = GameFactory.GetGame(gameType, gameParameterFactory);
            tvQuestion.SetText(game.GetNextAssignment());
        }

        public override bool OnCreateOptionsMenu(Android.View.IMenu menu)
        {
            var inflater = GetMenuInflater();
            inflater.Inflate(R.Menus.Menu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(Android.View.IMenuItem item)
        {
            bool result = true;
            switch (item.GetItemId())
            {
                case R.Ids.menu_options:
                    var intent = new Intent(this, typeof(SettingsActivity));
                    StartActivityForResult(intent, 1);
                    break;
                default:
                    result = base.OnOptionsItemSelected(item);
                    break;
            }
            return result;
        }

        protected override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == RESULT_OK)
            {
                var gamePreferences = new Preferences();
                gamePreferences.RetrievePreferences(GetSharedPreferences("GamePreferences", MODE_MULTI_PROCESS));
                gameParameterFactory.Preferences = gamePreferences;

                game = GameFactory.GetGame(gameType, gameParameterFactory);
                tvQuestion.SetText(game.GetNextAssignment());
            }
        }

        void btn_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var value = btn.GetText().ToString();
            tvAnswer.SetText(tvAnswer.GetText().ToString() + value);
        }

        void btnOk_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var answer = tvAnswer.GetText().ToString();
            // Compare answer
            SetResult(game.CheckAnswer(answer));
            // Empty textView
            tvAnswer.SetText("");
            // Get next question
            tvQuestion.SetText(game.GetNextAssignment());

        }

        void btnErase_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            // Empty textView
            tvAnswer.SetText("");
        }

        void SetResult(bool result)
        {
            if (result)
            {
                tvResult.SetText(GetString(R.Strings.answer_correct));
                //tvResult.SetTextColor();
            }
            else
            {
                tvResult.SetText(GetString(R.Strings.answer_incorrect));
            }
        }

        public bool OnNavigationItemSelected(int itemPosition, long itemId)
        {
            switch (itemPosition)
            {
                case 0:
                    gameType = "Multiply";
                    break;
                case 1:
                    gameType = "Divide";
                    break;
                case 2:
                    gameType = "Add";
                    break;
                case 3:
                    gameType = "Subtract";
                    break;
                default:
                    break;
            }
            game = GameFactory.GetGame(gameType, gameParameterFactory);
            tvQuestion.SetText(game.GetNextAssignment());
            return true;
        }

    }
}
