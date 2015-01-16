using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Speech.Tts;
using Android.Content;
using Java.Util;
using Android.Util;

namespace SorterenMaar.Games
{
  public class Text2Speech : TextToSpeech.IOnInitListener
  {
    TextToSpeech tts = null;

    public Text2Speech(Context context)
    {
      tts = new TextToSpeech(context, this);
      //tts.SetSpeechRate
    }

    public void OnInit(int status)
    {
      if (status == TextToSpeech.SUCCESS)
      {

        int result = tts.SetLanguage(Locale.ENGLISH);

        if (   result == TextToSpeech.LANG_MISSING_DATA
            || result == TextToSpeech.LANG_NOT_SUPPORTED)
        {
          Log.E("TTS", "This Language is not supported");
        }
        //else
        //{
        //  btnSpeak.setEnabled(true);
        //  speakOut();
        //}

      }
      else
      {
        Log.E("TTS", "Initilization Failed!");
      }
    }

    public void SpeakOut(string text)
    {
      tts.Speak(text, TextToSpeech.QUEUE_FLUSH, null);
    }
  }
}
