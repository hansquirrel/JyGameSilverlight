using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 切片
    /// </summary>
    public class Bands : EffectBase {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(Bands), 0);
        public static readonly DependencyProperty BandDensityProperty = DependencyProperty.Register("BandDensity", typeof(double), typeof(Bands), new PropertyMetadata(((double)(65D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty BandIntensityProperty = DependencyProperty.Register("BandIntensity", typeof(double), typeof(Bands), new PropertyMetadata(((double)(0.056D)), PixelShaderConstantCallback(1)));

        public Bands() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("Bands") };
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(BandDensityProperty);
            this.UpdateShaderValue(BandIntensityProperty);
        }

        public Brush Input {
            get { return ((Brush)(this.GetValue(InputProperty))); }
            set { this.SetValue(InputProperty, value); }
        }

        /// <summary>The number of verical bands to add to the output. The higher the value the more bands.</summary>
        public double BandDensity {
            get { return ((double)(this.GetValue(BandDensityProperty))); }
            set { this.SetValue(BandDensityProperty, value); }
        }

        /// <summary>Intensity of each band.</summary>
        public double BandIntensity {
            get { return ((double)(this.GetValue(BandIntensityProperty))); }
            set { this.SetValue(BandIntensityProperty, value); }
        }
    }
}
