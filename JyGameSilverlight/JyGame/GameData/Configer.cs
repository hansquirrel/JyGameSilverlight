using System;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace JyGame.GameData
{
    public class Configer
    {
        public static Configer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Configer();

                    //load from config.xml
                    XElement rootNode = XElement.Load("/JyGame;component/config.xml");
                    _instance.Debug = bool.Parse(rootNode.Attribute("debug").Value);
                    string scriptModeStr = rootNode.Attribute("script").Value;
                    if (scriptModeStr == "xap")
                    {
                        _instance.ScriptMode = ScriptModeType.XAP;
                    }
                    else if(scriptModeStr == "dll")
                    {
                        _instance.ScriptMode = ScriptModeType.DLL;
                    }else if(scriptModeStr == "file")
                    {
                        _instance.ScriptMode = ScriptModeType.FILE;
                    }

                    if (rootNode.Attribute("scriptroot") != null)
                        _instance.ScriptRootMenu = rootNode.Attribute("scriptroot").Value;

                    if (rootNode.Attribute("resourceroot") != null)
                        _instance.ResourceRootMenu = rootNode.Attribute("resourceroot").Value;

                    //load from isolatedstorage
                    if (IsolatedStorageSettings.ApplicationSettings.Contains("AnimationSpeed"))
                        _instance.AnimationSpeed = (SkillAnimationSpeed)(int)(IsolatedStorageSettings.ApplicationSettings["AnimationSpeed"]);
                    if (IsolatedStorageSettings.ApplicationSettings.Contains("HighQualityAudio"))
                        _instance.HighQualityAudio = (bool)IsolatedStorageSettings.ApplicationSettings["HighQualityAudio"];
                    if (IsolatedStorageSettings.ApplicationSettings.Contains("AutoSave"))
                        _instance.AutoSave = (bool)IsolatedStorageSettings.ApplicationSettings["AutoSave"];
                    if (IsolatedStorageSettings.ApplicationSettings.Contains("AutoBattle"))
                        _instance.AutoBattle = (bool)IsolatedStorageSettings.ApplicationSettings["AutoBattle"];
                    if (IsolatedStorageSettings.ApplicationSettings.Contains("Music"))
                        _instance.Music = (bool)IsolatedStorageSettings.ApplicationSettings["Music"];
                    if (IsolatedStorageSettings.ApplicationSettings.Contains("Audio"))
                        _instance.Audio = (bool)IsolatedStorageSettings.ApplicationSettings["Audio"];
                    if (IsolatedStorageSettings.ApplicationSettings.Contains("Danmu"))
                        _instance.Danmu = (bool)IsolatedStorageSettings.ApplicationSettings["Danmu"];
                    if (IsolatedStorageSettings.ApplicationSettings.Contains("JiqiAnimation"))
                        _instance.JiqiAnimation = (bool)IsolatedStorageSettings.ApplicationSettings["JiqiAnimation"];
                }
                return _instance;
            }
        }

        static private Configer _instance = null;

        public bool Debug = false;
        public ScriptModeType ScriptMode = ScriptModeType.DLL;
        public string ScriptRootMenu = "";
        public string ResourceRootMenu = "";
        public void Init() { }
        public SkillAnimationSpeed AnimationSpeed
        {
            get { return _animationSpeed; }
            set
            {
                _animationSpeed = value;
                IsolatedStorageSettings.ApplicationSettings["AnimationSpeed"] = (int)(value);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }
        private SkillAnimationSpeed _animationSpeed = SkillAnimationSpeed.NORMAL;
        public int SkillAnimtionSwitchTime
        {
            get
            {
                switch(AnimationSpeed)
                {
                    case SkillAnimationSpeed.FAST:
                        return 30;
                    case SkillAnimationSpeed.NORMAL:
                        return 100;
                    case SkillAnimationSpeed.SLOW:
                        return 150;
                    default:
                        return 100;
                }
            }
        }

        public bool HighQualityAudio
        {
            get { return _highQualityAudio; }
            set
            {
                _highQualityAudio = value;
                IsolatedStorageSettings.ApplicationSettings["HighQualityAudio"] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }
        private bool _highQualityAudio = false;

        public bool AutoSave
        {
            get { return _autoSave; }
            set
            {
                _autoSave = value;
                IsolatedStorageSettings.ApplicationSettings["AutoSave"] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }
        private bool _autoSave = true;

        public bool AutoBattle
        {
            get { return _autoBattle; }
            set
            {
                _autoBattle = value;
                IsolatedStorageSettings.ApplicationSettings["AutoBattle"] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }
        private bool _autoBattle = false;

        public bool Music
        {
            get { return _music; }
            set
            {
                AudioManager.MuteMusic(value);
                IsolatedStorageSettings.ApplicationSettings["Music"] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
                _music = value;
            }
        }
        private bool _music = true;

        public bool Audio
        {
            get { return _audio; }
            set
            {
                AudioManager.MuteAudio(value);
                IsolatedStorageSettings.ApplicationSettings["Audio"] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
                _audio = value;
            }
        }
        private bool _audio = true;

        public bool Danmu
        {
            get { return _danmu; }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["Danmu"] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
                _danmu = value;
                try
                {
                    if (_danmu)
                    {
                        
                    }
                    else
                    {
                        RuntimeData.Instance.gameEngine.uihost.DanmuCanvas.Visibility = Visibility.Collapsed;
                    }
                }catch
                {
                }
            }
        }
        private bool _danmu = false;
        
        public bool JiqiAnimation
        {
            get { return _jiqiAnimation; }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["JiqiAnimation"] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
                _jiqiAnimation = value;
            }
        }

        private bool _jiqiAnimation = true;
    }

    public enum ScriptModeType
    {
        DLL, //从DLL中加载
        XAP, //从XAP包中加载
        FILE //从本地路径文件加载
    }

    public enum SkillAnimationSpeed
    {
        FAST = 0,
        NORMAL,
        SLOW
    }
}
