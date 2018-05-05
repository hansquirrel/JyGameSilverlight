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
    public class AnimationImage
    {
        public int AnchorX;
        public int AnchorY;
        public int W;
        public int H;
        public string Name = "[img]";
        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
                //Image = Tools.GetImage(_url);
            }
        }
        private string _url;
        public ImageSource Image { get { return Tools.GetImage(_url); } }
    }
    public class AnimationGroup
    {
        public string Name = "";
        public List<AnimationImage> Images = new List<AnimationImage>();
        public XElement ToXml()
        {
            XElement root = new XElement("group");
            root.SetAttributeValue("name", Name);

            for (int i = 0; i < Images.Count; ++i)
            {
                AnimationImage img = Images[i];
                XElement imgNode = new XElement("image");
                imgNode.SetAttributeValue("anchorx", img.AnchorX);
                imgNode.SetAttributeValue("anchory", img.AnchorY);
                //imgNode.SetAttributeValue("name", i);
                root.Add(imgNode);
            }
            return root;
        }
    }

    public class AnimationUnit
    {
        public AnimationUnit()
        {
            Name = "";
        }
        public string Name;
        public List<AnimationGroup> Groups = new List<AnimationGroup>();
        public int PicNumbers
        {
            get
            {
                int rst = 0;
                foreach (var g in Groups)
                {
                    rst += g.Images.Count;
                }
                return rst;
            }
        }
        public XElement ToXml()
        {
            XElement root = new XElement("animation");
            root.SetAttributeValue("name", Name);
            foreach (var g in Groups)
            {
                if (g.Images.Count == 0) continue;
                root.Add(g.ToXml());
            }
            return root;
        }

        static public AnimationUnit Parse(XElement root)
        {
            AnimationUnit rst = new AnimationUnit();
            rst.Name = root.Attribute("name").Value;
            foreach (XElement node in root.Elements("group"))
            {
                string groupName = node.Attribute("name").Value;
                AnimationGroup group = new AnimationGroup();
                group.Name = groupName;
                rst.Groups.Add(group);

                int index = -1;
                foreach (XElement imageNode in node.Elements("image"))
                {
                    index++;
                    AnimationImage img = new AnimationImage();
                    img.AnchorX = int.Parse(imageNode.Attribute("anchorx").Value);
                    img.AnchorY = int.Parse(imageNode.Attribute("anchory").Value);
                    img.W = Tools.GetXmlAttributeInt(imageNode, "w");
                    img.H = Tools.GetXmlAttributeInt(imageNode, "h");
                    img.Name = index.ToString();

                    string dir = "";

                    if (groupName == "skill") 
                    {
                        dir = AnimationManager.SkillAnimationImagePath;
                    }
                    else
                    {
                        dir = AnimationManager.AnimationImagePath;
                    }
                    string imageFile = dir + string.Format("{0}-{1}-{2}.png", rst.Name, group.Name, index);
                    img.Url = imageFile;
                    //img.Image = Tools.GetImage(imageFile);
                    group.Images.Add(img);
                }
            }
            return rst;
        }
    }

    public class AnimationProject
    {
        public List<AnimationUnit> Units = new List<AnimationUnit>();

        public XElement ToXml()
        {
            XElement root = new XElement("animations");
            foreach (var u in Units)
            {
                root.Add(u.ToXml());
            }
            return root;
        }

        static public AnimationProject Load(List<string> files)
        {
            AnimationProject rst = new AnimationProject();
            foreach (var file in files)
            {
                XElement root = Tools.LoadXml("Scripts/" + file);
                foreach (var unit in root.Elements("animation"))
                {
                    rst.Units.Add(AnimationUnit.Parse(unit));
                }
            }
            return rst;
        }
    }

    public class AnimationManager
    {
        public const string AnimationImagePath = "/Resource/animations/";
        public const string SkillAnimationImagePath = "/Resource/skillanimations/";
        public static AnimationProject project = null;
        public static void Init()
        {
            project = AnimationProject.Load(GameProject.GetFiles("animation"));
        }

        static public AnimationGroup GetAnimation(string unit, string group)
        {
            if (project == null)
            {
                MessageBox.Show("error animation project==null");
                return null;
            }
            foreach (var u in project.Units)
            {
                if (u.Name == unit)
                {
                    foreach (var g in u.Groups)
                    {
                        if (g.Name == group)
                        {
                            return g;
                        }
                    }
                }
            }
            if (group == "skill")
            {
                MessageBox.Show(string.Format("error, null animation:{0} {1}", unit, group));
            }
            return null;
        }
    }
}
