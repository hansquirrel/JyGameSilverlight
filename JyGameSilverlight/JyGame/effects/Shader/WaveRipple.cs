using System.Windows;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 波纹特效
    /// </summary>
    public class WaveRipple : EffectBase {

        public static DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(WaveRipple), 0);

        public static DependencyProperty CenterProperty = DependencyProperty.Register("Center", typeof(System.Windows.Point), typeof(WaveRipple), new PropertyMetadata(new System.Windows.Point(), PixelShaderConstantCallback(0)));

        public static DependencyProperty AmplitudeProperty = DependencyProperty.Register("Amplitude", typeof(double), typeof(WaveRipple), new PropertyMetadata(new double(), PixelShaderConstantCallback(1)));

        public static DependencyProperty FrequencyProperty = DependencyProperty.Register("Frequency", typeof(double), typeof(WaveRipple), new PropertyMetadata(new double(), PixelShaderConstantCallback(2)));

        public static DependencyProperty PhaseProperty = DependencyProperty.Register("Phase", typeof(double), typeof(WaveRipple), new PropertyMetadata(new double(), PixelShaderConstantCallback(3)));

        public WaveRipple() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("WaveRipple") };
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(CenterProperty);
            this.UpdateShaderValue(AmplitudeProperty);
            this.UpdateShaderValue(FrequencyProperty);
            this.UpdateShaderValue(PhaseProperty);
        }

        public virtual System.Windows.Media.Brush Input {
            get { return ((System.Windows.Media.Brush)(GetValue(InputProperty))); }
            set { SetValue(InputProperty, value); }
        }

        public virtual System.Windows.Point Center {
            get { return ((System.Windows.Point)(GetValue(CenterProperty))); }
            set { SetValue(CenterProperty, value); }
        }

        public virtual double Amplitude {
            get { return ((double)(GetValue(AmplitudeProperty))); }
            set { SetValue(AmplitudeProperty, value); }
        }

        public virtual double Frequency {
            get { return ((double)(GetValue(FrequencyProperty))); }
            set { SetValue(FrequencyProperty, value); }
        }

        public virtual double Phase {
            get { return ((double)(GetValue(PhaseProperty))); }
            set { SetValue(PhaseProperty, value); }
        }

    }
}