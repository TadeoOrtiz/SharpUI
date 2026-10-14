using SharpUI.Core.Painters;
using SharpUI.Core.Signals;
using SkiaSharp;

namespace SharpUI.Core.Widgets;

public class View : Component
{
  public readonly Signal<SKColor> BackgroundColor = new(default);
  public readonly Signal<SKColor> BorderColor = new(default);
  public readonly Signal<float> BorderWidth = new(0f);
  public readonly Signal<float> CornerRadius = new(0f);

  public SKColor backgroundColor { get => BackgroundColor.Value; set => BackgroundColor.Value = value; }
  public SKColor borderColor { get => BorderColor.Value; set => BorderColor.Value = value; }
  public float borderWidth { get => BorderWidth.Value; set => BorderWidth.Value = value; }
  public float cornerRadius { get => CornerRadius.Value; set => CornerRadius.Value = value; }

  public IPainter<View> painter { get; set; } = new ViewPainter();

  public View()
  {
    Observe(BackgroundColor);
    Observe(BorderColor);
    Observe(BorderWidth);
    Observe(CornerRadius);
  }

  public override void Render(SKCanvas canvas) => painter.Paint(this, canvas);

  public override void Dispose()
  {
    (painter as IDisposable)?.Dispose();
    base.Dispose();
  }
}
