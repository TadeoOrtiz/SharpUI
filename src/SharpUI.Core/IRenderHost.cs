using SkiaSharp;

namespace SharpUI.Core;

public interface IRenderHost
{
  void Invalidate(SKRect? region = null, bool relayout = false);
}
