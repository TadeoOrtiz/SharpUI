using System.Numerics;
using SharpUI.Components;
using SkiaSharp;

namespace SharpUI.Components;

public class RawView : Component
{

  public override void Render(SKCanvas canvas)
  {
    using var paint = new SKPaint();
    paint.Color = BackgroundColor;
    canvas.DrawRect(Parent.Width, Parent.Height, Width, Height, paint);
  }

}