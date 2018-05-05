using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 百叶窗
    /// </summary>
    public class Blinds : EffectBase {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(Blinds), 0);
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress", typeof(double), typeof(Blinds), new PropertyMetadata(((double)(30D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty NumberOfBlindsProperty = DependencyProperty.Register("NumberOfBlinds", typeof(double), typeof(Blinds), new PropertyMetadata(((double)(5D)), PixelShaderConstantCallback(1)));
        public static readonly DependencyProperty Texture2Property = ShaderEffect.RegisterPixelShaderSamplerProperty("Texture2", typeof(Blinds), 1);

        public Blinds() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("Blinds") };
			this.UpdateShaderValue(InputProperty);
			this.UpdateShaderValue(ProgressProperty);
			this.UpdateShaderValue(NumberOfBlindsProperty);
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
		/// <summary>The number of Blinds strips </summary>
		public double NumberOfBlinds {
			get {
				return ((double)(this.GetValue(NumberOfBlindsProperty)));
			}
			set {
				this.SetValue(NumberOfBlindsProperty, value);
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
