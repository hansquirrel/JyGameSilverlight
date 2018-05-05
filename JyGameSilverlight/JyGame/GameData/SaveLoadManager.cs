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
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

namespace JyGame.GameData
{
    public class SaveInfo
    {
        public SaveInfo(string name, XElement node)
        {
            Name = name;
            root = node;

            XElement eventMaskNode = Tools.GetXmlElement(root, "keyvalues");
            foreach (var attr in eventMaskNode.Attributes())
            {
                KeyValues[attr.Name.ToString()] = attr.Value;
            }
        }
        public string Name;
        public XElement root;
        Dictionary<string, string> KeyValues = new Dictionary<string, string>();

        public override string ToString()
        {

            string date = string.Format("江湖{0}年{1}月{2}日",
                CommonSettings.chineseNumber[Time.Date.Year],
                CommonSettings.chineseNumber[Time.Date.Month],
                CommonSettings.chineseNumber[Time.Date.Day]);
            
            string gameMode = "难度:简单";
            if (GameMode == "hard") gameMode = "难度:进阶";
            if (GameMode == "crazy") gameMode = "难度:炼狱";

            string gameRound = "周目:" + RuntimeData.Instance.Round;

            return string.Format("{0}\n\n{1}\n当前位置:{2}\n{3}\n{4}",
                Name,
                date,
                Locate,
                gameMode,
                gameRound);
        }

