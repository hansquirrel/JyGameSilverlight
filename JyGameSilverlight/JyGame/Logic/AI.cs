using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using JyGame.UserControls;
using System.Collections.Generic;
using JyGame.GameData;

namespace JyGame.Logic
{
    public class BattleAI
    {
        public BattleAI(BattleField field)
        {
            this.Field = field;
        }

        private BattleField Field;


        /// <summary>
        /// 寻找移动范围
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public List<LocationBlock> GetMoveRange(int x, int y)
        {
            List<LocationBlock> rst = new List<LocationBlock>();
            //广度优先遍历，角色移动范围
            Queue<MoveSearchHelper> searchQueue = new Queue<MoveSearchHelper>();
            searchQueue.Enqueue(new MoveSearchHelper() { X = x, Y = y, Cost = 0 });

            while (searchQueue.Count > 0)
            {
                MoveSearchHelper currentNode = searchQueue.Dequeue();
                int xx = currentNode.X;
                int yy = currentNode.Y;
                int cost = currentNode.Cost;

                bool exist = false;
                foreach (var b in rst)
                {
                    if (b.X == xx && b.Y == yy)
                    {
                        exist = true;
                        break;
                    }
                }
                if (exist) continue;

                rst.Add(new LocationBlock() { X = xx, Y = yy });
                for (int i = 0; i < 4; ++i)
                {
                    int x2 = xx + directionX[i];
                    int y2 = yy + directionY[i];
                    int dcost = 1;

                    if (!Field.currentSpirit.Role.HasTalent("轻功大师"))
                    {
                        //周围有敌人，则增大代价
                        for (int j = 0; j < 4; ++j)
                        {
                            int x3 = x2 + directionX[j];
                            int y3 = y2 + directionY[j];
                            Spirit targetBlockSpirit = Field.GetSpirit(x3, y3);
                            if (targetBlockSpirit != null && targetBlockSpirit.Team != Field.currentSpirit.Team)
                            {
                                dcost = 2;
                                break;
                            }
                        }
                    }
                    if (IsEmptyBlock(x2,y2) 
                        && cost + dcost <= Field.currentSpirit.MoveAbility)
                    {
                        searchQueue.Enqueue(new MoveSearchHelper() { X = x2, Y = y2, Cost = cost + dcost });
                    }
                }
            }
            return rst;
        }

        public bool IsEmptyBlock(int x, int y)
        {
            return (x >= 0 && x < Field.actualXBlockNo && y >= 0 && y < Field.actualYBlcokNo &&
                        Field.GetSpirit(x, y) == null && Field.mapCoverLayer[x, y] == false);
        }

        /// <summary>
        /// 广度优先寻路
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="ex"></param>
        /// <param name="ey"></param>
        /// <returns></returns>
        public List<MoveSearchHelper> GetWay(int x, int y, int ex, int ey,bool ignoreSpirits = true)
        {
            bool[,] visited = new bool[Field.actualXBlockNo, Field.actualYBlcokNo];
            for (int i = 0; i < Field.actualXBlockNo; ++i)
                for (int j = 0; j < Field.actualYBlcokNo; ++j) visited[i, j] = false;

            Queue<MoveSearchHelper> queue = new Queue<MoveSearchHelper>();
            List<MoveSearchHelper> rst = new List<MoveSearchHelper>();
            queue.Enqueue(new MoveSearchHelper() { X = x, Y = y });
            visited[x, y] = true;
            while (queue.Count > 0)
            {
                MoveSearchHelper node = queue.Dequeue();
                if (node.X == ex && node.Y == ey)
                {
                    do
                    {
                        rst.Add(node);
                        node = node.front;
                    } while (node != null);
                    rst.Reverse();
                    return rst;
                }
                for (int i = 0; i < 4; ++i)
                {
                    int newx = node.X + directionX[i];
                    int newy = node.Y + directionY[i];
                    if ((ignoreSpirits && Field.GetSpirit(newx, newy) != null) || newx < 0 || newx >= Field.actualXBlockNo || newy < 0 ||
                        newy >= Field.actualYBlcokNo || Field.mapCoverLayer[newx, newy] == true || visited[newx, newy]) continue;
                    queue.Enqueue(new MoveSearchHelper() { X = newx, Y = newy, front = node });
                    visited[newx, newy] = true;
                }
            }
            return new List<MoveSearchHelper>();
        }

