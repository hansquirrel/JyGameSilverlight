using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 老电影(回忆)
    /// </summary>
    public class OldMovie : EffectBase {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(OldMovie), 0);
        public static readonly DependencyProperty ScratchAmountProperty = DependencyProperty.Register("ScratchAmount", typeof(double), typeof(OldMovie), new PropertyMetadata(((double)(0D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty NoiseAmountProperty = DependencyProperty.Register("NoiseAmount", typeof(double), typeof(OldMovie), new PropertyMetadata(((double)(0D)), PixelShaderConstantCallback(1)));
        public static readonly DependencyProperty RandomCoord1Property = DependencyProperty.Register("RandomCoord1", typeof(Point), typeof(OldMovie), new PropertyMetadata(new Point(0D, 0D), PixelShaderConstantCallback(2)));
        public static readonly DependencyProperty RandomCoord2Property = DependencyProperty.Register("RandomCoord2", typeof(Point), typeof(OldMovie), new PropertyMetadata(new Point(0D, 0D), PixelShaderConstantCallback(3)));
        public static readonly DependencyProperty FrameProperty = DependencyProperty.Register("Frame", typeof(double), typeof(OldMovie), new PropertyMetadata(((double)(0D)), PixelShaderConstantCallback(4)));
        public static readonly DependencyProperty NoiseSamplerProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("NoiseSampler", typeof(OldMovie), 1);

        public OldMovie() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("OldMovie") };
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(ScratchAmountProperty);
            this.UpdateShaderValue(NoiseAmountProperty);
            this.UpdateShaderValue(RandomCoord1Property);
            this.UpdateShaderValue(RandomCoord2Property);
            this.UpdateShaderValue(FrameProperty);
            this.UpdateShaderValue(NoiseSamplerProperty);
        }

        public Brush Input {
            get { return ((Brush)(this.GetValue(InputProperty))); }
            set { this.SetValue(InputProperty, value); }
        }

        public double ScratchAmount {
            get { return ((double)(this.GetValue(ScratchAmountProperty))); }
            set { this.SetValue(ScratchAmountProperty, value); }
        }

        public double NoiseAmount {
            get { return ((double)(this.GetValue(NoiseAmountProperty))); }
            set { this.SetValue(NoiseAmountProperty, value); }
        }
        /// <summary>The random coordinate 1 that is used for lookup in the noise texture.</summary>
        public Point RandomCoord1 {
            get { return ((Point)(this.GetValue(RandomCoord1Property))); }
            set { this.SetValue(RandomCoord1Property, value); }
        }

        /// <summary>The random coordinate 2 that is used for lookup in the noise texture.</summary>
        public Point RandomCoord2 {
            get { return ((Point)(this.GetValue(RandomCoord2Property))); }
            set { this.SetValue(RandomCoord2Property, value); }
        }

        /// <summary>The current frame.</summary>
        public double Frame {
            get { return ((double)(this.GetValue(FrameProperty))); }
            set { this.SetValue(FrameProperty, value); }
        }

        public Brush NoiseSampler {
            get { return ((Brush)(this.GetValue(NoiseSamplerProperty))); }
            set { this.SetValue(NoiseSamplerProperty, value); }
        }

    }
}
