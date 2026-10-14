using SkiaSharp;
using SharpUI.Core.Widgets;

namespace SharpUI.Core.Painters;

/// <summary>
/// Painter base: dibuja fondo y borde de cualquier View.
/// </summary>
public class ViewPainter : IPainter<View>, IDisposable
{
  protected readonly SKPaint paint = new() { IsAntialias = true };

  public virtual void Paint(View view, SKCanvas canvas)
  {
    var theme = view.resolveTheme();

    var background = view.backgroundColor;
    // El fondo del tema solo se aplica a la raíz del árbol (el "body"),
    // no a cada View transparente.
    if (background.Alpha == 0 && view.isRoot) background = theme.background;
    float radius = view.cornerRadius > 0
        ? view.cornerRadius
        : view.isRoot ? 0f : theme.cornerRadius;

    if (background.Alpha > 0)
    {
      paint.Reset();
      paint.IsAntialias = true;
      paint.Style = SKPaintStyle.Fill;
      paint.Color = background;

      if (radius > 0) canvas.DrawRoundRect(view.Rect, radius, radius, paint);
      else canvas.DrawRect(view.Rect, paint);
    }

    if (view.borderWidth > 0 && view.borderColor.Alpha > 0)
    {
      paint.Reset();
      paint.IsAntialias = true;
      paint.Style = SKPaintStyle.Stroke;
      paint.StrokeWidth = view.borderWidth;
      paint.Color = view.borderColor;

      if (radius > 0) canvas.DrawRoundRect(view.Rect, radius, radius, paint);
      else canvas.DrawRect(view.Rect, paint);
    }
  }

  public virtual void Dispose() => paint.Dispose();
}
