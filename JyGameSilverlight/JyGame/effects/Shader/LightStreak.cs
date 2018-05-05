using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Effects;

namespace Effects.Shader {

    /// <summary>
    /// 雷电
    /// </summary>
    public class LightStreak : EffectBase {

        public static DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(LightStreak), 0);
        public static DependencyProperty BrightthresholdProperty = DependencyProperty.Register("Brightthreshold", typeof(double), typeof(LightStreak), new PropertyMetadata(new double(), PixelShaderConstantCallback(0)));
        public static DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(LightStreak), new PropertyMetadata(new double(), PixelShaderConstantCallback(1)));
        public static DependencyProperty AttenuationProperty = DependencyProperty.Register("Attenuation", typeof(double), typeof(LightStreak), new PropertyMetadata(new double(), PixelShaderConstantCallback(2)));
        
        public LightStreak() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("LightStreak") };
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(BrightthresholdProperty);
            this.UpdateShaderValue(ScaleProperty);
            this.UpdateShaderValue(AttenuationProperty);
        }

        public virtual Brush Input {
            get { return ((System.Windows.Media.Brush)(GetValue(InputProperty))); }
            set { SetValue(InputProperty, value); }
        }

        /// <summary>
        /// 亮度(大于1则纯白)
        /// </summary>
        public virtual double Brightthreshold {
            get { return ((double)(GetValue(BrightthresholdProperty))); }
            set { SetValue(BrightthresholdProperty, value); }
        }

        /// <summary>
        /// 规模
        /// </summary>
        public virtual double Scale {
            get { return ((double)(GetValue(ScaleProperty))); }
            set { SetValue(ScaleProperty, value); }
        }

        public virtual double Attenuation {
            get { return ((double)(GetValue(AttenuationProperty))); }
            set { SetValue(AttenuationProperty, value); }
        }

    }
}