using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 亮度与对比度
    /// </summary>
    public class ContrastAdjust : EffectBase {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(ContrastAdjust), 0);
        public static readonly DependencyProperty BrightnessProperty = DependencyProperty.Register("Brightness", typeof(double), typeof(ContrastAdjust), new PropertyMetadata(((double)(0D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty ContrastProperty = DependencyProperty.Register("Contrast", typeof(double), typeof(ContrastAdjust), new PropertyMetadata(((double)(1.5D)), PixelShaderConstantCallback(1)));

        public ContrastAdjust() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("ContrastAdjust") };
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(BrightnessProperty);
            this.UpdateShaderValue(ContrastProperty);
        }

        public Brush Input {
            get { return ((Brush)(this.GetValue(InputProperty))); }
            set { this.SetValue(InputProperty, value); }
        }

        /// <summary>亮度</summary>
        public double Brightness {
            get { return ((double)(this.GetValue(BrightnessProperty))); }
            set { this.SetValue(BrightnessProperty, value); }
        }

        /// <summary>对比度</summary>
        public double Contrast {
            get { return ((double)(this.GetValue(ContrastProperty))); }
            set { this.SetValue(ContrastProperty, value); }
        }

    }
}
