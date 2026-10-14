using SkiaSharp;

namespace SharpUI.Core.Layout;

/// <summary>
/// Layout tipo flexbox: apila los hijos en una dirección (Column/Row),
/// con padding interior, spacing entre hijos, estirado del eje cruzado
/// (stretchChildren) y flex-grow del eje principal.
/// </summary>
public sealed class FlexLayout : ILayout
{
  public Display direction { get; set; } = Display.Column;
  public float spacing { get; set; } = 0f;

  /// <summary>Espacio interior en los cuatro lados del contenedor.</summary>
  public float padding { get; set; } = 0f;

  /// <summary>
  /// Si es true los hijos ocupan todo el ancho (Column) o alto (Row) del contenedor.
  /// Equivalente a align-items: stretch de CSS.
  /// </summary>
  public bool stretchChildren { get; set; } = true;

  public SKSize Measure(Component container, SKSize available)
  {
    var children = container.Children;
    int n = children.Count;

    float outerWidth = container.width > 0 ? container.width : available.Width;
    float outerHeight = container.height > 0 ? container.height : available.Height;

    float constraintWidth = MathF.Max(0f, outerWidth - padding * 2f);
    float constraintHeight = MathF.Max(0f, outerHeight - padding * 2f);

    if (n == 0)
      return new SKSize(container.width > 0 ? container.width : padding * 2f,
                        container.height > 0 ? container.height : padding * 2f);

    float totalWidth = 0f;
    float totalHeight = 0f;
    float maxChildWidth = 0f;
    float maxChildHeight = 0f;

    foreach (var child in children)
    {
      SKSize childDesired = child.Measure(new SKSize(constraintWidth, constraintHeight));

      maxChildWidth = MathF.Max(maxChildWidth, childDesired.Width);
      maxChildHeight = MathF.Max(maxChildHeight, childDesired.Height);

      totalWidth += childDesired.Width;
      totalHeight += childDesired.Height;
    }

    if (n > 1)
    {
      if (direction == Display.Row)
        totalWidth += spacing * (n - 1);
      else
        totalHeight += spacing * (n - 1);
    }

    float contentWidth = direction == Display.Row ? totalWidth : maxChildWidth;
    float contentHeight = direction == Display.Column ? totalHeight : maxChildHeight;

    float finalWidth = container.width > 0 ? container.width : contentWidth + padding * 2f;
    float finalHeight = container.height > 0 ? container.height : contentHeight + padding * 2f;

    return new SKSize(finalWidth, finalHeight);
  }

  public void Arrange(Component container, SKRect rect)
  {
    var inner = new SKRect(
      rect.Left + padding,
      rect.Top + padding,
      rect.Right - padding,
      rect.Bottom - padding);

    if (direction == Display.Column)
      ArrangeColumn(container, inner);
    else
      ArrangeRow(container, inner);
  }

  private void ArrangeColumn(Component container, SKRect rect)
  {
    var children = container.Children;
    int n = children.Count;

    var sizes = new SKSize[n];
    float baseTotal = spacing * (n - 1);
    float growTotal = 0f;

    for (int i = 0; i < n; i++)
    {
      sizes[i] = children[i].Measure(new SKSize(rect.Width, rect.Height));
      baseTotal += children[i].height > 0 ? children[i].height : sizes[i].Height;
      growTotal += MathF.Max(0f, children[i].flexGrow);
    }

    float remaining = MathF.Max(0f, rect.Height - baseTotal);
    float unit = growTotal > 0 ? remaining / growTotal : 0f;

    float y = rect.Top;

    for (int i = 0; i < n; i++)
    {
      var child = children[i];

      float baseH = child.height > 0 ? child.height : sizes[i].Height;
      float h = baseH + MathF.Max(0f, child.flexGrow) * unit;

      float w = child.width > 0 ? child.width
              : stretchChildren ? rect.Width
              : sizes[i].Width;

      child.Arrange(new SKRect(rect.Left, y, rect.Left + w, y + h));

      y += h + spacing;
    }
  }

  private void ArrangeRow(Component container, SKRect rect)
  {
    var children = container.Children;
    int n = children.Count;

    var sizes = new SKSize[n];
    float baseTotal = spacing * (n - 1);
    float growTotal = 0f;

    for (int i = 0; i < n; i++)
    {
      sizes[i] = children[i].Measure(new SKSize(rect.Width, rect.Height));
      baseTotal += children[i].width > 0 ? children[i].width : sizes[i].Width;
      growTotal += MathF.Max(0f, children[i].flexGrow);
    }

    float remaining = MathF.Max(0f, rect.Width - baseTotal);
    float unit = growTotal > 0 ? remaining / growTotal : 0f;

    float x = rect.Left;

    for (int i = 0; i < n; i++)
    {
      var child = children[i];

      float baseW = child.width > 0 ? child.width : sizes[i].Width;
      float w = baseW + MathF.Max(0f, child.flexGrow) * unit;

      float h = child.height > 0 ? child.height
              : stretchChildren ? rect.Height
              : sizes[i].Height;

      child.Arrange(new SKRect(x, rect.Top, x + w, rect.Top + h));

      x += w + spacing;
    }
  }
}
