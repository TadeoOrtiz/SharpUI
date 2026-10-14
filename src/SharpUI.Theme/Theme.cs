using SkiaSharp;
using SharpUI.Core.Theme;

namespace SharpUI.Theme;

/// <summary>
/// Tema concreto del módulo de theming. Implementa ITheme y agrega valores
/// editables de paleta.
/// </summary>
public class Theme : ITheme
{
  public SKColor background { get; set; } = new(0xF4, 0xF6, 0xF9);
  public SKColor foreground { get; set; } = SKColors.Black;
  public SKColor accent { get; set; } = new(0x3B, 0x82, 0xF6);
  public float fontSize { get; set; } = 14f;
  public float cornerRadius { get; set; } = 6f;
}
