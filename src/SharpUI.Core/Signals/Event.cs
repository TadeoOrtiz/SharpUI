namespace SharpUI.Core.Signals;

/// <summary>
/// Señal de evento (equivale a un handler múltiple). En el markup se enlaza
/// con <c>OnClick={handler}</c>.
/// </summary>
public sealed class Event
{
  private event Action? handlers;

  public void Bind(Action handler) => handlers += handler;

  public void Unbind(Action handler) => handlers -= handler;

  public void Emit() => handlers?.Invoke();
}
