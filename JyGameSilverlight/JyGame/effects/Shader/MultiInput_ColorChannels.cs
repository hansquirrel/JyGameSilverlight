using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 带状涡流
    /// </summary>
    public class MultiInput_ColorChannels : EffectBase {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(MultiInput_ColorChannels), 0);
        public static readonly DependencyProperty Texture1Property = ShaderEffect.RegisterPixelShaderSamplerProperty("Texture1", typeof(MultiInput_ColorChannels), 1);
        public static readonly DependencyProperty RedRatioProperty = DependencyProperty.Register("RedRatio", typeof(double), typeof(MultiInput_ColorChannels), new PropertyMetadata(((double)(1D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty BlueRatioProperty = DependencyProperty.Register("BlueRatio", typeof(double), typeof(MultiInput_ColorChannels), new PropertyMetadata(((double)(0.5D)), PixelShaderConstantCallback(1)));
        public static readonly DependencyProperty GreenRatioProperty = DependencyProperty.Register("GreenRatio", typeof(double), typeof(MultiInput_ColorChannels), new PropertyMetadata(((double)(0.5D)), PixelShaderConstantCallback(2)));

        public MultiInput_ColorChannels() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("MultiInput_ColorChannels") };
			this.UpdateShaderValue(InputProperty);
			this.UpdateShaderValue(Texture1Property);
			this.UpdateShaderValue(RedRatioProperty);
			this.UpdateShaderValue(BlueRatioProperty);
			this.UpdateShaderValue(GreenRatioProperty);
		}
		public Brush Input {
			get {
				return ((Brush)(this.GetValue(InputProperty)));
			}
			set {
				this.SetValue(InputProperty, value);
			}
		}
		/// <summary>The second input texture.</summary>
		public Brush Texture1 {
			get {
				return ((Brush)(this.GetValue(Texture1Property)));
			}
			set {
				this.SetValue(Texture1Property, value);
			}
		}
		public double RedRatio {
			get {
				return ((double)(this.GetValue(RedRatioProperty)));
			}
			set {
				this.SetValue(RedRatioProperty, value);
			}
		}
		public double BlueRatio {
			get {
				return ((double)(this.GetValue(BlueRatioProperty)));
			}
			set {
				this.SetValue(BlueRatioProperty, value);
			}
		}
		public double GreenRatio {
			get {
				return ((double)(this.GetValue(GreenRatioProperty)));
			}
			set {
				this.SetValue(GreenRatioProperty, value);
			}
		}
	}
}
