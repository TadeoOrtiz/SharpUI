using SkiaSharp;
using SharpUI.Core.Widgets;

namespace SharpUI.Core.Painters;

public class ButtonPainter : ViewPainter
{
  private readonly SKFont font = new();
  private readonly SKPaint statePaint = new();
  private readonly SKPaint textPaint = new() { IsAntialias = true };

  public override void Paint(View view, SKCanvas canvas)
  {
    base.Paint(view, canvas);

    if (view is not Button button) return;

    var theme = view.resolveTheme();
    float size = theme.fontSize > 0 ? theme.fontSize : 14f;
    font.Size = size;

    // Capa de estado (hover / pressed / disabled) encima del fondo base.
    var baseColor = button.backgroundColor.Alpha > 0 ? button.backgroundColor : theme.background;
    if (baseColor.Alpha > 0)
    {
      var stateColor = baseColor;
      if (!button.isEnabled) stateColor = stateColor.WithAlpha((byte)(stateColor.Alpha * 0.5f));
      else if (button.isPressed) stateColor = Shade(baseColor, -0.18f);
      else if (button.isHovered) stateColor = Shade(baseColor, 0.12f);

      float radius = view.cornerRadius > 0 ? view.cornerRadius : theme.cornerRadius;

      statePaint.Reset();
      statePaint.IsAntialias = true;
      statePaint.Style = SKPaintStyle.Fill;
      statePaint.Color = stateColor;

      if (radius > 0) canvas.DrawRoundRect(button.Rect, radius, radius, statePaint);
      else canvas.DrawRect(button.Rect, statePaint);
    }

    var textColor = button.textColor.Alpha > 0 ? button.textColor : theme.foreground;
    if (!button.isEnabled) textColor = textColor.WithAlpha((byte)(textColor.Alpha * 0.6f));
    textPaint.Color = textColor;

    var metrics = font.Metrics;
    float baseline = button.Rect.MidY - (metrics.Ascent + metrics.Descent) / 2f;

    canvas.DrawText(button.text, button.Rect.MidX, baseline, SKTextAlign.Center, font, textPaint);
  }

  private static SKColor Shade(SKColor color, float amount)
  {
    byte Mix(byte channel) => amount >= 0
        ? (byte)(channel + (255 - channel) * amount)
        : (byte)(channel * (1f + amount));

    return new SKColor(Mix(color.Red), Mix(color.Green), Mix(color.Blue), color.Alpha);
  }

  public override void Dispose()
  {
    font.Dispose();
    statePaint.Dispose();
    textPaint.Dispose();
    base.Dispose();
  }
}
