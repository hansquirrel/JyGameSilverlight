using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using JyGame.UserControls;
using System.Collections.Generic;
using JyGame.GameData;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Linq;

namespace JyGame.UserControls
{
    enum ShopType
    {
        SHOP,
        SELL,
        XIANGZI,
        MYITEMS
    }

	public partial class StorePanel : UserControl
	{
        public delegate void OnSelectItemDelegate(Item item);

        public CommonSettings.VoidCallBack Callback = null;

		public StorePanel()
		{
			InitializeComponent();
		}

        private ShopType Type = ShopType.SHOP;
        private Shop shop;
        private SellShop sellShop;

        #region 商店
        // <summary>
        // 显示
        // </summary>
        // 只显示价格为正的产品（价格为零的是非卖品）
        public void Show(Shop shop)
        {
            AllSellButton.Visibility = System.Windows.Visibility.Collapsed;
            Type = ShopType.SHOP;
            this.shop = shop;
            UIHost uiHost = RuntimeData.Instance.gameEngine.uihost;
            uiHost.RightClickCallback = null;
            uiHost.shopPanel.SuggestText.Text = "右键快捷购买";
            uiHost.shopPanel.myItemsPanel.Visibility = Visibility.Collapsed;
            Dictionary<ShopItem, int> sales = shop.GetAvaliableSales();
            RootPanel.Children.Clear();
            uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);//刷新物品栏
            this.money.Text = RuntimeData.Instance.Money.ToString();
            this.yuanbao.Text = RuntimeData.Instance.YuanBao.ToString();

            foreach (var sale in sales.Keys)
            {
                Item item = sale.Item;
                int maxLimit = sales[sale];
                if (ItemTypeCombo.SelectedItem.ToString() == "武器" && (item.Type != (int)ItemType.Weapon)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "消耗品" && (item.Type != (int)ItemType.Costa)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "防具" && (item.Type != (int)ItemType.Armor)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "饰品" && (item.Type != (int)ItemType.Accessories)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "经书" && (item.Type != (int)ItemType.Book)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "任务物品" && (item.Type != (int)ItemType.Mission)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "技能书" && (item.Type != (int)ItemType.TalentBook) && item.Type != (int)ItemType.Upgrade) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "残章" && (item.Type != (int)ItemType.Canzhang)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "特殊物品" && (item.Type != (int)ItemType.Special)) continue;

                ItemUnit c = new ItemUnit();
                c.image.Source = item.Pic;
                c.ItemName = item.Name;
                c.Margin = new Thickness(2, 2, 2, 5);
                if (maxLimit != -1)
                {
                    c.count.Text = maxLimit.ToString();
                }
                else
                {
                    c.count.Text = "";
                }
                string info = item.ToString();
                int price = sale.Price;
                ShopItem tmp = sale;
                c.MouseRightButtonUp += (s, e) =>
                    {
                        if (uiHost.selectPanel.Visibility == System.Windows.Visibility.Visible) return;
                        if (tmp.YuanBao == -1)
                        {
                            if (RuntimeData.Instance.Money >= price)
                            {
                                RuntimeData.Instance.Money -= price;
                                RuntimeData.Instance.Items.Add(tmp.Item.Clone());
                                AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
                                this.money.Text = RuntimeData.Instance.Money.ToString();
                                shop.BuyItem(tmp.Name);
                                this.Show(shop);
                            }
                            else
                            {
                                uiHost.dialogPanel.ShowDialog("汉家松鼠", "你的金钱不足!", null);
                            }
                        }
                        else
                        {
                            if (RuntimeData.Instance.YuanBao >= tmp.YuanBao)
                            {
                                RuntimeData.Instance.YuanBao -= tmp.YuanBao;
                                RuntimeData.Instance.Items.Add(tmp.Item.Clone());
                                AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
                                shop.BuyItem(tmp.Name);
                                this.Show(shop);
                            }
                            else
                            {
                                uiHost.dialogPanel.ShowDialog("汉家松鼠", "你的元宝不足!", null);
                            }
                        }
                        e.Handled = true;
                    };
                c.MouseLeftButtonUp += (s, e) =>
                    {
                        BuyItemWindow win = new BuyItemWindow();
                        win.Bind(tmp.Item, "购买物品", (number) =>
                        {
                            if (tmp.YuanBao == -1)
                            {
                                if (RuntimeData.Instance.Money >= price * number)
                                {
                                    RuntimeData.Instance.Money -= price * number;
                                    for (int i = 0; i < number; ++i)
                                        RuntimeData.Instance.Items.Add(tmp.Item.Clone());
                                    AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
                                    shop.BuyItem(tmp.Name, number);
                                    this.Show(shop);
                                }
                                else
                                {
                                    uiHost.dialogPanel.ShowDialog("汉家松鼠", "你的金钱不足!", null);
                                }
                            }
                            else
                            {
                                if (RuntimeData.Instance.YuanBao >= tmp.YuanBao * number)
                                {
                                    RuntimeData.Instance.YuanBao -= tmp.YuanBao * number;
                                    for (int i = 0; i < number; ++i)
                                        RuntimeData.Instance.Items.Add(tmp.Item.Clone());
                                    AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
                                    shop.BuyItem(tmp.Name, number);
                                    this.Show(shop);
                                }
                                else
                                {
                                    uiHost.dialogPanel.ShowDialog("汉家松鼠", "你的元宝不足!", null);
                                }
                            }
                        }, maxLimit);
                        win.Show();
                    };

                
                
