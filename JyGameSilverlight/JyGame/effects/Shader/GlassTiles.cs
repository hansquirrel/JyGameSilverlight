using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 玻璃砖
    /// </summary>
    public class GlassTiles : EffectBase {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(GlassTiles), 0);
        public static readonly DependencyProperty TilesProperty = DependencyProperty.Register("Tiles", typeof(double), typeof(GlassTiles), new PropertyMetadata(((double)(5D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty BevelWidthProperty = DependencyProperty.Register("BevelWidth", typeof(double), typeof(GlassTiles), new PropertyMetadata(((double)(1D)), PixelShaderConstantCallback(1)));
        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register("Offset", typeof(double), typeof(GlassTiles), new PropertyMetadata(((double)(1D)), PixelShaderConstantCallback(3)));
        public static readonly DependencyProperty GroutColorProperty = DependencyProperty.Register("GroutColor", typeof(Color), typeof(GlassTiles), new PropertyMetadata(Color.FromArgb(255, 0, 0, 0), PixelShaderConstantCallback(2)));

        public GlassTiles() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("GlassTiles") };
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(TilesProperty);
            this.UpdateShaderValue(BevelWidthProperty);
            this.UpdateShaderValue(OffsetProperty);
            this.UpdateShaderValue(GroutColorProperty);
        }

        public Brush Input {
            get { return ((Brush)(this.GetValue(InputProperty))); }
            set { this.SetValue(InputProperty, value); }
        }

        /// <summary>The approximate number of tiles per row/column.</summary>
        public double Tiles {
            get { return ((double)(this.GetValue(TilesProperty))); }
            set { this.SetValue(TilesProperty, value); }
        }

        public double BevelWidth {
            get { return ((double)(this.GetValue(BevelWidthProperty))); }
            set { this.SetValue(BevelWidthProperty, value); }
        }

        public double Offset {
            get { return ((double)(this.GetValue(OffsetProperty))); }
            set { this.SetValue(OffsetProperty, value); }
        }

        public Color GroutColor {
            get { return ((Color)(this.GetValue(GroutColorProperty))); }
            set { this.SetValue(GroutColorProperty, value); }
        }
    }
}