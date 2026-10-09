using SkiaSharp;

namespace SharpUI.Components;

public class Flex : RawView
{
  public override void Render(SKCanvas canvas)
  {
    var paint = new SKPaint
    {
      Color = BackgroundColor
    };
    canvas.DrawRect(Rect, paint);
  }
}