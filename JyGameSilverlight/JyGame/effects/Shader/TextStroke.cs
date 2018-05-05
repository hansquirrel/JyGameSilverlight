using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Effects.Shader {

    /// <summary>
    /// 文字描边
    /// </summary>
    public class TextStroke : EffectBase {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(TextStroke), 0, SamplingMode.NearestNeighbor);
        public static readonly DependencyProperty FontcolorProperty = DependencyProperty.Register("Fontcolor", typeof(Color), typeof(TextStroke), new PropertyMetadata(Color.FromArgb(255, 0, 0, 0), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty BordercolorProperty = DependencyProperty.Register("Bordercolor", typeof(Color), typeof(TextStroke), new PropertyMetadata(Color.FromArgb(255, 0, 0, 0), PixelShaderConstantCallback(1)));
        public static readonly DependencyProperty DdxUvDdyUvProperty = DependencyProperty.Register("DdxUvDdyUv", typeof(Color), typeof(TextStroke/*MyfillborderEffect*/), new PropertyMetadata(Color.FromArgb(255, 0, 0, 0), PixelShaderConstantCallback(2)));
        
        public TextStroke() {
            this.PixelShader = new PixelShader() { UriSource = GetShaderUri("TextStroke") };
            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(FontcolorProperty);
            this.UpdateShaderValue(BordercolorProperty);
            this.UpdateShaderValue(DdxUvDdyUvProperty);

            PaddingBottom = 1;
            PaddingLeft = 1;
            PaddingRight = 1;
            PaddingTop = 1;
        }
        public Brush Input {
            get {
                return ((Brush)(this.GetValue(InputProperty)));
            }
            set {
                this.SetValue(InputProperty, value);
            }
        }
        public Color Fontcolor {
            get {
                return ((Color)(this.GetValue(FontcolorProperty)));
            }
            set {
                this.SetValue(FontcolorProperty, value);
            }
        }
        public Color Bordercolor {
            get {
                return ((Color)(this.GetValue(BordercolorProperty)));
            }
            set {
                this.SetValue(BordercolorProperty, value);
            }
        }
        public Color DdxUvDdyUv
        {
            get
            {
               return ((Color)(this.GetValue(DdxUvDdyUvProperty)));
            }
            set
            {
                this.SetValue(DdxUvDdyUvProperty, value);
            }
        }
    }
}
