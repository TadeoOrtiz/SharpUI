using System.Numerics;
using SharpUI.Core;
using SharpUI.Core.Widgets;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;

namespace SharpUI.Windowing;

public class SharpUIWindow : IRenderHost
{

  private volatile bool needsLayout = true;
  private volatile bool needsRender = true;

  private IWindow? window;
  private GL gl;
  private GRContext grContext;
  private IInputContext inputContext;
  private GRBackendRenderTarget renderTarget;
  private SKSurface surface;
  private SKCanvas canvas;

  private Component root;
  private Control? hovered;
  private Control? pressed;



  private readonly object dirtyLock = new();
  private SKRect dirtyRect;
  private bool hasDirty;
  private bool fullRedraw = true;

  private SKSurface? cache;



  public SharpUIWindow(int x = 800, int y = 600, string title = "SharpUI")
  {
    var options = WindowOptions.Default;
    options.Size = new(x, y);
    options.Title = title;
    options.ShouldSwapAutomatically = false;

    window = Window.Create(options);
    window.IsEventDriven = true;

    window.Load += OnLoad;
    window.Resize += OnResize;
    window.Render += OnRender;
    window.Closing += OnClosing;
  }

  public void Run(SharpRenderComponent render)
  {
    var root = render.Build();
    this.root = root;
    root.Host = this;
    window?.Run();
  }

  public void Invalidate(SKRect? region = null, bool relayout = false)
  {
    lock (dirtyLock)
    {
      if (relayout || region == null)
      {
        // Sin rect = redibujo completo
        if (relayout) needsLayout = true;
        fullRedraw = true;
      }
      else
      {
        dirtyRect = hasDirty ? SKRect.Union(dirtyRect, region.Value) : region.Value;
        hasDirty = true;
      }
      needsRender = true;
    }
    window?.ContinueEvents();
  }

  private void ConfigureSkiaSurface()
  {
    surface?.Dispose();
    renderTarget?.Dispose();
    cache?.Dispose();

    var maxSamples = grContext.GetMaxSurfaceSampleCount(SKColorType.Rgba8888);
    renderTarget = new GRBackendRenderTarget(
        window!.Size.X, window.Size.Y, maxSamples, 8, new GRGlFramebufferInfo(0, 0x8058));
    surface = SKSurface.Create(grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
    canvas = surface.Canvas;

    var info = new SKImageInfo(window.Size.X, window.Size.Y, SKColorType.Rgba8888, SKAlphaType.Premul);
    cache = SKSurface.Create(grContext, true, info, 0, GRSurfaceOrigin.TopLeft);

    lock (dirtyLock) fullRedraw = true;
  }

  private void OnClosing()
  {
    cache?.Dispose();
    surface?.Dispose();
    renderTarget?.Dispose();
    grContext?.Dispose();
    gl?.Dispose();
    root?.Dispose();
  }

  private void OnRender(double delta)
  {
    SKRect region;
    bool full;

    lock (dirtyLock)
    {
      if (!needsRender) return;
      needsRender = false;

      if (needsLayout)
      {
        needsLayout = false;
        DoLayout();
      }

      full = fullRedraw;
      region = hasDirty ? dirtyRect : SKRect.Empty;
      fullRedraw = false;
      hasDirty = false;
    }

    if (!full && region.IsEmpty) return;

    grContext.ResetContext();

    var c = cache!.Canvas;
    c.Save();

    if (full)
    {
      region = new SKRect(0, 0, window!.Size.X, window.Size.Y);
    }
    else
    {
      region = new SKRect(
      MathF.Floor(region.Left),
      MathF.Floor(region.Top),
      MathF.Ceiling(region.Right),
      MathF.Ceiling(region.Bottom));
      c.ClipRect(region);
    }

    c.DrawColor(SKColors.Black, SKBlendMode.Src);
    root.PropagateRender(c, region);
    c.Restore();

    canvas.DrawSurface(cache, 0, 0);
    canvas.Flush();
    grContext.Flush();
    window!.SwapBuffers();
  }


  private void OnResize(Vector2D<int> size)
  {
    if (size.X <= 0 || size.Y <= 0) return;

    ConfigureSkiaSurface();
    Invalidate(relayout: true);
    window!.DoRender();
  }
  private void OnLoad()
  {
    gl = window.CreateOpenGL();

    var glInterface = GRGlInterface.Create();
    grContext = GRContext.CreateGl(glInterface);

    inputContext = window!.CreateInput();
    foreach (var mouse in inputContext.Mice)
    {
      mouse.MouseMove += OnMouseMove;
      mouse.MouseDown += OnMouseDown;
      mouse.MouseUp += OnMouseUp;
    }

    ConfigureSkiaSurface();
    DoLayout();
  }

  private void OnMouseMove(IMouse mouse, System.Numerics.Vector2 pos)
  {
    var hit = root.HitTest(new SKPoint(pos.X, pos.Y));
    if (hit == hovered) return; // sin cambio: no invalida, no dibuja

    hovered?.RaiseMouseLeave();
    hovered = hit;
    hovered?.RaiseMouseEnter();
  }

  private void OnMouseDown(IMouse mouse, MouseButton button)
  {
    if (button != MouseButton.Left) return;
    pressed = hovered;
    pressed?.RaiseMouseDown();
  }

  private void OnMouseUp(IMouse mouse, MouseButton button)
  {
    if (button != MouseButton.Left || pressed == null) return;

    var p = pressed;
    pressed = null;
    p.RaiseMouseUp();

    if (p == hovered)
      p.RaiseClick();
  }

  private void DoLayout()
  {
    root.Arrange(new SKRect(0, 0, window!.Size.X, window!.Size.Y));
  }

}
