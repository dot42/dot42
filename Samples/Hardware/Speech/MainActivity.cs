using System;
using Android.App;
using Android.Content;
using Android.Os;
using Android.Widget;
using Dot42.Manifest;
using Android.Speech;
using Java.Util;
using Android.Speech.Tts;

[assembly: Application("dot42 Echo")]

namespace Echo
{
    [Activity(Icon = "Icon")]
    public class MainActivity : Activity, TextToSpeech.IOnInitListener
    {
        private const int RECOGNIZER_RESULT = 1234;
        private const int SPEECH_RESULT = 4321;

        private TextToSpeech textToSpeech;
        private string recognizedText;

        /// <summary>
        /// Initialize the activity
        /// </summary>
        protected override void OnCreate(Bundle savedInstance)
        {
            base.OnCreate(savedInstance);
            SetContentView(R.Layouts.MainLayout);

            var startSpeech = FindViewById<Button>(R.Ids.getSpeechButton);
            startSpeech.Click += OnClick;
        }

        /// <summary>
        /// Start text recognition.
        /// </summary>
        public void OnClick(object sender, EventArgs a)
        {
            var intent = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
            intent.PutExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL, RecognizerIntent.LANGUAGE_MODEL_FREE_FORM);
            intent.PutExtra(RecognizerIntent.EXTRA_PROMPT, "Speech to text");
            StartActivityForResult(intent, RECOGNIZER_RESULT);
        }

        /// <summary>
        /// Process results from started activities.
        /// </summary>
        protected override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (requestCode == RECOGNIZER_RESULT && resultCode == RESULT_OK)
            {
                var matches = data.GetStringArrayListExtra(RecognizerIntent.EXTRA_RESULTS);

                var speechText = FindViewById<TextView>(R.Ids.speechText);
                recognizedText = matches.Get(0);
                speechText.SetText(recognizedText);

                var checkIntent = new Intent();
                checkIntent.SetAction(TextToSpeech.Engine.ACTION_CHECK_TTS_DATA);
                StartActivityForResult(checkIntent, SPEECH_RESULT);
            }

            if (requestCode == SPEECH_RESULT)
            {
                if (resultCode == TextToSpeech.Engine.CHECK_VOICE_DATA_PASS)
                {
                    textToSpeech = new TextToSpeech(this, this);
                    textToSpeech.SetLanguage(Locale.US);
                }
                else
                {
                    // TTS data not yet loaded, try to install it
                    var ttsLoadIntent = new Intent();
                    ttsLoadIntent.SetAction(TextToSpeech.Engine.ACTION_INSTALL_TTS_DATA);
                    StartActivity(ttsLoadIntent);
                }
            }
            base.OnActivityResult(requestCode, resultCode, data);
        }

        /// <summary>
        /// Text to speech initialization callback.
        /// </summary>
        public void OnInit(int status)
        {
            if (status == TextToSpeech.SUCCESS)
            {
                textToSpeech.Speak(recognizedText, TextToSpeech.QUEUE_FLUSH, null);
            }
            else if (status == TextToSpeech.ERROR)
            {
                textToSpeech.Shutdown();
            }
        }
    }
}
