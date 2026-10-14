namespace SharpUI.Core.Signals;

/// <summary>
/// Algo observable al que un componente puede suscribirse.
/// </summary>
public interface ISignal
{
  void Subscribe(Action observer);
  void Unsubscribe(Action observer);
}
