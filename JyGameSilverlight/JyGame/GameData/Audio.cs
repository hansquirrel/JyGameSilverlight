using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace JyGame.GameData
{
    public class AudioTag
    {
        public CommonSettings.VoidCallBack callback = null;
        public int Index = -1;
    }

    public class AudioManager
    {
        static private MediaElement GameMusic;

        private const int EFFECT_POOL_SIZE = 10; //音效池大小
        static private MediaElement[] EffectPool = new MediaElement[EFFECT_POOL_SIZE];
        static private bool[] EffectChannelBusy = new bool[EFFECT_POOL_SIZE];
        static private int CurrentChannel = 0;

        static private string currentPath = "";

        static public void Init()
        {
            Canvas container = App.RootCanvas;
            GameMusic = new MediaElement()
            {
                IsHitTestVisible = false,
                Visibility = System.Windows.Visibility.Collapsed,
                Volume = 1,
                IsMuted = !Configer.Instance.Music
            };
            
            GameMusic.MediaEnded += (s, e) =>
            {
                GameMusic.Stop();
                GameMusic.Play();
            };
            container.Children.Add(GameMusic);

            for (int i = 0; i < EFFECT_POOL_SIZE; ++i)
            {
                MediaElement effectElement = new MediaElement()
                {
                    IsHitTestVisible = false,
                    Visibility = System.Windows.Visibility.Collapsed,
                    Volume = 1,
                    IsMuted = !Configer.Instance.Audio
                };
                EffectPool[i] = effectElement;
                EffectChannelBusy[i] = false;
                effectElement.Tag = new AudioTag() { Index = i };

                effectElement.MediaEnded += (s, e) =>
                {
                    AudioTag tag = effectElement.Tag as AudioTag;
                    if (tag.callback != null)
                        tag.callback();
                    tag.callback = null;
                    EffectChannelBusy[tag.Index] = false;
                };

                effectElement.MediaFailed += (s, e) =>
                {
                    AudioTag tag = effectElement.Tag as AudioTag;
                    EffectChannelBusy[tag.Index] = false;
                };


                container.Children.Add(effectElement);
            }
        }

        static public void PlayMusic(string path)
        {
            if (path == null || path=="")
            {
                //GameMusic.Stop();
                //currentPath = path;
                return;
            }
            if (Configer.Instance.ResourceRootMenu != "")
                path = Configer.Instance.ResourceRootMenu + path;

            if(Configer.Instance.HighQualityAudio)
            {
                path = path.Replace("audios", "audios_source");
            }
            else
            {
                path = path.Replace("audios_source", "audios");
            }
            path = path.Replace("\\", "/");
            if (path != currentPath)
            {
                GameMusic.Stop();
                GameMusic.Source = new Uri(path, UriKind.Relative);
                
                GameMusic.Play();
                currentPath = path;
            }
        }
        static public void Replay()
        {
            var t = GameMusic.Position;
            PlayMusic(currentPath);
            GameMusic.Position = t;
        }

        static public void PlayEffect(string path, CommonSettings.VoidCallBack callback = null)
        {
            if (Configer.Instance.HighQualityAudio)
            {
                path = path.Replace("audios", "audios_source");
            }
            else
            {
                path = path.Replace("audios_source", "audios");
            }
            if (Configer.Instance.ResourceRootMenu != "")
                path = Configer.Instance.ResourceRootMenu + path;
            path = path.Replace("\\", "/");
            int channel = CurrentChannel;
            EffectPool[channel].Source = new Uri(path, UriKind.Relative);
            AudioTag tag = EffectPool[channel].Tag as AudioTag;
            tag.callback = callback;
            EffectPool[channel].Play();
            CurrentChannel++;
            if (CurrentChannel >= EFFECT_POOL_SIZE)
                CurrentChannel = 0;
        }

        static public void PlayRandomEffect(string[] paths, CommonSettings.VoidCallBack callback = null)
        {
            PlayEffect(ResourceManager.Get(paths[Tools.GetRandomInt(0, paths.Length) % paths.Length]), callback);
        }

        static public void MuteMusic(bool isMute)
        {
            if(GameMusic !=null)
                GameMusic.IsMuted = !isMute;
        }

        static public void MuteAudio(bool isMute)
        {
            foreach (var m in EffectPool)
            {
                if(m!=null)
                    m.IsMuted = !isMute;
            }
        }

        static public bool IsMusicMuted { get { return GameMusic.IsMuted; } }
        static public bool IsAudioMuted { get { return EffectPool[0].IsMuted; } }
     }
}
