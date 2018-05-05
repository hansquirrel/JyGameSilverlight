using System;
using System.Collections.Generic;
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
    /// <summary>
    /// 存放物品的箱子
    /// </summary>
    public static class XiangziManager
    {
        static public List<Item> Items
        {
            get
            {
                return RuntimeData.Instance.Xiangzi;
            }
            set
            {
                RuntimeData.Instance.Xiangzi = value;
            }
        }
    }
}
