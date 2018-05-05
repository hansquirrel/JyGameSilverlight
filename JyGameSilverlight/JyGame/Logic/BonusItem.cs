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
using JyGame.GameData;

namespace JyGame.Logic
{
    public class BonusItem
    {
        public string name;//名称
        public int totalNumber;//总共能拿到的数量, 0是无限制
        public double property;//概率

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">物品名称</param>
        /// <param name="totalNumber">最大能得到的数量</param>
        /// <param name="property">摇号概率 range:(0,1]</param>
        public BonusItem(string name, int totalNumber = 0, double property = 1)
        {
            this.name = name;
            this.totalNumber = totalNumber;
            this.property = property;
        }

        static public string GetRandomBonus(List<BonusItem> bonusList)
        {
            while (true)
            {
                BonusItem b = bonusList[Tools.GetRandomInt(0, bonusList.Count - 1) % bonusList.Count];
                string hashKey = "bonus_" + b.name;

                if (RuntimeData.Instance.KeyValues.ContainsKey(hashKey) && b.totalNumber > 0)
                {
                    int alreadyGetNumber = int.Parse(RuntimeData.Instance.KeyValues[hashKey]);
                    if (alreadyGetNumber >= b.totalNumber) //已经达到上限
                        continue;
                }

                if (!Tools.ProbabilityTest(b.property)) continue; //随机系数不满足

                if (b.totalNumber > 0)
                {
                    if (!RuntimeData.Instance.KeyValues.ContainsKey(hashKey))
                    {
                        RuntimeData.Instance.KeyValues[hashKey] = "1";
                    }
                    else
                    {
                        int alreadyGetNumber = int.Parse(RuntimeData.Instance.KeyValues[hashKey]);
                        RuntimeData.Instance.KeyValues[hashKey] = (alreadyGetNumber + 1).ToString();
                    }
                }

                return b.name;
            }
        }
    }
}
