using SharpUI.Core.Painters;
using SharpUI.Core.Signals;
using SkiaSharp;

namespace SharpUI.Core.Widgets;

public class Label : View
{
  public readonly Signal<string> Text = new("");
  public readonly Signal<float> FontSize = new(12f);
  public readonly Signal<SKColor> TextColor = new(default);

  public string text { get => Text.Value; set => Text.Value = value; }
  public float fontSize { get => FontSize.Value; set => FontSize.Value = value; }
  public SKColor textColor { get => TextColor.Value; set => TextColor.Value = value; }

  protected readonly SKFont measureFont = new();

  public Label()
  {
    (painter as IDisposable)?.Dispose();
    painter = new LabelPainter();

    Observe(Text, relayout: true);
    Observe(FontSize, relayout: true);
    Observe(TextColor);
  }

  public override SKSize Measure(SKSize availableConstraint)
  {
    measureFont.Size = fontSize;
    return new SKSize(measureFont.MeasureText(text), fontSize);
  }

  public override void Dispose()
  {
    measureFont.Dispose();
    base.Dispose();
  }
}
