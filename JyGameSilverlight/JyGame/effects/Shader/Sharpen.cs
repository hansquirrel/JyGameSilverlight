using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 锐化
    /// </summary>
    public class Sharpen : EffectBase {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(Sharpen), 0);
        public static readonly DependencyProperty AmountProperty = DependencyProperty.Register("Amount", typeof(double), typeof(Sharpen), new PropertyMetadata(((double)(1D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty InputSizeProperty = DependencyProperty.Register("InputSize", typeof(Size), typeof(Sharpen), new PropertyMetadata(new Size(800D, 600D), PixelShaderConstantCallback(1)));
        
        public Sharpen() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("Sharpen") };
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(AmountProperty);
            this.UpdateShaderValue(InputSizeProperty);
        }

        public Brush Input {
            get { return ((Brush)(this.GetValue(InputProperty))); }
            set { this.SetValue(InputProperty, value); }
        }

        /// <summary>The amount of sharpening.</summary>
        public double Amount {
            get { return ((double)(this.GetValue(AmountProperty))); }
            set { this.SetValue(AmountProperty, value); }
        }

        /// <summary>The size of the input (in pixels).</summary>
        public Size InputSize {
            get { return ((Size)(this.GetValue(InputSizeProperty))); }
            set { this.SetValue(InputSizeProperty, value); }
        }
    }
}