using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using JyGame.UserControls;
using System.Collections.Generic;
using JyGame.GameData;
using System.Collections.ObjectModel;

namespace JyGame
{
	public partial class ItemSelectPanel : UserControl
	{
        public delegate void OnSelectItemDelegate(Item item);

        public OnSelectItemDelegate Callback
        {
            get
            {
                lock (this) { return _callback; }
            }
            set
            {
                lock (this) { _callback = value; }
            }
        }
        private OnSelectItemDelegate _callback;

		public ItemSelectPanel()
		{
			InitializeComponent();
		}

        public void HideUI()
        {
            if (!_hideUI)
            {
                //this.dragElement.Detach();
            }
            Cancel.Visibility = System.Windows.Visibility.Collapsed;
            _hideUI = true;
        }
        private bool _hideUI = false;

        public int CurrentPage = 1;

        /// <summary>
        /// 显示
        /// </summary>
        /// <param name="items"></param>
        /// <param name="onlyCost">只显示消耗品</param>
        public void Show(List<Item> items,bool onlyCost = false,int money=-1,bool xiangzi = false)
        {
            //RuntimeData.Instance.ArrangeItems();

            if (onlyCost)
                this.ArrangeButton.Visibility = Visibility.Collapsed;
            else
                this.ArrangeButton.Visibility = Visibility.Visible;
            this.Visibility = System.Windows.Visibility.Visible;

            RootPanel.Children.Clear();

            this.ItemInfo.Text = "";
            if (money != -1)
            {
                this.money.Text = money.ToString() ;
            }
            else
                this.money.Text = RuntimeData.Instance.Money.ToString();

            StackPanel currentPanel = null;

            List<string> visitedItem = new List<string>();

            List<Item> itemPages = new List<Item>();
            List<int> itemCounts = new List<int>();

            foreach (var item in items)
            {
                int itemCount = 1;

                if (ItemTypeCombo.SelectedItem.ToString() == "武器" && (item.Type != (int)ItemType.Weapon)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "消耗品" && (item.Type != (int)ItemType.Costa)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "防具" && (item.Type != (int)ItemType.Armor)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "饰品" && (item.Type != (int)ItemType.Accessories)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "经书" && (item.Type != (int)ItemType.Book)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "任务物品" && (item.Type != (int)ItemType.Mission)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "技能书" && (item.Type != (int)ItemType.TalentBook) && item.Type != (int)ItemType.Upgrade) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "残章" && (item.Type != (int)ItemType.Canzhang)) continue;
                if (ItemTypeCombo.SelectedItem.ToString() == "特殊物品" && (item.Type != (int)ItemType.Special)) continue;
                

                //如果不是装备，则层叠
                if (!(item.Type == (int)ItemType.Armor || item.Type == (int)ItemType.Accessories || item.Type == (int)ItemType.Weapon))
                {
                    if (visitedItem.Contains(item.Name)) continue;
                    itemCount = items.Count(p => p.Name == item.Name);
                    visitedItem.Add(item.Name);
                }

                if (onlyCost && item.Type != 0) continue;
                itemPages.Add(item);
                itemCounts.Add(itemCount);
            }

            int count = 0;
            int MAX_ITEM_IN_PAGE = 36;
            int pageCount = (int)(Math.Ceiling((double)itemPages.Count / MAX_ITEM_IN_PAGE));
            PageLabel.Text = string.Format("{0}/{1}", CurrentPage, pageCount);
            _pageCount = pageCount;
            for (int i = 0; i < MAX_ITEM_IN_PAGE; ++i )
            {
                if (count > 5) count = 0;
                int index = (CurrentPage - 1) * MAX_ITEM_IN_PAGE + i;
                if(index < itemPages.Count)
                {
                    if (count == 0)
                    {
                        currentPanel = new StackPanel();
                        currentPanel.Orientation = Orientation.Horizontal;
                        currentPanel.Height = 58;
                        currentPanel.Margin = new Thickness(2, 2, 2, 0);
                        RootPanel.Children.Add(currentPanel);
                    }
                    Item item = itemPages[index];
                    ItemUnit itemUnit = new ItemUnit();

                    itemUnit.ItemCount = itemCounts[index];
                    itemUnit.BindItem(item);
                    itemUnit.Margin = new Thickness(2, 2, 2, 5);
                    itemUnit.MouseLeftButtonUp += (s, e) =>
                    {
                        this.Visibility = System.Windows.Visibility.Collapsed;
                        this.Callback(item);
                    };
                    ToolTipService.SetToolTip(itemUnit, item.GenerateTooltip());
                    currentPanel.Children.Add(itemUnit);
                    count++;
                }
            }
            UpdatePageButtons();
        }

        private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Callback(null);
        }

        //整理物品
        private void Arrange_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RuntimeData.Instance.ArrangeItems();
            AudioManager.PlayEffect(ResourceManager.Get("音效.装备"));
            this.Show(RuntimeData.Instance.Items);
        }

        private void money_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.ItemInfo.Text = "白花花的银子，有钱能使鬼推磨";
        }

        private void money_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.ItemInfo.Text = "";
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
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
            this.Visibility = Visibility.Collapsed;
        }

        private void ItemTypeCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CurrentPage = 1;
            this.Show(RuntimeData.Instance.Items);
        }

        private void PrevPage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                this.Show(RuntimeData.Instance.Items);
            }
        }

        private void NextPage_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (CurrentPage < _pageCount)
            {
                CurrentPage++;
                this.Show(RuntimeData.Instance.Items);
            }
        }

        private void UpdatePageButtons()
        {
            PrevPage.IsEnabled = !(CurrentPage == 1);
            NextPage.IsEnabled = !(CurrentPage == _pageCount);
        }

        private int _pageCount = -1;
	}
}