        public DateTime Time
        {
            get
            {
                if (!KeyValues.ContainsKey("date"))
                    KeyValues.Add("date", 
                        System.DateTime.ParseExact("0001-01-01 10:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd HH:mm:ss"));
                try
                {
                    return DateTime.Parse(KeyValues["date"]);
                }
                catch
                {
                    return DateTime.ParseExact(KeyValues["date"], "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                }
            }
        }

        public string Locate
        {
            get
            {
                if (!KeyValues.ContainsKey("currentBigMap"))
                    KeyValues.Add("currentBigMap", "");
                return KeyValues["currentBigMap"];
            }
        }

        public string GameMode
        {
            get
            {
                if (!KeyValues.ContainsKey("mode"))
                    KeyValues.Add("mode", "normal");
                return KeyValues["mode"];
            }
        }
    }

    public class SaveLoadManager
    {
        public const long SAVE_SPACE = 1024 * 1024 * 50;

        #region singleton
        static public SaveLoadManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SaveLoadManager();
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if(store.Quota != SAVE_SPACE)
                        {
                            store.IncreaseQuotaTo(SAVE_SPACE);
                        }
                    }
                }
                return _instance;
            }
        }
        
        static private SaveLoadManager _instance = null;
        #endregion

        #region method
        //public int SaveCount
        //{
        //    get
        //    {
        //        if (_count == -1)
        //            _count = this.GetList().Count;
        //        return _count;
        //    }
        //}
        //private int _count = -1;

        public List<SaveInfo> GetList()
        {
            this.LoadData();
            List<SaveInfo> rst = new List<SaveInfo>();
            if (SaveData == null)
                return rst;
            foreach (var save in SaveData.Elements("save"))
            {
                string name = Tools.GetXmlAttribute(save, "name");
                XElement node = save.Element("root");
                rst.Add(new SaveInfo(name, node));
            }
            //_count = rst.Count;
            return rst;
        }

        public void Save(string saveName, XElement node)
        {
            this.LoadData();
            XElement nodeToDel = this.GetSave(saveName);
            if (nodeToDel != null)
            {
                nodeToDel.Remove();
            }
            XElement saveNode = new XElement("save");
            saveNode.SetAttributeValue("name", saveName);
            saveNode.Add(node);
            SaveData.Add(saveNode);
            WriteData();
        }

        public XElement Load(string saveName)
        {
            this.LoadData();
            XElement pointer = GetSave(saveName);
            if (pointer != null)
                return pointer.Element("root");
            else
                return null;
        }

        public void DeleteSave(string saveName)
        {
            this.LoadData();
            XElement node = this.GetSave(saveName);
            if (node != null)
            {
                node.Remove();
            }
            //_count--;
            this.WriteData();
        }

        private XElement GetSave(string saveName)
        {
            XElement pointer = null;
            foreach (var save in SaveData.Elements("save"))
            {
                if (Tools.GetXmlAttribute(save, "name") == saveName)
                {
                    pointer = save;
                    break;
                }
            }
            return pointer;
        }

        public void CreateEmptySave()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists("savedata"))
                {
                    store.CopyFile("savedata", "savedatabackup",true);
                    store.DeleteFile("savedata");
                }
                if (!store.FileExists("savedata"))
                {
                    string enc = "";
                    using (StreamWriter sw = new StreamWriter(store.OpenFile("savedata", FileMode.Create, FileAccess.Write)))
                    {
                        XElement root = new XElement("savedata");
                        enc = jm(root.ToString());//加密
                        

                        string crc = CRC16_C(enc); //对存档求crc
                        //IsolatedStorageSettings.ApplicationSettings["c"] = crc;
                        sw.Write(crc+"@"+enc);
                    }
                }
            }
        }

        private void LoadData()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists("savedata"))
                    CreateEmptySave();

                this.SaveData = null;
                string tmp = "";
                try
                {
                    //载入
                    using (StreamReader sr = new StreamReader(store.OpenFile("savedata", FileMode.Open, FileAccess.Read)))
                    {
                        tmp = sr.ReadToEnd();
                    }
                    string crc = "";
                    string content = "";
                    bool findCrcTag = false;
                    for (int i = 0; i < 8; ++i)
                    {
                        if (tmp[i] == '@')
                        {
                            crc = tmp.Substring(0, i);
                            content = tmp.Substring(i + 1, tmp.Length - i - 1);
                            if(CRC16_C(content)!=crc)
                            {
                                MessageBoxResult rst =
                                MessageBox.Show(
                                    "读取失败！你的存档可能被篡改！请勿使用修改器之类工具。是否清空所有存档？",
                                    "删除确认",
                                    MessageBoxButton.OKCancel);
                                if (rst == MessageBoxResult.OK)
                                {
                                    this.CreateEmptySave();
                                }
                                SaveData = null;
                                return;
                            }
                            findCrcTag = true;
                            break;
                        }
                    }
                    if (!findCrcTag)
                    {
                        MessageBoxResult rst =
                                MessageBox.Show(
                                    "读取失败！你的存档可能被篡改！请勿使用修改器之类工具。是否清空所有存档？",
                                    "删除确认",
                                    MessageBoxButton.OKCancel);
                        if (rst == MessageBoxResult.OK)
                        {
                            this.CreateEmptySave();
                        }
                        SaveData = null;
                        return;
                    }
                    string c = m(content); //解密
                    SaveData = XElement.Parse(c);
                    //if (IsolatedStorageSettings.ApplicationSettings.Contains("c"))
                    //{
                    //    string crc = (string)IsolatedStorageSettings.ApplicationSettings["c"];
                    //    if (CRC16_C(tmp) != crc)
                    //    {
                    //        MessageBoxResult rst =
                    //            MessageBox.Show(
                    //                "读取失败！你的存档可能被篡改！请勿使用修改器之类工具。是否清空所有存档？",
                    //                "删除确认",
                    //                MessageBoxButton.OKCancel);
                    //        if (rst == MessageBoxResult.OK)
                    //        {
                    //            this.CreateEmptySave();
                    //        }
                    //        SaveData = null;
                    //        return;
                    //    }
                    //}
                }
                catch(Exception e)
                {
                    //MessageBox.Show("存档文件读取错误，错误代码：" + e.ToString() + ",tmp=" + tmp);

                    MessageBoxResult rst =
                                MessageBox.Show(
                                    "读取失败！你的存档可能被篡改！请勿使用修改器之类工具。是否清空所有存档？",
                                    "删除确认",
                                    MessageBoxButton.OKCancel);
                    if (rst == MessageBoxResult.OK)
                    {
                        this.CreateEmptySave();
                    }
                    SaveData = null;
                    return;
                }
            }
        }

        private void WriteData()
        {
            if (SaveData == null || SaveData.ToString() == "")
                this.LoadData();
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string enc = "";
                    enc = jm(SaveData.ToString());//加密
                    string crc = CRC16_C(enc); //对存档求crc

                    using (StreamWriter sw = new StreamWriter(store.OpenFile("savedata_new", FileMode.Create, FileAccess.Write)))
                    {
                        if (enc != "")
                        {
                            sw.Write(crc + "@");
                            sw.Write(enc);
                        }
                    }
                    store.CopyFile("savedata_new", "savedata", true);
                }
            }catch(Exception e)
            {
                MessageBox.Show("存储异常:" + e.ToString());
                MessageBox.Show("存储异常:" + e.StackTrace);
            }
        }

        private XElement SaveData;

        #endregion

        #region secure
        private string saltValue = "fuck1" + CommonSettings.Key;
        private string pwdValue = CommonSettings.Key + "fuck1";

        #region 加密
        /**/
        /// <summary>
        /// 加密数据
        /// </summary>
        /// <param name="input">加密前的字符串</param>
        /// <returns>加密后的字符串</returns>
        private string jm(string input)
        {
            byte[] data = UTF8Encoding.UTF8.GetBytes(input);
            byte[] salt = UTF8Encoding.UTF8.GetBytes(saltValue);

            // AesManaged - 高级加密标准(AES) 对称算法的管理类
            AesManaged aes = new AesManaged();

            // Rfc2898DeriveBytes - 通过使用基于 HMACSHA1 的伪随机数生成器，实现基于密码的密钥派生功能 (PBKDF2 - 一种基于密码的密钥派生函数)
            // 通过 密码 和 salt 派生密钥
            Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(pwdValue, salt);

            /**/
            /*
         * AesManaged.BlockSize - 加密操作的块大小（单位：bit）
         * AesManaged.LegalBlockSizes - 对称算法支持的块大小（单位：bit）
         * AesManaged.KeySize - 对称算法的密钥大小（单位：bit）
         * AesManaged.LegalKeySizes - 对称算法支持的密钥大小（单位：bit）
         * AesManaged.Key - 对称算法的密钥
         * AesManaged.IV - 对称算法的密钥大小
         * Rfc2898DeriveBytes.GetBytes(int 需要生成的伪随机密钥字节数) - 生成密钥
         */

            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            aes.Key = rfc.GetBytes(aes.KeySize / 8);
            aes.IV = rfc.GetBytes(aes.BlockSize / 8);

            // 用当前的 Key 属性和初始化向量 IV 创建对称加密器对象
            ICryptoTransform encryptTransform = aes.CreateEncryptor();

            // 加密后的输出流
            MemoryStream encryptStream = new MemoryStream();

            // 将加密后的目标流（encryptStream）与加密转换（encryptTransform）相连接
            CryptoStream encryptor = new CryptoStream(encryptStream, encryptTransform, CryptoStreamMode.Write);

            // 将一个字节序列写入当前 CryptoStream （完成加密的过程）
            encryptor.Write(data, 0, data.Length);
            encryptor.Close();

            // 将加密后所得到的流转换成字节数组，再用Base64编码将其转换为字符串
            string encryptedString = Convert.ToBase64String(encryptStream.ToArray());

            return encryptedString;
        }
        #endregion

        #region 解密
        /**/
        /// <summary>
        /// 解密数据
        /// </summary>
        /// <param name="input">加密后的字符串</param>
        /// <returns>加密前的字符串</returns>
        private string m(string input)
        {
            byte[] encryptBytes = Convert.FromBase64String(input);
            byte[] salt = Encoding.UTF8.GetBytes(saltValue);

            AesManaged aes = new AesManaged();

            Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(pwdValue, salt);

            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            aes.Key = rfc.GetBytes(aes.KeySize / 8);
            aes.IV = rfc.GetBytes(aes.BlockSize / 8);

            // 用当前的 Key 属性和初始化向量 IV 创建对称解密器对象
            ICryptoTransform decryptTransform = aes.CreateDecryptor();

            // 解密后的输出流
            MemoryStream decryptStream = new MemoryStream();

            // 将解密后的目标流（decryptStream）与解密转换（decryptTransform）相连接
            CryptoStream decryptor = new CryptoStream(decryptStream, decryptTransform, System.Security.Cryptography.CryptoStreamMode.Write);

            // 将一个字节序列写入当前 CryptoStream （完成解密的过程）
            decryptor.Write(encryptBytes, 0, encryptBytes.Length);
            
            decryptor.Close();

            // 将解密后所得到的流转换为字符串
            byte[] decryptBytes = decryptStream.ToArray();
            string decryptedString = UTF8Encoding.UTF8.GetString(decryptBytes, 0, decryptBytes.Length);

            return decryptedString;
        }

        #endregion

        #region CRC
        private string CRC16_C(string str)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(str);
            byte CRC16Lo;
            byte CRC16Hi;   //CRC寄存器 
            byte CL; byte CH;       //多项式码&HA001 
            byte SaveHi; byte SaveLo;
            byte[] tmpData;
            int I;
            int Flag;
            CRC16Lo = 0xFF;
            CRC16Hi = 0xFF;
            CL = 0x01;
            CH = 0xA0;
            tmpData = data;
            for (int i = 0; i < tmpData.Length; i++)
            {
                CRC16Lo = (byte)(CRC16Lo ^ tmpData[i]); //每一个数据与CRC寄存器进行异或 
                for (Flag = 0; Flag <= 7; Flag++)
                {
                    SaveHi = CRC16Hi;
                    SaveLo = CRC16Lo;
                    CRC16Hi = (byte)(CRC16Hi >> 1);      //高位右移一位 
                    CRC16Lo = (byte)(CRC16Lo >> 1);      //低位右移一位 
                    if ((SaveHi & 0x01) == 0x01) //如果高位字节最后一位为1 
                    {
                        CRC16Lo = (byte)(CRC16Lo | 0x80);   //则低位字节右移后前面补1 
                    }             //否则自动补0 
                    if ((SaveLo & 0x01) == 0x01) //如果LSB为1，则与多项式码进行异或 
                    {
                        CRC16Hi = (byte)(CRC16Hi ^ CH);
                        CRC16Lo = (byte)(CRC16Lo ^ CL);
                    }
                }
            }
            return string.Format("{0}{1}",CRC16Hi,CRC16Lo);
        } 
        #endregion

        #endregion
    }
}