                RichTextBox rtb = item.GenerateTooltip();
                (rtb.Blocks[0] as Paragraph).Inlines.Add(new LineBreak());
                (rtb.Blocks[0] as Paragraph).Inlines.Add(new LineBreak());
                if(sale.YuanBao != -1)
                {
                    (rtb.Blocks[0] as Paragraph).Inlines.Add(new Run() { Text = "价格:" + tmp.YuanBao.ToString() + "个元宝", FontSize = 12, FontWeight = FontWeights.Bold });
                }
                else
                {
                    (rtb.Blocks[0] as Paragraph).Inlines.Add(new Run() { Text = "价格:" + price.ToString() + "两白银", FontSize = 12, FontWeight = FontWeights.Bold });
                }
                
                ToolTipService.SetToolTip(c, rtb);
                RootPanel.Children.Add(c);
            }

            this.Visibility = System.Windows.Visibility.Visible;
        }
        #endregion

        #region 当铺
        // <summary>
        // 显示
        // </summary>
        // 只显示价格为正的产品（价格为零的是非卖品）
        public void Show(SellShop shop)
        {
            AllSellButton.Visibility = System.Windows.Visibility.Visible;
            Type = ShopType.SELL;
            sellShop = shop;
            UIHost uiHost = RuntimeData.Instance.gameEngine.uihost;
            uiHost.RightClickCallback = null;
            uiHost.shopPanel.myItemsPanel.Visibility = Visibility.Collapsed;
            uiHost.shopPanel.SuggestText.Text = "右键快捷出售";
            Dictionary<Item, int> items = shop.GetAvaliableItems();
            int count = 0;
            RootPanel.Children.Clear();
            this.money.Text = RuntimeData.Instance.Money.ToString();
            this.yuanbao.Text = RuntimeData.Instance.YuanBao.ToString();

            List<string> visitedItem = new List<string>();
            foreach (var item in items.Keys)
            {
                if (ItemTypeCombo.SelectedItem.ToString() == "武器" && (item.Type != (int)ItemType.Weapon)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "消耗品" && (item.Type != (int)ItemType.Costa)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "防具" && (item.Type != (int)ItemType.Armor)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "饰品" && (item.Type != (int)ItemType.Accessories)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "经书" && (item.Type != (int)ItemType.Book)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "任务物品" && (item.Type != (int)ItemType.Mission)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "技能书" && (item.Type != (int)ItemType.TalentBook) && item.Type != (int)ItemType.Upgrade) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "残章" && (item.Type != (int)ItemType.Canzhang)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "特殊物品" && (item.Type != (int)ItemType.Special)) continue;
                int itemCount = 1;
                //如果不是装备，则层叠
                if (!(item.Type == (int)ItemType.Armor || item.Type == (int)ItemType.Accessories || item.Type == (int)ItemType.Weapon))
                {
                    if (visitedItem.Contains(item.Name)) continue;
                    itemCount = items.Keys.Count(p => p.Name == item.Name);
                    visitedItem.Add(item.Name);
                }

                if (count > 5) count = 0;

                ItemUnit c = new ItemUnit();
                c.BindItem(item);
                c.ItemCount = itemCount;
                c.Margin = new Thickness(2, 2, 2, 5);
                string info = item.ToString();
                int price = items[item];
                Item tmp = item;
                c.MouseRightButtonUp += (s, e) =>
                    {
                        if (uiHost.selectPanel.Visibility == System.Windows.Visibility.Visible) return;
                        RuntimeData.Instance.Money += price;
                        RuntimeData.Instance.Items.Remove(tmp);
                        AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
                        this.money.Text = RuntimeData.Instance.Money.ToString();
                        //uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);//刷新物品栏
                        this.Show(shop);
                        e.Handled = true;
                    };
                c.MouseLeftButtonUp += (s, e) =>
                {
                    //uiHost.selectPanel.CallBack = () =>
                    //{
                    //    if (uiHost.selectPanel.currentSelection == "yes")
                    //    {
                    //        RuntimeData.Instance.Money += price;
                    //        RuntimeData.Instance.Items.Remove(tmp);
                    //        AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
                    //        this.money.Text = RuntimeData.Instance.Money.ToString();
                    //        //uiHost.itemSelectPanel.Show(RuntimeData.Instance.Items);//刷新物品栏
                    //        this.Show(shop);
                    //    }
                    //};
                    //uiHost.selectPanel.title.Text = "确定出售？";
                    //uiHost.selectPanel.yes.Content = "出售";
                    //uiHost.selectPanel.no.Content = "取消";
                    //uiHost.selectPanel.ShowSelection();
                    BuyItemWindow win = new BuyItemWindow();
                    win.Bind(tmp, "出售物品", (number) =>
                    {
                        RuntimeData.Instance.Money += price * number;
                        RuntimeData.Instance.RemoveItem(tmp.Name, number);
                        AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
                        this.Show(shop);
                    }, itemCount);
                    win.Show();
                    e.Handled = true;
                };
                //ToolTipService.SetToolTip(c,info + "\n价格:" + price.ToString() + "两白银");
                RichTextBox rtb = item.GenerateTooltip();
                (rtb.Blocks[0] as Paragraph).Inlines.Add(new LineBreak());
                (rtb.Blocks[0] as Paragraph).Inlines.Add(new LineBreak());
                (rtb.Blocks[0] as Paragraph).Inlines.Add(new Run() { Text = "价格:" + price.ToString() + "两白银", FontSize = 12, FontWeight = FontWeights.Bold });
                ToolTipService.SetToolTip(c, rtb);
                
                RootPanel.Children.Add(c);
                count++;
            }

            this.Visibility = System.Windows.Visibility.Visible;
        }
        #endregion

        #region 箱子
        /// <summary>
        /// 显示箱子
        /// </summary>
        public void ShowXiangzi()
        {
            AllSellButton.Visibility = System.Windows.Visibility.Collapsed ;
            Type = ShopType.XIANGZI;
            UIHost uiHost = RuntimeData.Instance.gameEngine.uihost;
            uiHost.RightClickCallback = null;
            uiHost.shopPanel.SuggestText.Text = "单击鼠标左键取出或存入装备";
            uiHost.shopPanel.myItemsPanel.Visibility = Visibility.Visible;
            uiHost.shopPanel.myItemsPanel.Type = ShopType.MYITEMS;
            List<Item> xiangziItems = XiangziManager.Items;
            RootPanel.Children.Clear();

            this.money.Text = "";
            StackPanel currentPanel = null;
            int count = 0;
            List<string> visitedItem = new List<string>();
            foreach (var item in xiangziItems)
            {
                if (ItemTypeCombo.SelectedItem.ToString() == "武器" && (item.Type != (int)ItemType.Weapon)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "消耗品" && (item.Type != (int)ItemType.Costa)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "防具" && (item.Type != (int)ItemType.Armor)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "饰品" && (item.Type != (int)ItemType.Accessories)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "经书" && (item.Type != (int)ItemType.Book)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "任务物品" && (item.Type != (int)ItemType.Mission)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "技能书" && (item.Type != (int)ItemType.TalentBook) && item.Type != (int)ItemType.Upgrade) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "残章" && (item.Type != (int)ItemType.Canzhang)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "特殊物品" && (item.Type != (int)ItemType.Special)) continue;
                if (count > 5) count = 0;
                if (count == 0)
                {
                    currentPanel = new StackPanel();
                    currentPanel.Orientation = Orientation.Horizontal;
                    currentPanel.Height = 58;
                    currentPanel.Margin = new Thickness(2, 2, 2, 0);
                    RootPanel.Children.Add(currentPanel);
                }
                int itemCount = 1;
                //如果不是装备，则层叠
                if (!(item.Type == (int)ItemType.Armor || item.Type == (int)ItemType.Accessories || item.Type == (int)ItemType.Weapon))
                {
                    if (visitedItem.Contains(item.Name)) continue;
                    itemCount = xiangziItems.Count(p => p.Name == item.Name);
                    visitedItem.Add(item.Name);
                }

                ItemUnit c = new ItemUnit();
                c.BindItem(item);
                c.Margin = new Thickness(2, 2, 2, 5);
                c.ItemCount = itemCount;
                Item tmp = item;
                c.MouseLeftButtonUp += (s, e) =>
                {
                    lock (this)
                    {
                        if (xiangziItems.Remove(item))
                        {
                            RuntimeData.Instance.Items.Add(item);
                        }
                        XiangziManager.Items = xiangziItems;
                    }
                    this.ShowXiangzi();
                };
                ToolTipService.SetToolTip(c, item.GenerateTooltip());
                currentPanel.Children.Add(c);
                count++;
            }

            RefreshMyItems();

            this.Visibility = System.Windows.Visibility.Visible;
        }
        #endregion

        private void RefreshMyItems()
        {
            StackPanel currentPanel = null;
            UIHost uiHost = RuntimeData.Instance.gameEngine.uihost;
            List<Item> xiangziItems = XiangziManager.Items;
            int count = 0;
            uiHost.shopPanel.myItemsPanel.RootPanel.Children.Clear();
            List<string> visitedItem = new List<string>();
            foreach (var item in RuntimeData.Instance.Items)
            {
                if (ItemTypeCombo.SelectedItem.ToString() == "武器" && (item.Type != (int)ItemType.Weapon)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "消耗品" && (item.Type != (int)ItemType.Costa)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "防具" && (item.Type != (int)ItemType.Armor)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "饰品" && (item.Type != (int)ItemType.Accessories)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "经书" && (item.Type != (int)ItemType.Book)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "任务物品" && (item.Type != (int)ItemType.Mission)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "技能书" && (item.Type != (int)ItemType.TalentBook) && item.Type != (int)ItemType.Upgrade) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "残章" && (item.Type != (int)ItemType.Canzhang)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "特殊物品" && (item.Type != (int)ItemType.Special)) continue;
                if (count > 5) count = 0;

                //只能存装备和残章
                if (item.Type != (int)ItemType.Armor
                    && item.Type != (int)ItemType.Canzhang
                    && item.Type != (int)ItemType.Weapon
                    && item.Type != (int)ItemType.Accessories)
                    continue;
                if (count == 0)
                {
                    currentPanel = new StackPanel();
                    currentPanel.Orientation = Orientation.Horizontal;
                    currentPanel.Height = 58;
                    currentPanel.Margin = new Thickness(2, 2, 2, 0);
                    uiHost.shopPanel.myItemsPanel.RootPanel.Children.Add(currentPanel);
                }
                int itemCount = 1;
                //如果不是装备，则层叠
                if (!(item.Type == (int)ItemType.Armor || item.Type == (int)ItemType.Accessories || item.Type == (int)ItemType.Weapon))
                {
                    if (visitedItem.Contains(item.Name)) continue;
                    itemCount = RuntimeData.Instance.Items.Count(p => p.Name == item.Name);
                    visitedItem.Add(item.Name);
                }
                ItemUnit c = new ItemUnit();
                c.BindItem(item);
                c.Margin = new Thickness(2, 2, 2, 5);
                c.ItemCount = itemCount;
                Item tmp = item;
                c.MouseLeftButtonUp += (s, e) =>
                {
                    lock (this)
                    {
                        int maxItemCount = 4 + RuntimeData.Instance.Round * 3;
                        if (xiangziItems.Count + 1 > maxItemCount)
                        {
                            MessageBox.Show("你这个周目最多存放" + maxItemCount + "个装备");
                            uiHost.shopPanel.storePanel.ShowXiangzi();
                            //this.ShowXiangzi();
                            return;
                        }
                        xiangziItems.Add(item);
                        XiangziManager.Items = xiangziItems;
                        RuntimeData.Instance.Items.Remove(item);
                        //this.ShowXiangzi();
                        uiHost.shopPanel.storePanel.ShowXiangzi();
                        e.Handled = true;
                    }
                };
                ToolTipService.SetToolTip(c, item.GenerateTooltip());
                currentPanel.Children.Add(c);
                count++;
            }
        }

        private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            if (Callback != null) Callback();
        }

        private void money_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
        }

        private void money_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
        }

        private void Refresh()
        {
            switch (Type)
            {
                case ShopType.SHOP:
                    this.Show(shop);
                    break;
                case ShopType.SELL:
                    this.Show(sellShop);
                    break;
                case ShopType.XIANGZI:
                    this.ShowXiangzi();
                    break;
                case ShopType.MYITEMS:
                    this.RefreshMyItems();
                    break;
                default:
                    break;
            }
        }

        private void ItemTypeCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                this.Refresh();

                if(Type == ShopType.XIANGZI)
                {
                    RuntimeData.Instance.gameEngine.uihost.shopPanel.myItemsPanel.ItemTypeCombo.SelectedIndex = ItemTypeCombo.SelectedIndex;
                }else if(Type == ShopType.MYITEMS)
                {
                    RuntimeData.Instance.gameEngine.uihost.shopPanel.storePanel.ItemTypeCombo.SelectedIndex = ItemTypeCombo.SelectedIndex;
                }
            }
            catch
            {

            }
        }

        private void ItemTypeCombo_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ItemTypeCombo.Items.Add("全部");
            ItemTypeCombo.Items.Add("消耗品");
            ItemTypeCombo.Items.Add("武器");
            ItemTypeCombo.Items.Add("防具");
            ItemTypeCombo.Items.Add("饰品");
            ItemTypeCombo.Items.Add("经书");
            ItemTypeCombo.Items.Add("任务物品");
            ItemTypeCombo.Items.Add("技能书");
            ItemTypeCombo.Items.Add("特殊物品");
            ItemTypeCombo.Items.Add("残章");
            ItemTypeCombo.SelectedIndex = 0;
        }

        private void AllSellButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	if(MessageBox.Show("确认全部卖出吗？","确认卖出", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                List<Item> tobeRemoved = new List<Item>();
                foreach(var item in RuntimeData.Instance.Items)
                {
                    if (ItemTypeCombo.SelectedItem.ToString() == "武器" && (item.Type != (int)ItemType.Weapon)) continue;
                    if (ItemTypeCombo.SelectedItem.ToString() == "消耗品" && (item.Type != (int)ItemType.Costa)) continue;
                    if (ItemTypeCombo.SelectedItem.ToString() == "防具" && (item.Type != (int)ItemType.Armor)) continue;
                    if (ItemTypeCombo.SelectedItem.ToString() == "饰品" && (item.Type != (int)ItemType.Accessories)) continue;
                    if (ItemTypeCombo.SelectedItem.ToString() == "经书" && (item.Type != (int)ItemType.Book)) continue;
                    if (ItemTypeCombo.SelectedItem.ToString() == "任务物品" && (item.Type != (int)ItemType.Mission)) continue;
                    if (ItemTypeCombo.SelectedItem.ToString() == "技能书" && (item.Type != (int)ItemType.TalentBook) && item.Type != (int)ItemType.Upgrade) continue;
                    if (ItemTypeCombo.SelectedItem.ToString() == "残章" && (item.Type != (int)ItemType.Canzhang)) continue;
                    if (ItemTypeCombo.SelectedItem.ToString() == "特殊物品" && (item.Type != (int)ItemType.Special)) continue;

                    if(item.price > 0)
                    {
                        RuntimeData.Instance.Money += (int)(item.price / 2);
                        tobeRemoved.Add(item);
                    }
                }
                AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
                foreach (var item in tobeRemoved) { RuntimeData.Instance.Items.Remove(item); }
                this.Refresh();
            }
        }
	}
}