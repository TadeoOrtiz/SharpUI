using SkiaSharp;
using SharpUI.Core.Layout;
using SharpUI.Core.Signals;
using SharpUI.Core.Theme;
using SharpUI.Core.Widgets;

namespace SharpUI.Core;

public abstract class Component : ILayoutable, IDisposable
{

  protected Component? Parent { get; set; }
  public IRenderHost? Host { get; set; }
  public List<Component> Children { get; set; } = [];

  /// <summary>True si es la raíz del árbol (sin padre).</summary>
  public bool isRoot => Parent == null;


  public SKRect Rect { get; set; }
  public float height { get; set; }
  public float width { get; set; }

  /// <summary>
  /// Cuánto crece este componente para ocupar el espacio sobrante del eje
  /// principal de su contenedor. 0 = no crece (flex-grow de CSS).
  /// </summary>
  public float flexGrow { get; set; } = 0f;

  private List<(ISignal signal, Action handler)>? observations;

  public virtual SKSize Measure(SKSize availableConstraint)
  {
    float w = width > 0 ? width : availableConstraint.Width;
    float h = height > 0 ? height : availableConstraint.Height;

    foreach (var child in Children)
    {
      child.Measure(new SKSize(w, h));
    }

    return new SKSize(w, h);
  }

  public Control? HitTest(SKPoint p)
  {
    if (!Rect.Contains(p)) return null;

    for (int i = Children.Count - 1; i >= 0; i--)
    {
      var hit = Children[i].HitTest(p);
      if (hit != null) return hit;
    }
    if (this is Control c)
      return c;
    return null;
  }

  public virtual void Arrange(SKRect availableRect)
  {
    Rect = availableRect;

    foreach (var child in Children)
      child.Arrange(Rect);
  }

  public abstract void Render(SKCanvas canvas);

  public void AddChild(Component component)
  {
    component.Parent = this;
    Children.Add(component);
  }

  /// <summary>
  /// Suscribe el componente a una señal: cuando la señal cambia, se re-renderiza
  /// (y opcionalmente se vuelve a layoutar). Reemplaza el viejo SetState/Invalidate.
  /// </summary>
  protected void Observe(ISignal signal, bool relayout = false)
  {
    Action handler = () => Invalidate(relayout);
    signal.Subscribe(handler);
    (observations ??= []).Add((signal, handler));
  }

  /// <summary>
  /// Resuelve el tema activo subiendo por el árbol hasta un proveedor de tema.
  /// Si no hay ninguno, usa el tema por defecto del core.
  /// </summary>
  public ITheme resolveTheme()
  {
    for (Component? c = this; c != null; c = c.Parent)
      if (c is IThemeProvider provider)
        return provider.theme;
    return DefaultTheme.Instance;
  }

  protected void Invalidate(bool relayout = false) => InvalidateRegion(Rect, relayout);

  protected void InvalidateRegion(SKRect region, bool relayout)
  {
    if (Parent != null) Parent.InvalidateRegion(region, relayout);
    else Host?.Invalidate(region, relayout);
  }

  private static bool Overlaps(SKRect a, SKRect b) =>
  a.Left < b.Right && a.Right > b.Left && a.Top < b.Bottom && a.Bottom > b.Top;


  public void PropagateRender(SKCanvas canvas, SKRect dirty)
  {
    if (!Overlaps(Rect, dirty)) return;
    if (!Rect.IntersectsWith(dirty)) return;

    Render(canvas);
    foreach (var child in Children)
      child.PropagateRender(canvas, dirty);
  }


  public virtual void Dispose()
  {
    if (observations != null)
    {
      foreach (var (signal, handler) in observations)
        signal.Unsubscribe(handler);
      observations.Clear();
    }

    foreach (var child in Children)
      child.Dispose();
  }

}
