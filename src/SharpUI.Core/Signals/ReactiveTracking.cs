namespace SharpUI.Core.Signals;

/// <summary>
/// Contexto ambiente de lectura de señales. Permite que Computed/Effect
/// rastreen de qué señales dependen.
/// </summary>
public static class ReactiveTracking
{
  [ThreadStatic] private static Stack<Action<ISignal>>? readers;

  public static IDisposable Track(Action<ISignal> reader)
  {
    (readers ??= new()).Push(reader);
    return new Scope();
  }

  public static void NotifyRead(ISignal signal)
  {
    if (readers is { Count: > 0 })
      readers.Peek()(signal);
  }

  private sealed class Scope : IDisposable
  {
    public void Dispose() => readers?.Pop();
  }
}
