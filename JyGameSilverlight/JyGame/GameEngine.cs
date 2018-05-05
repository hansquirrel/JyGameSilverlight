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
using JyGame.GameData;
using JyGame.Interface;
using System.Collections.ObjectModel;

namespace JyGame
{
    public class NextGameState
    {
        public string Type { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", Type, Value);
        }
    };

    public class GameEngine
    {
        public string currentStatus = null;

        public GameEngine(UIHost uihost)
        {
            this.uihost = uihost;
        }

        public void RollRole()
        {
            this.uihost.Dispatcher.BeginInvoke(() =>
            {
                CallScence(null, new NextGameState() { Type = "rollrole" });
            });
        }

        public void NewGame()
        {
            RuntimeData.Instance.SetLocation("大地图","南贤居");
            RuntimeData.Instance.TrialRoles = ""; //清空霹雳堂的人
            RuntimeData.Instance.Rank = -1;
            this.uihost.Dispatcher.BeginInvoke(() =>
            {
                RuntimeData.Instance.Rank = -1;
                CallScence(null, new NextGameState() { Type = "story", Value = "original_序章" });
            });
        }

        public void NewGameJump()
        {
            RuntimeData.Instance.KeyValues["original_主角之家.开场"] = "0";
            RuntimeData.Instance.SetLocation("大地图", "南贤居");
            RuntimeData.Instance.TrialRoles = ""; //清空霹雳堂的人
            this.uihost.Dispatcher.BeginInvoke(() =>
            {
                RuntimeData.Instance.Rank = -1;
                CallScence(null, new NextGameState() { Type = "story", Value = "original_主角之家.开场.跳过" });
            });
        }

        public void LoadGame()
        {
            this.uihost.Dispatcher.BeginInvoke(() =>
            {
                RuntimeData.Instance.Rank = -1;
                CallScence(null, new NextGameState() { Type = "map", Value = RuntimeData.Instance.CurrentBigMap });
            });
        } 

        public void CallScence(IScence sender, NextGameState next)
        {
            if (next == null)
            {
                if (sender != null)
                    sender.Hide();
                //this.uihost.Dispatcher.BeginInvoke(() => { this.Scence(next); });
                this.Scence(next);
                return;
            }

            //this.uihost.Dispatcher.BeginInvoke(() => { this.Scence(next); });
            if (sender != null)
                sender.Hide();
            this.Scence(next);
            //this.Scence(next);
        }

