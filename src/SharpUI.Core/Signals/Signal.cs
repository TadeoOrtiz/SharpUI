namespace SharpUI.Core.Signals;

/// <summary>
/// Valor reactivo. Al asignar <see cref="Value"/> notifica a los suscriptores
/// solo si el valor cambió. Reemplaza al viejo patrón SetState + Invalidate.
/// </summary>
public sealed class Signal<T> : ISignal
{
  private T value;
  private readonly List<Action> observers = new();

  public Signal(T initial) => value = initial;

  public T Value
  {
    get
    {
      ReactiveTracking.NotifyRead(this);
      return value;
    }
    set
    {
      if (EqualityComparer<T>.Default.Equals(value, this.value)) return;
      this.value = value;
      Emit();
    }
  }

  public void Subscribe(Action observer) => observers.Add(observer);

  public void Unsubscribe(Action observer) => observers.Remove(observer);

  public void Emit()
  {
    // Copia para permitir desuscribir durante la notificación.
    foreach (var observer in observers.ToArray())
      observer();
  }
}
