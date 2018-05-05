using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 定向模糊
    /// </summary>
    public class DirectionalBlur : EffectBase {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(DirectionalBlur), 0);
        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(DirectionalBlur), new PropertyMetadata(((double)(0D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty BlurAmountProperty = DependencyProperty.Register("BlurAmount", typeof(double), typeof(DirectionalBlur), new PropertyMetadata(((double)(0.003D)), PixelShaderConstantCallback(1)));

        public DirectionalBlur() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("DirectionalBlur") };
			this.UpdateShaderValue(InputProperty);
			this.UpdateShaderValue(AngleProperty);
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
		/// <summary>The direction of the blur (in degrees).</summary>
		public double Angle {
			get {
				return ((double)(this.GetValue(AngleProperty)));
			}
			set {
				this.SetValue(AngleProperty, value);
			}
		}
		/// <summary>The scale of the blur (as a fraction of the input size).</summary>
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
