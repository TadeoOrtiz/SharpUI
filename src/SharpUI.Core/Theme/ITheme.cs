using SkiaSharp;

namespace SharpUI.Core.Theme;

/// <summary>
/// Superficie de estilo que consumen los painters. La implementación concreta
/// (colores, fuentes, etc.) vive en el módulo SharpUI.Theme.
/// </summary>
public interface ITheme
{
  SKColor background { get; }
  SKColor foreground { get; }
  SKColor accent { get; }
  float fontSize { get; }
  float cornerRadius { get; }
}
