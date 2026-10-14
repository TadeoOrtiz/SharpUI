using SkiaSharp;

namespace SharpUI.Core.Layout;

/// <summary>
/// Algoritmo de layout de un contenedor. Se puede intercambiar por contenedor
/// (flex, filas, columnas, etc.).
/// </summary>
public interface ILayout
{
  SKSize Measure(Component container, SKSize available);
  void Arrange(Component container, SKRect rect);
}
