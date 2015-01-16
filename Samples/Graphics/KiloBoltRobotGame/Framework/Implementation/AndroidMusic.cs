using System;
using System.IO;
using Android.Content.Res;
using Android.Media;
using Java.Lang;
using Exception = Java.Lang.Exception;

namespace KiloBoltRobotGame.Framework
{
    public class AndroidMusic : IMusic, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnSeekCompleteListener,
        MediaPlayer.IOnPreparedListener, MediaPlayer.IOnVideoSizeChangedListener
    {
        private MediaPlayer mediaPlayer;
        private bool isPrepared = false;

        public AndroidMusic(AssetFileDescriptor assetDescriptor)
        {
            mediaPlayer = new MediaPlayer();
            try
            {
                mediaPlayer.SetDataSource(assetDescriptor.GetFileDescriptor(),
                                          assetDescriptor.GetStartOffset(),
                                          assetDescriptor.GetLength());
                mediaPlayer.Prepare();
                isPrepared = true;
                mediaPlayer.SetOnCompletionListener(this);
                mediaPlayer.SetOnSeekCompleteListener(this);
                mediaPlayer.SetOnPreparedListener(this);
                mediaPlayer.SetOnVideoSizeChangedListener(this);

            }
            catch (Exception e)
            {
                throw new SystemException("Couldn't load music");
            }
        }

        public void dispose()
        {

            if (this.mediaPlayer.IsPlaying())
            {
                this.mediaPlayer.Stop();
            }
            this.mediaPlayer.Release();
        }

        public bool isLooping()
        {
            return mediaPlayer.IsLooping();
        }

        public bool isPlaying()
        {
            return this.mediaPlayer.IsPlaying();
        }

        public bool isStopped()
        {
            return !isPrepared;
        }

        public void pause()
        {
            if (this.mediaPlayer.IsPlaying())
                mediaPlayer.Pause();
        }


        public void play()
        {
            if (this.mediaPlayer.IsPlaying())
                return;

            try
            {
                lock (this)
                {
                    if (!isPrepared)
                        mediaPlayer.Prepare();
                    mediaPlayer.Start();
                }
            }
            catch (IllegalStateException e)
            {
                e.PrintStackTrace();
            }
            catch (IOException e)
            {
                e.PrintStackTrace();
            }
        }

        public void setLooping(bool isLooping)
        {
            mediaPlayer.SetLooping(isLooping);
        }

        public void setVolume(float volume)
        {
            mediaPlayer.SetVolume(volume, volume);
        }

        public void stop()
        {
            if (this.mediaPlayer.IsPlaying() == true)
            {
                this.mediaPlayer.Stop();

                lock (this)
                {
                    isPrepared = false;
                }
            }
        }

        public void OnCompletion(MediaPlayer player)
        {
            lock (this)
            {
                isPrepared = false;
            }
        }

        public void seekBegin()
        {
            mediaPlayer.SeekTo(0);

        }


        public void OnPrepared(MediaPlayer player)
        {
            // TODO Auto-generated method stub
            lock (this)
            {
                isPrepared = true;
            }

        }

        public void OnSeekComplete(MediaPlayer player)
        {
            // TODO Auto-generated method stub

        }

        public void OnVideoSizeChanged(MediaPlayer player, int width, int height)
        {
            // TODO Auto-generated method stub

        }
    }
}