        /// <summary>
        /// 获取绝对距离
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int GetAbsoluteDistance(Spirit a, Spirit b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        /// <summary>
        /// 获取地图寻路距离
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int GetDistance(Spirit a, Spirit b)
        {
            List<MoveSearchHelper> rst = GetWay(a.X, a.Y, b.X, b.Y, false);
            if(rst == null ) return 0;
            else return rst.Count;
        }

        private int GetAbsoluteDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        private int GetDistance(int x1, int y1, int x2, int y2)
        {
            List<MoveSearchHelper> rst = GetWay(x1, y1, x2, y2, false);
            if (rst == null) return 0;
            else return rst.Count;
        }

        public AIResult GetAIResult()
        {
            AIResult rst = new AIResult();
            List<LocationBlock> range = this.GetMoveRange(Field.currentSpirit.X, Field.currentSpirit.Y);
            //若HP低下，则概率逃跑
            if ((double)Field.currentSpirit.Hp / (double)Field.currentSpirit.Role.Attributes["maxhp"] < 0.3 
                && Tools.ProbabilityTest(0.5))
            {
                int max = 0;
                //寻找一个点离敌人最远
                foreach (var r in range)
                {
                    int min = int.MaxValue;
                    foreach (Spirit sp in Field.Spirits)
                    {
                        int distance = GetDistance(sp.X, sp.Y, r.X, r.Y);
                        if (sp.Team != Field.currentSpirit.Team && distance < min)
                        {
                            min = distance;
                        }
                    }
                    if (min > max)
                    {
                        max = min;
                        rst.MoveX = r.X;
                        rst.MoveY = r.Y;
                        rst.IsRest = true;
                    }
                }
                return rst;
            }

            //AI策略：穷举移动到每一个能移动的点，并且使用技能攻击，选择伤害最大的攻击
            double maxAttack = 0;

            List<SkillBox> useSkills = new List<SkillBox>();
            //对AI进行减枝，控制技能数量
            foreach (var skill in Field.currentSpirit.Role.GetAvaliableSkills())
            {
                if (!(skill.Status == SkillStatus.Ok))
                    continue;
                if (useSkills.Count < CommonSettings.AI_MAX_COMPUTE_SKILL) useSkills.Add(skill);
                else
                {
                    //替换一个攻击最低的技能
                    double min = double.MaxValue;
                    SkillBox minPowerSkill = null;
                    foreach (var s in useSkills) 
                    {
                        if (s.Power < min)
                        {
                            min = s.Power;
                            minPowerSkill = s;
                        }
                    }
                    if (minPowerSkill != null && skill.Power > minPowerSkill.Power)
                    {
                        useSkills.Remove(minPowerSkill);
                        useSkills.Add(skill);
                    }
                }
            }

            //对走位进行剪枝，控制NPC行走
            List<LocationBlock> useMoveRange = new List<LocationBlock>();
            useMoveRange.Add(new LocationBlock() { X = Field.currentSpirit.X, Y = Field.currentSpirit.Y }); //默认的把不移动加上
            if (range.Count > CommonSettings.AI_MAX_COMPUTE_MOVERANGE)
            {
                while (useMoveRange.Count < CommonSettings.AI_MAX_COMPUTE_MOVERANGE)
                {
                    LocationBlock block = range[Tools.GetRandomInt(0, range.Count - 1)];
                    if (!useMoveRange.Contains(block)) useMoveRange.Add(block);
                }
            }
            else
            {
                useMoveRange = range;
            }

            foreach (var skill in useSkills) //选技能
            {
                if (!(skill.Status == SkillStatus.Ok))
                    continue;
                //DateTime t1 = DateTime.Now;
                foreach (var r in useMoveRange) //走位
                {
                    int x = r.X;
                    int y = r.Y;
                    List<LocationBlock> castRange = skill.GetSkillCastBlocks(x, y);
                    foreach (var cast in castRange)
                    {
                        double totalHit = 0;
                        List<LocationBlock> hitRange = skill.GetSkillCoverBlocks(cast.X, cast.Y, x, y);
                        foreach (var hit in hitRange)
                        {
                            Spirit target = Field.GetSpirit(hit.X, hit.Y);
                            if (target == null || target == Field.currentSpirit) continue;
                            
                            AttackResult attackResult = skill.Attack(Field.currentSpirit, target);
                            rst.totalAttackComputeNum++;
                            if (target.Team != Field.currentSpirit.Team)
                            {
                                totalHit += attackResult.Hp; //攻击到敌人
                            }
                            else if (target.Team == Field.currentSpirit.Team && 
                                RuntimeData.Instance.FriendlyFire)
                            {
                                totalHit -= attackResult.Hp / 2; //攻击到友方
                            }
                        }
                        if (totalHit > maxAttack)
                        {
                            rst.MoveX = x;
                            rst.MoveY = y;
                            rst.skill = skill;
                            rst.IsRest = false;
                            rst.AttackX = cast.X;
                            rst.AttackY = cast.Y;
                            maxAttack = totalHit;
                        }
                    }
                }
                
                //DateTime t2 = DateTime.Now;
                //MessageBox.Show((t2 - t1).TotalMilliseconds.ToString());
            }
            if (rst.skill != null)
            {
                return rst;//攻击对手
            }

            //否则靠近自己最近的敌人
            int minDistance = int.MaxValue;
            Spirit targetRole = null;
            //寻找离自己最近的敌人
            foreach (Spirit sp in Field.Spirits)
            {
                if (sp.Team == Field.currentSpirit.Team)
                    continue;
                int distance = GetDistance(sp, Field.currentSpirit);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    targetRole = sp;
                }
            }

            if (targetRole != null)
            {
                int minDis2 = int.MaxValue;
                int movex = Field.currentSpirit.X, movey = Field.currentSpirit.Y;
                //寻找离对手最近的一点
                foreach (var mr in range)
                {
                    int distance = GetDistance(mr.X, mr.Y, targetRole.X, targetRole.Y);
                    if (distance <= minDis2)
                    {
                        minDis2 = distance;
                        movex = mr.X;
                        movey = mr.Y;
                    }
                }
                rst.skill = null;
                rst.MoveX = movex;
                rst.MoveY = movey;
                rst.IsRest = true; //靠近对手


                //如果有增益性技能，则一定概率使用
                if (Tools.ProbabilityTest(0.5))
                {
                    foreach (var s in Field.currentSpirit.Role.SpecialSkills)
                    {
                        SkillBox sb = new SkillBox() { SpecialSkill = s };
                        if (sb.Status == SkillStatus.Ok && s.Skill.HitSelf)
                        {
                            rst.skill = sb;
                            rst.AttackX = movex;
                            rst.AttackY = movey;
                            rst.IsRest = false;
                        }
                    }
                }

                return rst;
            }
            else
            {
                //否则休息
                rst.MoveX = Field.currentSpirit.X;
                rst.MoveY = Field.currentSpirit.Y;
                rst.IsRest = true; //没有对手，休息
                return rst;
            }
        }

        int[] directionX = new int[] { 1, 0, -1, 0 };
        int[] directionY = new int[] { 0, 1, 0, -1 };
    }

    public class AIResult
    {
        public int MoveX;
        public int MoveY;

        public SkillBox skill;
        public int AttackX;
        public int AttackY;
        public bool IsRest;

        public int totalAttackComputeNum = 0;
    }

    public struct LocationBlock
    {
        public int X;
        public int Y;
    }

    public class MoveSearchHelper
    {
        public int X;
        public int Y;
        public int Cost;
        public MoveSearchHelper front;
    }

}
