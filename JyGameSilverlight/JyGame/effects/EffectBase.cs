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
using System.Windows.Media.Effects;

namespace Effects {

    /// <summary>
    /// HLSL特效基类
    /// </summary>
    public abstract class EffectBase : ShaderEffect {

        /// <summary>
        /// 获取渲染特效的文件地址
        /// </summary>
        protected Uri GetShaderUri(string shaderName) {
            return new Uri(string.Format("/JyGame;component/effects/Source/{0}.ps", shaderName), UriKind.Relative);
        }

    }
}
