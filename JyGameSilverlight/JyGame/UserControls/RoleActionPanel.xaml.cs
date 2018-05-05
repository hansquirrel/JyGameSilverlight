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

namespace JyGame
{
	public partial class RoleActionPanel : UserControl
	{
        public delegate void OnSelectRoleDelegate(RoleActionType type);

        public OnSelectRoleDelegate Callback;

        private Image[] imgs = null;
		public RoleActionPanel()
		{
			// 为初始化变量所必需
			InitializeComponent();
            imgs = new Image[] { Attack, Items, Rest, RoleStatus };
            //foreach (var i in imgs)
            //{
            //    i.MouseEnter += (s, e) =>
            //    {
            //        Image img = s as Image;
            //        Canvas.SetTop(img, Canvas.GetTop(img) - 5);
            //    };
            //    i.MouseLeave += (s, e) =>
            //    {
            //        Image img = s as Image;
            //        Canvas.SetTop(img, Canvas.GetTop(img) + 5);
            //    };
            //}

            Attack.MouseLeftButtonUp += Attack_Click;
            Items.MouseLeftButtonUp += Items_Click;
            Rest.MouseLeftButtonUp += Rest_Click;
            RoleStatus.MouseLeftButtonUp += RoleStatus_Click;

            //attackAnim.Completed += (s, e) =>
            //{
            //    foreach (var i in imgs)
            //    {
            //        i.IsHitTestVisible = true;
            //    }
            //};
		}

		private void Attack_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            Callback(RoleActionType.Attack);
		}

		private void Items_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            Callback(RoleActionType.Items);
		}

		private void Rest_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            Callback(RoleActionType.Rest);
		}

		private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            Callback(RoleActionType.Cancel);
		}

        private void RoleStatus_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Callback(RoleActionType.RoleStatus);
        }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
            //foreach (var i in imgs)
            //{
            //    i.IsHitTestVisible = false;
            //}
            this.attackAnim.Begin();
        }
	}
}