        public void LoadMap(string mapKey)
        {
            //每次回到地图，都需要判断当前时间是否会触发主线剧情
            TimeTrigger tt = TimeTriggerManager.GetCurrentTrigger();

            if (tt != null)
            {
                //uihost.scence.Load(story);
                Scence(new NextGameState() { Type = "story", Value = tt.story });
            }
            else
            {
                lastScenceIsMap = true;
                if (mapKey == "" || mapKey == null)
                {
                    uihost.mapUI.Load("大地图");
                }
                else
                {
                    uihost.mapUI.Load(mapKey);
                }
            }
        }
        private bool lastScenceIsMap = false;
        private void Scence(NextGameState next)
        {
            if(RuntimeData.Instance.IsCheated)
            {
                MessageBox.Show("系统检测到你正在作弊，将结束游戏");
                uihost.gameOverPanel.Show();
                return;
            }

            uihost.reset();
            if (next == null)
            {
                uihost.mapUI.resetTeam();
                uihost.mapUI.Load(RuntimeData.Instance.CurrentBigMap);
                return;
            }

            //GC.Collect();
            currentStatus = next.Type;

            if(Configer.Instance.Debug)
            {
                App.GameStudio.GameState(next);
            }

            switch (next.Type)
            {
                case "rollrole":
                    uihost.rollRolePanel.Show();
                    break;
                case "tutorial":
                    Tutorial();
                    break;
                case "map":
                    uihost.mapUI.resetTeam();
                    if (next.Value == null || next.Value.Equals(string.Empty))
                    {
                        next.Value = RuntimeData.Instance.CurrentBigMap;
                    }
                    RuntimeData.Instance.Date = RuntimeData.Instance.Date.AddHours(1); //地图耗时
                    if (Tools.ProbabilityTest(0.1))
                    {
                        RuntimeData.Instance.CheckCheat();
                    }
                    RuntimeData.Instance.CheckTimeFlags();
                    LoadMap(next.Value);
                    if (Configer.Instance.AutoSave)
                    {
                        RuntimeData.Instance.Save("自动存档");
                    }
                    break;
                //case "battle":
                //    uihost.battleFieldContainer.Load(next.Value);
                //    break;
                //case "scenario":
                //    uihost.scence.Load(next.Value);
                //    break;
                case "battle":
                    Story story = new Story() { Name = "battle_" + next.Value };
                    story.Actions.Add(new GameData.Action() { Type = "BATTLE", Value = next.Value });
                    StoryManager.PlayStory(story, lastScenceIsMap);
                    break;
                case "story":
                    StoryManager.PlayStory(next.Value, lastScenceIsMap);
                    lastScenceIsMap = false;
                    break;
                case "arena":
                    Arena();
                    break;
                case "trial":
                    Trial();
                    break;
                case "tower":
                    Tower(true);
                    break;
                case "nextTower":
                    Tower(false);
                    break;
                case "huashan":
                    Huashan(true);
                    break;
                case "nextHuashan":
                    Huashan(false);
                    break;
                case "OLBattleHost":
                    //OLBattle(true, 1);
                    break;
                case "nextOLBattleHost":
                    //OLBattle(false, 1);
                    break;
                case "OLBattleGuest":
                    //OLBattle(true, 2);
                    break;
                case "nextOLBattleGuest":
                    //OLBattle(false, 2);
                    break;
                case "restart":
                    Restart();
                    break;
                case "nextZhoumu":
                    NextZhoumu();
                    break;
                case "gameOver":
                    uihost.gameOverPanel.Show();
                    break;
                case "gameFin":
                    uihost.fin.Visibility = Visibility.Visible;
                    break;
                case "shop":
                    uihost.shopPanel.Show(ShopManager.GetShop(next.Value));
                    break;
                case "sellshop":
                    uihost.shopPanel.Show(SellShopManager.GetSellShop(next.Value));
                    break;
                case "game":
                    uihost.playSmallGame(next.Value);
                    break;
                case "mainmenu":
                    uihost.mainMenu.Visibility = Visibility.Visible;
                    break;
                case "xiangzi": //储物箱
                    uihost.shopPanel.ShowXiangzi();
                    break;
                case "item": //获得一个物品
                    RuntimeData.Instance.Items.Add(ItemManager.GetItem(next.Value).Clone());
                    LoadMap(RuntimeData.Instance.CurrentBigMap);
                    break;
                case "danmu":
                    uihost.AddDanMu(next.Value);
                    LoadMap(RuntimeData.Instance.CurrentBigMap);
                    break;
                case "wudaodahui":
                    if(RuntimeData.Instance.IsCheated)
                    {
                        MessageBox.Show("系统检测到你正在作弊，禁止进入武道大会");
                        LoadMap(RuntimeData.Instance.CurrentBigMap);
                        return;
                    }
                    uihost.wudaodahuiPanel.Show();
                    break;
                case "dropallsaves":
                    SaveLoadManager.Instance.CreateEmptySave();
                    return;
                default:
                    MessageBox.Show("error scence type: " + next.Type);
                    uihost.mapUI.resetTeam();
                    uihost.mapUI.Load(RuntimeData.Instance.CurrentBigMap);
                    break;
            }
        }

        private void Tutorial()
        {
            uihost.mapUI.resetTeam();
            LoadMap("大地图");
            CallScence(null, new NextGameState() { Type = "story", Value = "original_教学开始" });
        }

        private void Arena()
        {
            uihost.arenaSelectRole.cancel.Visibility = Visibility.Visible;
            uihost.arenaSelectRole.confirmBack = () =>
                {
                    this.uihost.Dispatcher.BeginInvoke(() =>
                    {
                        uihost.battleFieldContainer.LoadArena(uihost.arenaSelectScene.battle, uihost.arenaSelectRole.selectedFriends, uihost.arenaSelectScene.selectedEnemies);
                    });
                };

            uihost.arenaSelectRole.cancelBack = () =>
                {
                    this.uihost.Dispatcher.BeginInvoke(() =>
                    {
                        uihost.mapUI.resetTeam();
                        uihost.mapUI.Load(RuntimeData.Instance.CurrentBigMap);
                    });
                };

            uihost.arenaSelectScene.confirmBack = () =>
                {
                    this.uihost.Dispatcher.BeginInvoke(() =>
                        {
                            uihost.arenaSelectRole.load(uihost.arenaSelectScene.maxFriendNo, null);
                        });
                };

            uihost.arenaSelectScene.cancelBack = () =>
                {
                    this.uihost.Dispatcher.BeginInvoke(() =>
                    {
                        uihost.mapUI.resetTeam();
                        uihost.mapUI.Load(RuntimeData.Instance.CurrentBigMap);
                    });
                };

            uihost.arenaSelectScene.load();
        }

