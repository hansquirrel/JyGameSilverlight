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
using System.Windows.Media.Imaging;

using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Browser;
using System.IO;
using System.Threading;
using JyGame.GameData;
using System.Text;
using System.Security.Cryptography;

using System.Collections;
using System.Globalization;

namespace JyGame
{
    public class BitMapImageCache
    {
        public BitmapImage Image = null;
        public DateTime Time;

        public BitMapImageCache(BitmapImage img)
        {
            Image = img;
            Time = DateTime.Now;
        }
    }

    public class Tools
    {
        private static Dictionary<string, BitMapImageCache> imageCache = new Dictionary<string, BitMapImageCache>();

        private static Object imageCacheLocker = new object();
        private const int imageCacheTimeInMinutes = 3;
        public static BitmapSource GetImage(string path)
        {
            //lock (imageCacheLocker)
            {
                if (imageCache.ContainsKey(path))
                    return imageCache[path].Image;
                BitmapImage rst = new BitmapImage(new Uri(string.Format(@"{0}", path), UriKind.Relative));
                imageCache.Add(path, new BitMapImageCache(rst));
                TryClearCache();
                //MessageBox.Show("out of cache:" + path);
                return rst;
            }
        }

        private static DateTime lastClearTime = DateTime.MinValue;
        private static void TryClearCache()
        {
            if ((DateTime.Now - lastClearTime).TotalMinutes < imageCacheTimeInMinutes) return;
            List<string> toremoveKeys = new List<string>();
            foreach(var key in imageCache)
            {
                if ((DateTime.Now - key.Value.Time).TotalMinutes > imageCacheTimeInMinutes)
                    toremoveKeys.Add(key.Key);
            }
            foreach (var key in toremoveKeys)
                imageCache.Remove(key);
            lastClearTime = DateTime.Now;
        }

        public static void PutImageCache(string path, BitmapImage img)
        {
            //lock (imageCacheLocker)
            {
                imageCache[path] = new BitMapImageCache(img);
            }
        }

        public static string GetImageUrl(BitmapImage img)
        {
            //lock (imageCacheLocker)
            {
                foreach (var d in imageCache)
                {
                    if (d.Value.Image == img)
                    {
                        return d.Key;
                    }
                }
                return string.Empty;
            }
        }

        #region XML操作

        public static XElement LoadXml(string path)
        {
            try
            {
                //使用resource形式，将XML封装在DLL里
                if (Configer.Instance.ScriptMode == ScriptModeType.DLL)
                {
                    return XElement.Load("/JyGame;component/" + path);
                }
                else if(Configer.Instance.ScriptMode == ScriptModeType.XAP)//使用content形式，将XML封装在XAP里
                {
                    return XElement.Load(path);
                }
                else if(Configer.Instance.ScriptMode == ScriptModeType.FILE)
                {
                    StreamReader sr = new StreamReader(Configer.Instance.ScriptRootMenu + path);
                    string content = sr.ReadToEnd();
                    sr.Close();
                    return XElement.Parse(content);
                }
                return null;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                MessageBox.Show("xml载入错误:" + path);
                return null;
            }
        }

        public static XElement GetXmlElement(XElement xml, string key)
        {
            return xml.Element(key);
        }

        public static IEnumerable<XElement> GetXmlElements(XElement xml, string key)
        {
            return xml.Elements(key);
        }

        public static string GetXmlAttribute(XElement xml, string attribute)
        {
            return xml.Attribute(attribute).Value;
        }

        public static float GetXmlAttributeFloat(XElement xml, string attribute)
        {
            return float.Parse(xml.Attribute(attribute).Value);
        }

        public static int GetXmlAttributeInt(XElement xml, string attribute)
        {
            return int.Parse(xml.Attribute(attribute).Value);
        }

        public static bool GetXmlAttributeBool(XElement xml, string attribute)
        {
            return bool.Parse(xml.Attribute(attribute).Value);
        }

        public static DateTime GetXmlAttributeDate(XElement xml, string attribute)
        {
            return DateTime.ParseExact(xml.Attribute(attribute).Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
        }
        #endregion

        #region 数学方法

        private static Random rnd = new Random();

        /// <summary>
        /// 生成a到b之间的随机数
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double GetRandom(double a, double b)
        {
            double k = rnd.NextDouble();
            double tmp = 0;
            if (b > a)
            {
                tmp = a;
                a = b;
                b = tmp;
            }
            return b + (a - b) * k;
        }

        public static int GetRandomInt(int a, int b)
        {
            return (int)Tools.GetRandom(a, b+1);
        }

        /// <summary>
        /// 测试概率
        /// </summary>
        /// <param name="p">小于1的</param>
        /// <returns></returns>
        public static bool ProbabilityTest(double p)
        {
            if (p < 0) return false;
            if (p >= 1) return true;
            return rnd.NextDouble() < p;
        }

        #endregion

        #region 字符串操作

        /// <summary>
        /// 给字符串分成多行
        /// </summary>
        /// <param name="content"></param>
        /// <param name="lineLength"></param>
        /// <param name="enterFlag"></param>
        public static string StringToMultiLine(string content, int lineLength, string enterFlag="\n")
        {
            string rst = "";
            string tmp = content;
            while (tmp.Length > 0)
            {
                if(tmp.Length > lineLength)
                {
                    string line = tmp.Substring(0, lineLength);
                    tmp = tmp.Substring(lineLength, tmp.Length - lineLength);
                    rst += line + "\n";
                }
                else
                {
                    rst += tmp;
                    tmp = "";
                }
            }
            return rst;
        }


        /// <summary>
        /// 将字符串HASH成int
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int StringHashtoInt(string str)
        {
            int rst = 0;
            foreach(var c in str)
            {
                rst += Convert.ToInt32(c);
            }
            return rst;
        }
        #endregion
    }

    #region 数据加密
    public class DEncryptHelper
    {
        #region 整型加密解密

        //public static int EncryptInt(int k)
        //{
        //    return (k + 1024) << 2;
        //}

        //public static int DecryptInt(int k)
        //{
        //    return (k >> 2) - 1024;
        //}

        //public static string EncryptInt(int k)
        //{
        //    int random = Tools.GetRandomInt(0, 100);
        //    return ((k + random) << 3) + "#" + (random << 2);
        //}

        //public static int DecryptInt(string k)
        //{
        //    if (string.IsNullOrEmpty(k))
        //        return 0;
        //    string[] tmp = k.Split(new char[] { '#' });
        //    int number1 = int.Parse(tmp[0]) >> 3;
        //    int number2 = int.Parse(tmp[1]) >> 2;
        //    int random = number2;
        //    return number1 - random;
        //    //return int.Parse(k.Split(new char[] { '#' })[0]) -5;
        //}

        public static int EncryptInt(int k)
        {
            if (k % 2 == 0) return -k + 100;
            else return k + 44;
        }

        public static int DecryptInt(int k)
        {
            if (k % 2 == 0) return -(k - 100);
            else return k - 44;
        }

        #endregion
    }
    #endregion

    #region 加密的dict
    public class SecureDictionary
    {
        private Dictionary<string, int> _data = new Dictionary<string, int>();
        public int this[string key]
        {
            get
            {
                return DEncryptHelper.DecryptInt(_data[key]);
            }
            set
            {
                _data[key] = DEncryptHelper.EncryptInt(value);
            }
        }

        public bool ContainsKey(string key)
        {

            return _data.ContainsKey(key);
        }

        public Dictionary<string, int>.KeyCollection Keys
        {
            get
            {
                return _data.Keys;
            }
        }

    }

    #endregion
}
