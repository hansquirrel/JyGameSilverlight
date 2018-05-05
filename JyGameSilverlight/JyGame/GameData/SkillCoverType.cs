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
using JyGame.Logic;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace JyGame.GameData
{
    /// <summary>
    /// 技能攻击遮罩范围类型
    /// </summary>
    public enum SkillCoverType
    {
        NORMAL = 0, //点
        CROSS = 1, //十字
        STAR = 2, //星形
        LINE = 3, //直线
        FACE = 4, //面
        FAN = 5, //扇形
        RING = 6, //环状
        X = 7, //对角线攻击
    }


    /// <summary>
    /// 技能遮罩辅助类
    /// </summary>
    public class SkillCoverTypeHelper
    {
        public SkillCoverTypeHelper(SkillCoverType type)
        {
            _type = type;
        }

        private SkillCoverType _type;
        private int _typeCode { get { return (int)_type; } }
        
        string[] coverTypeInfoMap = new string[] {
            "点攻击",
            "十字攻击",
            "米字攻击",
            "直线攻击",
            "面攻击",
            "扇形攻击",
            "环状攻击",
            "对角线攻击",
        };
        float[] dSizeMap = new float[] { 0.12f, 0.2f, 0.15f, 0.15f, 0.3f, 0.23f, 0.2f, 0.3f };
        int[] dBaseCaseSizeMap = new int[] { 1, 0, 0, 1, 1, 1, 0, 0 };
        float[] dCastSizeMap = new float[] { 0.25f, 0f, 0f, 0.2f, 0f, 0f, 0f, 0f };


        /// <summary>
        /// 攻击成长范围
        /// </summary>
        public float dSize { get { return dSizeMap[_typeCode]; } }
        public int baseCastSize { get { return dBaseCaseSizeMap[_typeCode]; } }
        public float dCastSize { get { return dCastSizeMap[_typeCode]; } }
        public string CoverTypeInfo { get { return coverTypeInfoMap[_typeCode]; } }

        public int CostMp(float power, int size)
        {
            int rst = 0;
            switch (_type)
            {
                case SkillCoverType.NORMAL:
                    rst = (int)(power * 2 * 4);
                    break;
                case SkillCoverType.CROSS:
                    rst = (int)(power * size * 4);
                    break;
                case SkillCoverType.STAR:
                    rst = (int)(power * size * 1.3 * 4);
                    break;
                case SkillCoverType.LINE:
                    rst = (int)(power * size * 0.6 * 4);
                    break;
                case SkillCoverType.FACE:
                    rst = (int)(power * size * 2 * 4);
                    break;
                case SkillCoverType.FAN:
                    rst = (int)(power * size * 1.5 * 4);
                    break;
                default:
                    rst = (int)(power * size * 1.5 * 4);
                    break;
            }
            return rst;
        }

        public List<LocationBlock> GetSkillCoverBlocks(int x, int y, int spx, int spy, int coversize)
        {
            List<LocationBlock> rst = new List<LocationBlock>();
            
            switch (_type)
            {
                case SkillCoverType.NORMAL:
                    rst.Add(new LocationBlock() { X = x, Y = y });
                    break;
                case SkillCoverType.CROSS:
                    rst.Add(new LocationBlock() { X = x, Y = y });
                    for (int i = 1; i <= coversize; ++i)
                    {
                        rst.Add(new LocationBlock() { X = x + i, Y = y });
                        rst.Add(new LocationBlock() { X = x + i * (-1), Y = y });
                        rst.Add(new LocationBlock() { X = x, Y = y + i });
                        rst.Add(new LocationBlock() { X = x, Y = y + i * (-1) });
                    }
                    break;
                case SkillCoverType.LINE:
                    rst.Add(new LocationBlock() { X = x, Y = y });
                    int dx = 0;
                    int dy = 0;
                    if (x < spx) dx = -1;
                    if (x > spx) dx = 1;
                    if (y < spy) dy = -1;
                    if (y > spy) dy = 1;
                    for (int i = 1; i <= coversize; ++i)
                    {
                        rst.Add(new LocationBlock() { X = x + dx * i, Y = y + dy * i });
                    }
                    break;
                case SkillCoverType.STAR:
                    rst.Add(new LocationBlock() { X = x, Y = y });
                    for (int i = 1; i <= coversize; ++i)
                    {
                        rst.Add(new LocationBlock() { X = x + i, Y = y });
                        rst.Add(new LocationBlock() { X = x + i * (-1), Y = y });
                        rst.Add(new LocationBlock() { X = x, Y = y + i });
                        rst.Add(new LocationBlock() { X = x, Y = y + i * (-1) });

                        rst.Add(new LocationBlock() { X = x + i, Y = y + i });
                        rst.Add(new LocationBlock() { X = x + i * (-1), Y = y + i });
                        rst.Add(new LocationBlock() { X = x + i, Y = y + i * (-1) });
                        rst.Add(new LocationBlock() { X = x + i * (-1), Y = y + i * (-1) });
                    }
                    break;
                case SkillCoverType.FACE:
                    rst.Add(new LocationBlock() { X = x, Y = y });
                    for (int i = x - coversize / 2; i < x + coversize / 2 + 1; ++i)
                    {
                        for (int j = y - coversize / 2; j < y + coversize / 2 + 1; ++j)
                        {
                            if (rst.Count(p => p.X == i && p.Y == j) == 0)
                                rst.Add(new LocationBlock() { X = i, Y = j });
                        }
                    }
                    break;
                case SkillCoverType.FAN:
                    rst.Add(new LocationBlock() { X = x, Y = y });
                    dx = 0;
                    dy = 0;
                    if (x < spx) dx = -1;
                    if (x > spx) dx = 1;
                    if (y < spy) dy = -1;
                    if (y > spy) dy = 1;
                    if (dx + dy == 0) break;
                    for (int i = 1; i <= coversize; ++i)
                    {
                        rst.Add(new LocationBlock() { X = x + dx * i, Y = y + dy * i });
                        for (int j = 1; j <= i; ++j)
                        {
                            if (dx == 0)
                            {
                                rst.Add(new LocationBlock() { X = x + dx * i + j, Y = y + dy * i });
                                rst.Add(new LocationBlock() { X = x + dx * i - j, Y = y + dy * i });
                            }
                            else
                            {
                                rst.Add(new LocationBlock() { X = x + dx * i, Y = y + dy * i + j });
                                rst.Add(new LocationBlock() { X = x + dx * i, Y = y + dy * i - j });
                            }
                        }
                    }
                    break;
                case SkillCoverType.RING:
                    for (int i = -coversize; i <= coversize; ++i)
                    {
                        for (int j = -coversize; j <= coversize; ++j)
                        {
                            if (Math.Abs(i) + Math.Abs(j) == coversize)
                            {
                                rst.Add(new LocationBlock() { X = x + i, Y = y + j });
                            }
                        }
                    }
                    break;
                case SkillCoverType.X:
                    for (int i = 0; i < coversize; ++i)
                    {
                        rst.Add(new LocationBlock() { X = x + i, Y = y + i });
                        rst.Add(new LocationBlock() { X = x + i, Y = y - i });
                        rst.Add(new LocationBlock() { X = x - i, Y = y + i });
                        rst.Add(new LocationBlock() { X = x - i, Y = y - i });
                    }
                    break;
                default:
                    MessageBox.Show("invalid skill cover type");
                    break;
            }
            return rst;
        }

    }
}
