using SharpUI.Core.Signals;

namespace SharpUI.Reactive;

/// <summary>
/// Señal derivada: su valor se calcula a partir de otras señales y se
/// recalcula automáticamente cuando alguna cambia.
/// </summary>
public sealed class Computed<T> : ISignal, IDisposable
{
  private readonly Func<T> compute;
  private readonly List<Action> observers = new();
  private readonly Dictionary<ISignal, Action> handlers = new();

  private T value;

  public Computed(Func<T> compute)
  {
    this.compute = compute;
    value = Evaluate();
  }

  public T Value
  {
    get
    {
      ReactiveTracking.NotifyRead(this);
      return value;
    }
  }

  private T Evaluate()
  {
    foreach (var (signal, handler) in handlers)
      signal.Unsubscribe(handler);
    handlers.Clear();

    using (ReactiveTracking.Track(OnRead))
      return compute();
  }

  private void OnRead(ISignal signal)
  {
    if (handlers.ContainsKey(signal)) return;

    Action handler = () =>
    {
      T next = Evaluate();
      if (EqualityComparer<T>.Default.Equals(next, value)) return;
      value = next;
      Emit();
    };

    handlers[signal] = handler;
    signal.Subscribe(handler);
  }

  public void Subscribe(Action observer) => observers.Add(observer);

  public void Unsubscribe(Action observer) => observers.Remove(observer);

  private void Emit()
  {
    foreach (var observer in observers.ToArray())
      observer();
  }

  public void Dispose()
  {
    foreach (var (signal, handler) in handlers)
      signal.Unsubscribe(handler);
    handlers.Clear();
  }
}
