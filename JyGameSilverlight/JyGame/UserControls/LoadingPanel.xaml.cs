using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using JyGame.GameData;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Net;
using System.Windows.Browser;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;

namespace JyGame
{
	public partial class LoadingPanel : UserControl
	{
		public LoadingPanel()
		{
            
			// 为初始化变量所必需
			InitializeComponent();
            cachedKey = new Dictionary<string, bool>();
		}


        private Dictionary<string, bool> cachedKey
        {
            set
            {
                lock (this)
                {
                    _cachedKey = value;
                }
            }
            get
            {
                lock (this)
                {
                    return _cachedKey;
                }
            }
        }
        private Dictionary<string, bool> _cachedKey;
        

        private CommonSettings.VoidCallBack Callback = null;
        private List<string> tobedownload = null;
        private int totalDownloadedNumber = 0;
        private string baseUrl = "";
        //public void Show(List<ImageSource> images, CommonSettings.VoidCallBack callback)
        public void Show(List<string> imagesUrls, CommonSettings.VoidCallBack callback)
        {
            downloadedImage = 0;
            progressBar.Value = 0;
            progressText.Text = "0%";

            if (checkTimer == null)
            {
                this.InitCheckTimer();
            }

            if (baseUrl == "")
            {
                baseUrl = GetURL();
            }
            
            Callback = callback;
            if (imagesUrls == null || imagesUrls.Count == 0)
            {
                Finish();
            }
            else
            {
                //先去掉已经缓存的图片
                List<string> tobeRemovedImages = new List<string>();
                foreach (var url in imagesUrls)
                {
                    if (cachedKey.ContainsKey(url))
                    {
                        tobeRemovedImages.Add(url);
                    }
                }
                foreach (var img in tobeRemovedImages)
                {
                    imagesUrls.Remove(img);
                }
                if (imagesUrls.Count == 0)
                {
                    //this.Dispatcher.BeginInvoke(() => { Finish(); });
                    Finish();
                    return;
                }
                suggestTip.Show();
                tobedownload = imagesUrls;
                totalDownloadedNumber = tobedownload.Count;
                this.Visibility = Visibility.Visible;
                StartDownload();
            }
        }

        private void Finish()
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
            Callback();
        }

        private void StartDownload()
        {
            
            if (tobedownload.Count == 0)
            {
                this.Dispatcher.BeginInvoke(() => { Finish(); });
                return;
            }

            checkTimer.Start();
            foreach (var img in tobedownload)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(DownloadImage), img);
            }
        }

        private void DownloadImage(object img)
        {
            string url = img as string;
            //ImageSource image = img as ImageSource;
            //string url = Tools.GetImageUrl(image as BitmapImage);
            try
            {
                if (!cachedKey.ContainsKey(url))
                {
                    if (Configer.Instance.ResourceRootMenu == "") //从互联网载入
                    {
                        WebClient webClient = new WebClient();
                        webClient.OpenReadCompleted += (s, e) =>
                        {
                            if (e.Error == null && e.Result != null)
                            {
                                try
                                {
                                    this.Dispatcher.BeginInvoke(() =>
                                    {
                                        BitmapImage bitMapImage = new BitmapImage();
                                        bitMapImage.SetSource(e.Result);
                                        Tools.PutImageCache(url, bitMapImage);
                                    });
                                }
                                catch (Exception ex)
                                {
                                    //this.Dispatcher.BeginInvoke(() =>
                                    //{
                                    //    MessageBox.Show(string.Format("{0}", url));
                                    //    MessageBox.Show(ex.StackTrace);
                                    //}); 
                                }
                            }
                            ReportImageOk(null);
                        };
                        Uri imgUri = new Uri(baseUrl + url, UriKind.RelativeOrAbsolute);
                        webClient.OpenReadAsync(imgUri);
                    }
                    else //从本地载入
                    {
                        this.Dispatcher.BeginInvoke(() =>
                            {
                                try
                                {
                                    using (StreamReader sr = new StreamReader(baseUrl + url))
                                    {
                                        BitmapImage bitMapImage = new BitmapImage();
                                        bitMapImage.SetSource(sr.BaseStream);
                                        Tools.PutImageCache(url, bitMapImage);
                                    }
                                }catch(Exception ee)
                                {
                                    //MessageBox.Show(ee.ToString());
                                }
                                ReportImageOk(null);
                            });
                    }
                    cachedKey[url] = true;
                }
                else
                {
                    ReportImageOk(null);
                }
            }
            catch (Exception e)
            {
                ReportImageOk(null);
            }
        }

        private int downloadedImage
        {
            get
            {
                int tmp = -1;
                lock (this)
                {
                    tmp = _downloadedImage;
                }
                return tmp;
            }
            set
            {
                _downloadedImage = value;
            }
        }
        private int _downloadedImage = 0;
        private void ReportImageOk(ImageSource img)
        {
            downloadedImage++;
        }

        private void InitCheckTimer()
        {
            checkTimer = new DispatcherTimer();
            checkTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            checkTimer.Tick += checkTimer_Tick;
        }

        private int checkTimeNoIncreaseCount = 0;
        private int lastCount = -1;
        private void checkTimer_Tick(object sender, EventArgs e)
        {
            if (downloadedImage >= totalDownloadedNumber - 1)
            {
                checkTimer.Stop();
                Finish();
                //this.Dispatcher.BeginInvoke(() => { Finish(); });
            }
            int newCount = totalDownloadedNumber;
            if (newCount == lastCount)
                checkTimeNoIncreaseCount++;
            else
                checkTimeNoIncreaseCount = 0;
            double rate = (double)downloadedImage / (double)totalDownloadedNumber;
            if (checkTimeNoIncreaseCount >= 20 && rate > 0.85)
            {
                checkTimer.Stop();
                Finish();
                //this.Dispatcher.BeginInvoke(() => { Finish(); });
            }

            progressBar.Value = ((double)downloadedImage / (double)totalDownloadedNumber) * 100;
            progressText.Text = ((int)progressBar.Value).ToString() + "%";
            lastCount = totalDownloadedNumber;
        }
        private DispatcherTimer checkTimer;
        
        public static string GetURL()
        {            
            if(Application.Current.IsRunningOutOfBrowser)
            {
                if (Configer.Instance.ResourceRootMenu == "")
                    return "./";
                else
                    return Configer.Instance.ResourceRootMenu;
            }
            ScriptObject location = (HtmlPage.Window.GetProperty("location") as ScriptObject);
            object r = location.GetProperty("href");
            string URL = r.ToString().Substring(0, r.ToString().LastIndexOf('/')); //截取到当前SILVERLIGHT程序存放网络URL的前缀
            return URL;
        }
	}
}