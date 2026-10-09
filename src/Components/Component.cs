using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using SkiaSharp;

namespace SharpUI.Components;

public class Component : IDisposable
{

  protected Component Parent { get; set; }
  public SKRect Rect { get; set; }
  public IInputContext? InputContext { get; set; }
  public float Height { get; set; }
  public float Width { get; set; }
  public Vector2D<float> Position { get; set; }

  public SKColor BackgroundColor;
  public List<Component> Childrens { get; set; } = [];
  public bool HandleInput { get; set; }


  public void SetInputContext(IInputContext inputContext)
  {
    InputContext = inputContext;
    foreach (var child in Childrens)
    {
      child.SetInputContext(inputContext);
    }
  }

  public virtual void Initialize() { }

  public void AddChild(Component component)
  {
    component.Parent = this;
    Childrens.Add(component);
  }

  public void PropageRender(SKCanvas canvas)
  {
    Render(canvas);
    foreach (var child in Childrens)
    {
      child.PropageRender(canvas);
    }
  }

  public virtual bool IsOnBounds(Vector2 position)
  {
    return false;
  }

  public void PropageOnClick(IMouse mouse, MouseButton button)
  {
    foreach (var child in Childrens)
    {
      child.PropageOnClick(mouse, button);
    }

    if (!HandleInput) return;
    if (IsOnBounds(mouse.Position))
      OnClick();
  }

  public virtual void OnClick() { }

  public virtual void Render(SKCanvas canvas) { }

  public virtual void Dispose()
  {
    foreach (var child in Childrens) child.Dispose();
  }

}
