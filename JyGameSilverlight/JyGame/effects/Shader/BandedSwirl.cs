using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 带状涡流
    /// </summary>
    public class BandedSwirl : EffectBase {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(BandedSwirl), 0);
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(BandedSwirl), new PropertyMetadata(((double)(30D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty TwistAmountProperty = DependencyProperty.Register("TwistAmount", typeof(double), typeof(BandedSwirl), new PropertyMetadata(((double)(1D)), PixelShaderConstantCallback(1)));
        public static readonly DependencyProperty FrequencyProperty = DependencyProperty.Register("Frequency", typeof(double), typeof(BandedSwirl), new PropertyMetadata(((double)(20D)), PixelShaderConstantCallback(2)));
        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register("Center", typeof(Point), typeof(BandedSwirl), new PropertyMetadata(new Point(0.5D, 0.5D), PixelShaderConstantCallback(3)));
        public static readonly DependencyProperty Texture2Property = ShaderEffect.RegisterPixelShaderSamplerProperty("Texture2", typeof(BandedSwirl), 1);

        public BandedSwirl() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("BandedSwirl") };
			this.UpdateShaderValue(InputProperty);
			this.UpdateShaderValue(ProgressProperty);
			this.UpdateShaderValue(TwistAmountProperty);
			this.UpdateShaderValue(FrequencyProperty);
			this.UpdateShaderValue(CenterProperty);
			this.UpdateShaderValue(Texture2Property);
		}
		public Brush Input {
			get {
				return ((Brush)(this.GetValue(InputProperty)));
			}
			set {
				this.SetValue(InputProperty, value);
			}
		}
		/// <summary>The amount(%) of the transition from first texture to the second texture. </summary>
		public double Progress {
			get {
				return ((double)(this.GetValue(ProgressProperty)));
			}
			set {
				this.SetValue(ProgressProperty, value);
			}
		}
		/// <summary>The amount of twist for the Swirl. </summary>
		public double TwistAmount {
			get {
				return ((double)(this.GetValue(TwistAmountProperty)));
			}
			set {
				this.SetValue(TwistAmountProperty, value);
			}
		}
		/// <summary>The amount of twist for the Swirl. </summary>
		public double Frequency {
			get {
				return ((double)(this.GetValue(FrequencyProperty)));
			}
			set {
				this.SetValue(FrequencyProperty, value);
			}
		}
		/// <summary>The amount of twist for the Swirl. </summary>
		public Point Center {
			get {
				return ((Point)(this.GetValue(CenterProperty)));
			}
			set {
				this.SetValue(CenterProperty, value);
			}
		}
		public Brush Texture2 {
			get {
				return ((Brush)(this.GetValue(Texture2Property)));
			}
			set {
				this.SetValue(Texture2Property, value);
			}
		}
	}
}