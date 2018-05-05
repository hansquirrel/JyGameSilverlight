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
using JyGame.Interface;

namespace JyGame
{
    public partial class ShopPanel : UserControl, IScence
	{
        public UIHost uiHost = null;
        public ShopPanel()
		{
			// 为初始化变量所必需
			InitializeComponent();
		}

        public void Show(Shop shop)
        {
            this.background.Source = shop.Pic;
            AudioManager.PlayMusic(ResourceManager.Get(shop.Music));
            storePanel.Show(shop);
            storePanel.Callback = () =>
                {
                    RuntimeData.Instance.gameEngine.CallScence(this, new NextGameState() { 
                        Type = "map",
                        Value = RuntimeData.Instance.CurrentBigMap
                    });
                };
            this.Visibility = System.Windows.Visibility.Visible;
        }

        public void Show(SellShop shop)
        {
            this.background.Source = shop.Pic;
            AudioManager.PlayMusic(ResourceManager.Get(shop.Music));
            storePanel.Show(shop);
            storePanel.Callback = () =>
            {
                RuntimeData.Instance.gameEngine.CallScence(this, new NextGameState()
                {
                    Type = "map",
                    Value = RuntimeData.Instance.CurrentBigMap
                });
            };
            this.Visibility = System.Windows.Visibility.Visible;
        }

        public void ShowXiangzi()
        {
            this.background.Source = ResourceManager.GetImage("地图.松鼠旅馆");
            storePanel.Callback = () =>
            {
                RuntimeData.Instance.gameEngine.CallScence(this, new NextGameState()
                {
                    Type = "map",
                    Value = RuntimeData.Instance.CurrentBigMap
                });
            };
            storePanel.ShowXiangzi();
            this.Visibility = System.Windows.Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = System.Windows.Visibility.Collapsed;
        }
        public void Load(string scenceName)
        {

        }

	}
}