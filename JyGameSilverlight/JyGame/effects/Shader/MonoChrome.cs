using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 单色
    /// </summary>
    public class MonoChrome : EffectBase {
        
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(MonoChrome), 0);
        public static readonly DependencyProperty FilterColorProperty = DependencyProperty.Register("FilterColor", typeof(Color), typeof(MonoChrome), new PropertyMetadata(Color.FromArgb(255, 255, 255, 0), PixelShaderConstantCallback(0)));
        
        public MonoChrome() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("MonoChrome") };
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(FilterColorProperty);
        }
        
        public Brush Input {
            get {  return ((Brush)(this.GetValue(InputProperty)));  }
            set {  this.SetValue(InputProperty, value); }
        }
        
        /// <summary>
        /// 单色颜色
        /// </summary>
        public Color FilterColor {
            get { return ((Color)(this.GetValue(FilterColorProperty))); }
            set { this.SetValue(FilterColorProperty, value); }
        }


    }
}
