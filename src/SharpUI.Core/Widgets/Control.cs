using SharpUI.Core.Signals;

namespace SharpUI.Core.Widgets;

public abstract class Control : View
{
  public readonly Event OnMouseEnter = new();
  public readonly Event OnMouseLeave = new();
  public readonly Event OnMouseDown = new();
  public readonly Event OnMouseUp = new();
  public readonly Event OnClick = new();

  public void RaiseMouseEnter() => OnMouseEnter.Emit();
  public void RaiseMouseLeave() => OnMouseLeave.Emit();
  public void RaiseMouseDown() => OnMouseDown.Emit();
  public void RaiseMouseUp() => OnMouseUp.Emit();
  public void RaiseClick() => OnClick.Emit();
}