        private void Tower(bool firstTower)
        {
            uihost.towerSelectRole.cancel.Visibility = Visibility.Visible;
            uihost.towerSelectRole.confirmBack = () =>
            {
                foreach (int i in uihost.towerSelectRole.selectedFriends)
                {
                    uihost.towerSelectScene.cannotSelected.Add(RuntimeData.Instance.Team[i].Key);
                }

                this.uihost.Dispatcher.BeginInvoke(() =>
                {
                    Battle battle = TowerManager.getTower(uihost.towerSelectScene.currentTower)[uihost.towerSelectScene.currentIndex];
                    uihost.battleFieldContainer.LoadTower(battle, uihost.towerSelectRole.selectedFriends);
                });
            };

            uihost.towerSelectScene.confirmBack = () =>
            {
                this.uihost.Dispatcher.BeginInvoke(() =>
                {
                    uihost.towerSelectRole.load(uihost.towerSelectScene.maxFriendNo, null, uihost.towerSelectScene.cannotSelected);
                });
            };

            uihost.towerSelectScene.cancelBack = () =>
            {
                this.uihost.Dispatcher.BeginInvoke(() =>
                {
                    uihost.mapUI.resetTeam();
                    uihost.mapUI.Load(RuntimeData.Instance.CurrentBigMap);
                });
            };

            if(firstTower)
                uihost.towerSelectScene.load();
            else
            {
                uihost.towerSelectScene.currentIndex++;
                if(uihost.towerSelectScene.currentIndex >= TowerManager.getTower(uihost.towerSelectScene.currentTower).Count)
                {
                    uihost.towerSelectScene.showBonus();
                }
                else
                {
                    Battle battle = TowerManager.getTower(uihost.towerSelectScene.currentTower)[uihost.towerSelectScene.currentIndex];
                    uihost.towerSelectScene.calcFriend(battle);
                    uihost.towerSelectRole.load(uihost.towerSelectScene.maxFriendNo, null, uihost.towerSelectScene.cannotSelected);
                 }
             }
        }

        private void Huashan(bool firstHuashan)
        {
            uihost.towerSelectRole.cancel.Visibility = Visibility.Visible;
            uihost.towerSelectRole.confirmBack = () =>
            {
                foreach (int i in uihost.towerSelectRole.selectedFriends)
                {
                    uihost.towerSelectScene.cannotSelected.Add(RuntimeData.Instance.Team[i].Key);
                }

                this.uihost.Dispatcher.BeginInvoke(() =>
                {
                    Battle battle = TowerManager.getTower(uihost.towerSelectScene.currentTower)[uihost.towerSelectScene.currentIndex];
                    uihost.battleFieldContainer.LoadHuashan(battle, uihost.towerSelectRole.selectedFriends);
                });
            };

            if (firstHuashan)
            {
                uihost.towerSelectScene.loadHuashan();
                uihost.towerSelectScene.currentTower = "华山论剑";
                Battle battle = TowerManager.getTower(uihost.towerSelectScene.currentTower)[uihost.towerSelectScene.currentIndex];
                uihost.towerSelectScene.calcFriend(battle);
                uihost.towerSelectRole.load(uihost.towerSelectScene.maxFriendNo, null, uihost.towerSelectScene.cannotSelected);
            }
            else
            {
                uihost.towerSelectScene.currentIndex++;
                Battle battle = TowerManager.getTower(uihost.towerSelectScene.currentTower)[uihost.towerSelectScene.currentIndex];
                uihost.towerSelectScene.calcFriend(battle);
                uihost.towerSelectRole.load(uihost.towerSelectScene.maxFriendNo, null, uihost.towerSelectScene.cannotSelected);
            }
        }

