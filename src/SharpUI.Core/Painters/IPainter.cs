using SkiaSharp;

namespace SharpUI.Core.Painters;

/// <summary>
/// Estrategia de dibujo de un widget. Separa el "cómo se ve" del comportamiento,
/// de modo que el Theme pueda reemplazar painters.
/// </summary>
public interface IPainter<in T> where T : Component
{
  void Paint(T view, SKCanvas canvas);
}
