using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 波纹
    /// </summary>
    public class Ripple : EffectBase {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(Ripple), 0);
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(Ripple), new PropertyMetadata(((double)(30D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty Texture2Property = ShaderEffect.RegisterPixelShaderSamplerProperty("Texture2", typeof(Ripple), 1);

        public Ripple() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("Ripple") };
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(ProgressProperty);
            this.UpdateShaderValue(Texture2Property);
        }

        public Brush Input {
            get { return ((Brush)(this.GetValue(InputProperty))); }
            set { this.SetValue(InputProperty, value); }
        }

        /// <summary>The amount(%) of the transition from first texture to the second texture. </summary>
        public double Progress {
            get { return ((double)(this.GetValue(ProgressProperty))); }
            set { this.SetValue(ProgressProperty, value); }
        }

        public Brush Texture2 {
            get { return ((Brush)(this.GetValue(Texture2Property))); }
            set { this.SetValue(Texture2Property, value); }
        }
    }
}