using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 盘状模糊
    /// </summary>
    public class GrowablePoissonDisk : EffectBase {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(GrowablePoissonDisk), 0);
        public static readonly DependencyProperty DiskRadiusProperty = DependencyProperty.Register("DiskRadius", typeof(double), typeof(GrowablePoissonDisk), new PropertyMetadata(((double)(5D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty InputSizeProperty = DependencyProperty.Register("InputSize", typeof(Size), typeof(GrowablePoissonDisk), new PropertyMetadata(new Size(600D, 400D), PixelShaderConstantCallback(1)));

        public GrowablePoissonDisk() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("GrowablePoissonDisk") };
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(DiskRadiusProperty);
            this.UpdateShaderValue(InputSizeProperty);
        }

        public Brush Input {
            get { return ((Brush)(this.GetValue(InputProperty))); }
            set { this.SetValue(InputProperty, value); }
        }

        /// <summary>The radius of the Poisson disk (in pixels).</summary>
        public double DiskRadius {
            get { return ((double)(this.GetValue(DiskRadiusProperty))); }
            set { this.SetValue(DiskRadiusProperty, value); }
        }

        /// <summary>The size of the input (in pixels).</summary>
        public Size InputSize {
            get { return ((Size)(this.GetValue(InputSizeProperty))); }
            set { this.SetValue(InputSizeProperty, value); }
        }
    }
}