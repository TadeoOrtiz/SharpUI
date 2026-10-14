using SharpUI.Core.Layout;
using SkiaSharp;

namespace SharpUI.Core.Widgets;

public class Flex : View
{
  private readonly FlexLayout flexLayout = new();

  /// <summary>
  /// Algoritmo de layout del contenedor. Por defecto, FlexLayout.
  /// </summary>
  public ILayout layout { get; set; }

  public Flex()
  {
    layout = flexLayout;
  }

  public Display display { get => flexLayout.direction; set => flexLayout.direction = value; }
  public float spacing { get => flexLayout.spacing; set => flexLayout.spacing = value; }
  public float padding { get => flexLayout.padding; set => flexLayout.padding = value; }
  public bool stretchChildren { get => flexLayout.stretchChildren; set => flexLayout.stretchChildren = value; }

  public override SKSize Measure(SKSize available) => layout.Measure(this, available);

  public override void Arrange(SKRect available)
  {
    Rect = available;
    layout.Arrange(this, available);
  }
}
