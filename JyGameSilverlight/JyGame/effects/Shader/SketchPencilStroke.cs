using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 铅笔刻刀
    /// </summary>
    public class SketchPencilStroke : EffectBase {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(SketchPencilStroke), 0);
        public static readonly DependencyProperty BrushSizeProperty = DependencyProperty.Register("BrushSize", typeof(double), typeof(SketchPencilStroke), new PropertyMetadata(((double)(0.005D)), PixelShaderConstantCallback(0)));
        
        public SketchPencilStroke() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("SketchPencilStroke") };
			this.UpdateShaderValue(InputProperty);
			this.UpdateShaderValue(BrushSizeProperty);
		}

		public Brush Input {
			get {
				return ((Brush)(this.GetValue(InputProperty)));
			}
			set {
				this.SetValue(InputProperty, value);
			}
		}
		/// <summary>The brush size of the sketch effect.</summary>
		public double BrushSize {
			get {
				return ((double)(this.GetValue(BrushSizeProperty)));
			}
			set {
				this.SetValue(BrushSizeProperty, value);
			}
		}
	}
}
