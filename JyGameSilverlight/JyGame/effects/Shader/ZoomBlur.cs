using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 模糊缩放
    /// </summary>
    public class ZoomBlur : EffectBase {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(ZoomBlur), 0);
        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register("Center", typeof(Point), typeof(ZoomBlur), new PropertyMetadata(new Point(0.9D, 0.6D), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty BlurAmountProperty = DependencyProperty.Register("BlurAmount", typeof(double), typeof(ZoomBlur), new PropertyMetadata(((double)(0.1D)), PixelShaderConstantCallback(1)));

        public ZoomBlur() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("ZoomBlur") };
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(CenterProperty);
            this.UpdateShaderValue(BlurAmountProperty);
        }
        public Brush Input {
            get {
                return ((Brush)(this.GetValue(InputProperty)));
            }
            set {
                this.SetValue(InputProperty, value);
            }
        }
        /// <summary>The center of the blur.</summary>
        public Point Center {
            get {
                return ((Point)(this.GetValue(CenterProperty)));
            }
            set {
                this.SetValue(CenterProperty, value);
            }
        }
        /// <summary>The amount of blur.</summary>
        public double BlurAmount {
            get {
                return ((double)(this.GetValue(BlurAmountProperty)));
            }
            set {
                this.SetValue(BlurAmountProperty, value);
            }
        }
    }
}