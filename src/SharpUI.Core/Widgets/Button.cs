using SharpUI.Core.Painters;
using SharpUI.Core.Signals;
using SkiaSharp;

namespace SharpUI.Core.Widgets;

public class Button : Control
{
  public readonly Signal<string> Text = new("Button");
  public readonly Signal<bool> IsEnabled = new(true);
  public readonly Signal<bool> IsHovered = new(false);
  public readonly Signal<bool> IsPressed = new(false);
  public readonly Signal<SKColor> TextColor = new(default);

  public string text { get => Text.Value; set => Text.Value = value; }
  public bool isEnabled { get => IsEnabled.Value; set => IsEnabled.Value = value; }
  public bool isHovered { get => IsHovered.Value; protected set => IsHovered.Value = value; }
  public bool isPressed { get => IsPressed.Value; protected set => IsPressed.Value = value; }
  public SKColor textColor { get => TextColor.Value; set => TextColor.Value = value; }

  protected readonly SKFont measureFont = new();

  public Button()
  {
    (painter as IDisposable)?.Dispose();
    painter = new ButtonPainter();

    Observe(Text, relayout: true);
    Observe(IsEnabled);
    Observe(IsHovered);
    Observe(IsPressed);
    Observe(TextColor);

    OnMouseEnter.Bind(() => isHovered = true);
    OnMouseLeave.Bind(() => isHovered = false);
    OnMouseDown.Bind(() => isPressed = true);
    OnMouseUp.Bind(() => isPressed = false);
  }

  public override SKSize Measure(SKSize availableConstraint)
  {
    var theme = resolveTheme();
    float size = theme.fontSize > 0 ? theme.fontSize : 14f;
    measureFont.Size = size;

    float textWidth = measureFont.MeasureText(text);

    float w = width > 0 ? width : textWidth + 28f;
    float h = height > 0 ? height : size + 18f;

    return new SKSize(w, h);
  }

  public override void Dispose()
  {
    measureFont.Dispose();
    base.Dispose();
  }
}
