using SkiaSharp;

namespace SharpUI.Core.Layout;

/// <summary>
/// Contrato de un elemento que puede medirse y posicionarse.
/// </summary>
public interface ILayoutable
{
  SKSize Measure(SKSize available);
  void Arrange(SKRect rect);
}