        //myTeamIndex:分边确认初始位置，自己属于编组1，还是编组2。一旦load进入战斗，在自己电脑上自己总是编组1
        public void OLBattle(bool firstOLBattle, int myTeamIndex,string channel)
        {
            uihost.towerSelectRole.cancel.Visibility = Visibility.Collapsed;
            uihost.towerSelectRole.confirmBack = () =>
            {
                foreach (int i in uihost.towerSelectRole.selectedFriends)
                {
                    uihost.towerSelectScene.cannotSelected.Add(RuntimeData.Instance.Team[i].Key);
                }

                this.uihost.Dispatcher.BeginInvoke(() =>
                {
                    Battle battle = TowerManager.getTower(uihost.towerSelectScene.currentTower)[uihost.towerSelectScene.currentIndex];
                    uihost.battleFieldContainer.LoadOLBattle(battle, uihost.towerSelectRole.selectedFriends, myTeamIndex, channel);
                });
            };

            if (firstOLBattle)
            {
                uihost.towerSelectScene.loadHuashan();
                uihost.towerSelectScene.currentTower = "江湖生死战";
                Battle battle = TowerManager.getTower(uihost.towerSelectScene.currentTower)[uihost.towerSelectScene.currentIndex];
                uihost.towerSelectScene.calcFriend(battle, myTeamIndex);
                uihost.towerSelectRole.load(uihost.towerSelectScene.maxFriendNo, null, uihost.towerSelectScene.cannotSelected);
            }
            else
            {
                uihost.towerSelectScene.currentIndex++;
                Battle battle = TowerManager.getTower(uihost.towerSelectScene.currentTower)[uihost.towerSelectScene.currentIndex];
                uihost.towerSelectScene.calcFriend(battle, myTeamIndex);
                uihost.towerSelectRole.load(uihost.towerSelectScene.maxFriendNo, null, uihost.towerSelectScene.cannotSelected);
            }
        }


        private void Restart()
        {
            //RuntimeData.Instance.Round++;
            RuntimeData.Instance.Rank = -1;
            RuntimeData.Instance.NextZhoumuClear();
            RuntimeData.Instance.gameEngine.RollRole();
        }

        private void NextZhoumu()
        {
            RuntimeData.Instance.Rank = -1;
            RuntimeData.Instance.Round++;
            RuntimeData.Instance.NextZhoumuClear();
            RuntimeData.Instance.gameEngine.RollRole();
        }

        /// <summary>
        /// 试炼之地
        /// </summary>
        public void Trial()
        {
            string passedRoles = RuntimeData.Instance.TrialRoles;

            Collection<string> cannotSelectList = new Collection<string>();
            cannotSelectList.Add("主角");
            foreach (var r in passedRoles.Split(new char[] { '#' }))
            {
                cannotSelectList.Add(r);
            }
            uihost.towerSelectRole.confirmBack = () =>
            {
                this.uihost.Dispatcher.BeginInvoke(() =>
                {
                    Battle battle = BattleManager.GetBattle("试炼之地_战斗");
                    uihost.battleFieldContainer.LoadTrail(
                        battle, uihost.towerSelectRole.selectedFriends[0], (result) =>
                    {
                        uihost.battleFieldContainer.field.battleCallback = null;
                        //win
                        if (result == 1)
                        {
                            Role role = RuntimeData.Instance.Team[uihost.towerSelectRole.selectedFriends[0]];
                            RuntimeData.Instance.TrialRoles += "#" + role.Key;
                            RuntimeData.Instance.TrialRoles.Trim(new char[] { '#' });
                            RuntimeData.Instance.AddLog(role.Name + "通过试炼之地");
                            string storyName = "霹雳堂_" + role.Key;
                            Story story = StoryManager.GetStory(storyName);
                            if (story == null)
                            {
                                RuntimeData.Instance.gameEngine.CallScence(
                                    uihost.battleFieldContainer.field,
                                    new NextGameState() { Type = "story", Value = "霹雳堂_胜利" });
                            }
                            else
                            {
                                RuntimeData.Instance.gameEngine.CallScence(
                                    uihost.battleFieldContainer.field,
                                    new NextGameState() { Type = "story", Value = storyName });
                            }
                        }
                        else //lose
                        {
                            RuntimeData.Instance.gameEngine.CallScence(
                                uihost.battleFieldContainer.field, 
                                new NextGameState() { Type = "story", Value = "original_试炼之地.失败" });
                        }
                    });
                });
            };
            uihost.towerSelectRole.load(1, null, cannotSelectList);
        }

        public UIHost uihost;
        private string currentScenario = string.Empty;
    }
}
