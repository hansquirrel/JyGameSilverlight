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
using System.Xml.Linq;
using System.Collections.Generic;

namespace JyGame.GameData
{
    public class ShopItem
    {
        public string Name;
        public int Limit = -1;
        public int YuanBao = -1;
        public int Price = -1;

        public Item Item { get { return ItemManager.GetItem(Name); } }

        public static ShopItem Parse(XElement node)
        {
            ShopItem item = new ShopItem();
            item.Name = Tools.GetXmlAttribute(node, "name");
            if(item.Item == null)
            {
                MessageBox.Show("错误，调用了未定义的物品：" + item.Name);
            }

            if (node.Attribute("limit") != null)
                item.Limit = Tools.GetXmlAttributeInt(node, "limit");
            if (node.Attribute("yuanbao") != null)
                item.YuanBao = Tools.GetXmlAttributeInt(node, "yuanbao");
            if (node.Attribute("price") != null)
            {
                item.Price = Tools.GetXmlAttributeInt(node, "price");
            }
            else
            {
                item.Price = item.Item.price;
            }
            return item;
        }
    }

    public class Shop
    {
        public string PicUrl;
        public ImageSource Pic { get { return Tools.GetImage(PicUrl); } }
        public string Music;

        public string Name;
        public string Key;

        //public Dictionary<Item, int> Items = new Dictionary<Item, int>();
        //public Dictionary<Item, int> YuanbaoItems = new Dictionary<Item, int>();
        //public List<string> OnceItems = new List<string>();
        public List<ShopItem> Sales = new List<ShopItem>();

        public Dictionary<ShopItem,int> GetAvaliableSales()
        {
            Dictionary<ShopItem, int> rst = new Dictionary<ShopItem, int>();
            foreach(var sale in Sales)
            {
                string itemKey = "shopBuyKey_" + Key + sale.Name;
                if (sale.Limit != -1) //限制购买物品
                {
                    int buyCount = 0;
                    if (RuntimeData.Instance.KeyValues.ContainsKey(itemKey))
                    {
                        buyCount = int.Parse(RuntimeData.Instance.KeyValues[itemKey]);
                    }
                    if (buyCount < sale.Limit)
                    {
                        rst.Add(sale, sale.Limit - buyCount);
                    }
                }
                else
                {
                    rst.Add(sale, -1);
                }
            }
            return rst;
        }

        //public Dictionary<Item, int> GetAvaliableItems()
        //{
        //    Dictionary<Item, int> rst = new Dictionary<Item, int>();

        //    foreach (var item in Items.Keys)
        //    {
        //        string itemKey = "shopBuyKey_" + Name + item.Name;
        //        if (RuntimeData.Instance.KeyValues.ContainsKey(itemKey) && OnceItems.Contains(item.Name))
        //        {
        //            continue;
        //        }
        //        rst.Add(item, Items[item]);
        //    }
        //    return rst;
        //}

        //public Dictionary<Item, int> GetAvaliableYuanbaoItems()
        //{
        //    Dictionary<Item, int> rst = new Dictionary<Item, int>();

        //    foreach (var item in YuanbaoItems.Keys)
        //    {
        //        string itemKey = "shopBuyKey_" + Name + item.Name;
        //        if (RuntimeData.Instance.KeyValues.ContainsKey(itemKey) && OnceItems.Contains(item.Name))
        //        {
        //            continue;
        //        }
        //        rst.Add(item, YuanbaoItems[item]);
        //    }
        //    return rst;
        //}

        public void BuyItem(string itemName, int count = 1)
        {
            string itemKey = "shopBuyKey_" + Key + itemName;
            if (RuntimeData.Instance.KeyValues.ContainsKey(itemKey))
            {
                int buyCount = int.Parse(RuntimeData.Instance.KeyValues[itemKey]);
                buyCount += count;
                RuntimeData.Instance.KeyValues[itemKey] = buyCount.ToString();
            }
            else
            {
                RuntimeData.Instance.KeyValues[itemKey] = count.ToString();
            }
        }
    }

    public class ShopManager
    {

        static private List<Shop> Shops = new List<Shop>();
        static public void Init()
        {
            Shops.Clear();
            foreach (var shopXmlFile in GameProject.GetFiles("shop"))
            {
                string path = "Scripts/" + shopXmlFile;
                XElement xmlRoot = Tools.LoadXml(path);

                foreach (var shopXml in xmlRoot.Elements("shop"))
                {
                    Shop shop = new Shop();
                    shop.Name = Tools.GetXmlAttribute(shopXml, "name");
                    shop.PicUrl = ResourceManager.Get(Tools.GetXmlAttribute(shopXml, "pic"));
                    shop.Music = Tools.GetXmlAttribute(shopXml, "music");
                    if(shopXml.Attribute("key") != null)
                    {
                        shop.Key = Tools.GetXmlAttribute(shopXml, "key");
                    }
                    else //如果没有定义key,则默认把name作为key（key是作为是否购买限制物品的标识）
                    {
                        shop.Key = shop.Name;
                    }
                    foreach (var sale in shopXml.Elements("sale"))
                    {
                        shop.Sales.Add(ShopItem.Parse(sale));
                    }
                    Shops.Add(shop);
                }
            }
        }

        static public Shop GetShop(string name)
        {
            foreach (var s in Shops)
            {
                if (s.Name.Equals(name)) return s;
            }
            MessageBox.Show("错误，调用了未定义的商店" + name);
            return null;
        }


    }

    public class SellShop
    {
        public string PicUrl;
        public ImageSource Pic { get { return Tools.GetImage(PicUrl); } }
        public string Music;

        public string Name;

        public Dictionary<Item, int> GetAvaliableItems()
        {
            Dictionary<Item, int> rst = new Dictionary<Item, int>();

            foreach (var item in RuntimeData.Instance.Items)
            {
                if((item.price * 0.5f) > 0)
                    rst.Add(item,  (int)(item.price * 0.5f) );
            }
            return rst;
        }
    }

    public class SellShopManager
    {
        static private List<SellShop> Shops = new List<SellShop>();
        static public void Init()
        {
            Shops.Clear();
            foreach (var shopXmlFile in GameProject.GetFiles("shop"))
            {
                string path = "Scripts/" + shopXmlFile;
                XElement xmlRoot = Tools.LoadXml(path);

                foreach (var shopXml in xmlRoot.Elements("sellshop"))
                {
                    SellShop shop = new SellShop();
                    shop.Name = Tools.GetXmlAttribute(shopXml, "name");
                    shop.PicUrl = ResourceManager.Get(Tools.GetXmlAttribute(shopXml, "pic"));
                    shop.Music = Tools.GetXmlAttribute(shopXml, "music");
                    Shops.Add(shop);
                }
            }
        }

        static public SellShop GetSellShop(string name)
        {
            foreach (var s in Shops)
            {
                if (s.Name.Equals(name)) return s;
            }
            MessageBox.Show("错误，调用了未定义的当铺" + name);
            return null;
        }


    }

}
