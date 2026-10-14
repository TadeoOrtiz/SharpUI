using SharpUI.Core.Signals;

namespace SharpUI.Reactive;

/// <summary>
/// Efecto secundario que rastrea las señales que lee y se re-ejecuta cuando
/// alguna cambia. Útil para reaccionar a estado sin tocar el render.
/// </summary>
public sealed class Effect : IDisposable
{
  private readonly Action action;
  private readonly Dictionary<ISignal, Action> handlers = new();
  private bool disposed;

  public Effect(Action action)
  {
    this.action = action;
    Run();
  }

  private void Run()
  {
    if (disposed) return;

    foreach (var (signal, handler) in handlers)
      signal.Unsubscribe(handler);
    handlers.Clear();

    using (ReactiveTracking.Track(OnRead))
      action();
  }

  private void OnRead(ISignal signal)
  {
    if (handlers.ContainsKey(signal)) return;

    Action handler = Run;
    handlers[signal] = handler;
    signal.Subscribe(handler);
  }

  public void Dispose()
  {
    if (disposed) return;
    disposed = true;

    foreach (var (signal, handler) in handlers)
      signal.Unsubscribe(handler);
    handlers.Clear();
  }
}
