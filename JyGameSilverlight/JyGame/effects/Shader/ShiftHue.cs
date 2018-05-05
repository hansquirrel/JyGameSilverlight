using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {
	
    /// <summary>
    /// 色相
    /// </summary>
	public class ShiftHue : EffectBase {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(ShiftHue), 0);
        public static readonly DependencyProperty HueShiftProperty = DependencyProperty.Register("HueShift", typeof(double), typeof(ShiftHue), new PropertyMetadata(((double)(0)), PixelShaderConstantCallback(0)));

        public ShiftHue() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("ShiftHue") };
			this.UpdateShaderValue(InputProperty);
			this.UpdateShaderValue(HueShiftProperty);
		}

		public Brush Input {
			get {return ((Brush)(this.GetValue(InputProperty)));}
			set {this.SetValue(InputProperty, value);}
		}

		/// <summary>色相</summary>
		public double HueShift {
			get {return ((double)(this.GetValue(HueShiftProperty)));}
			set {this.SetValue(HueShiftProperty, value);}
		}

	}
}
