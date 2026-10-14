using SkiaSharp;
using SharpUI.Core.Widgets;

namespace SharpUI.Core.Painters;

public class LabelPainter : ViewPainter
{
  private readonly SKFont font = new();
  private readonly SKPaint textPaint = new();

  public override void Paint(View view, SKCanvas canvas)
  {
    base.Paint(view, canvas);

    if (view is not Label label) return;

    var theme = view.resolveTheme();
    font.Size = label.fontSize > 0 ? label.fontSize : theme.fontSize;
    textPaint.Color = label.textColor.Alpha > 0 ? label.textColor : theme.foreground;

    canvas.DrawText(label.text, label.Rect.Left, label.Rect.Top + font.Size, SKTextAlign.Left, font, textPaint);
  }

  public override void Dispose()
  {
    font.Dispose();
    textPaint.Dispose();
    base.Dispose();
  }
}
