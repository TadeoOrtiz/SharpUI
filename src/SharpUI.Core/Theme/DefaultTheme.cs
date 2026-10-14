using SkiaSharp;

namespace SharpUI.Core.Theme;

/// <summary>
/// Tema por defecto del core. Se usa cuando ningún ThemeProvider del árbol
/// define uno. Conserva el look previo (texto blanco, sin fondos).
/// </summary>
public sealed class DefaultTheme : ITheme
{
  public static readonly DefaultTheme Instance = new();

  public SKColor background => SKColors.Transparent;
  public SKColor foreground => SKColors.White;
  public SKColor accent => SKColors.CornflowerBlue;
  public float fontSize => 12f;
  public float cornerRadius => 0f;
}
