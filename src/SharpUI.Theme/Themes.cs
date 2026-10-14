using SkiaSharp;

namespace SharpUI.Theme;

/// <summary>
/// Temas listos para usar.
/// </summary>
public static class Themes
{
  public static Theme Light() => new();

  public static Theme Dark() => new()
  {
    background = new SKColor(0x1E, 0x29, 0x3B),
    foreground = SKColors.White,
    accent = new SKColor(0x3B, 0x82, 0xF6),
    fontSize = 14f,
    cornerRadius = 6f,
  };
}
