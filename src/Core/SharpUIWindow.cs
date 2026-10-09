using SharpUI.Components;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;

public class SharpUIWindow
{
  private IWindow? window;
  private GL gl;
  private GRContext grContext;
  private IInputContext inputContext;
  private GRBackendRenderTarget renderTarget;
  private SKSurface surface;
  private SKCanvas canvas;

  private Component root;

  public SharpUIWindow(int x = 800, int y = 600, string title = "SharpUI")
  {
    var options = WindowOptions.Default;
    options.Size = new(x, y);
    options.Title = title;

    window = Window.Create(options);

    window.Load += OnLoad;
    window.Resize += OnResize;
    window.Render += OnRender;
    window.Closing += OnClosing;
  }

  public void Run(Component root)
  {
    this.root = root;
    window?.Run();
  }


  private void ConfigureSkiaSurface()
  {
    surface?.Dispose();
    renderTarget?.Dispose();

    var maxSamples = grContext.GetMaxSurfaceSampleCount(SKColorType.Rgba8888);
    var framebufferInfo = new GRGlFramebufferInfo(0, 0x8058);

    renderTarget = new GRBackendRenderTarget(
        window!.Size.X,
        window!.Size.Y,
        maxSamples,
        8,
        framebufferInfo
    );

    surface = SKSurface.Create(grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
    canvas = surface.Canvas;
  }

  private void ConfigureInput()
  {
    foreach (var mouse in inputContext.Mice)
    {
      //mouse.MouseMove += (m, pos) =>
      //{
      //  root.PropageOnClick()
      //};

      mouse.MouseDown += root.PropageOnClick;

      //mouse.MouseUp += (m, button) =>
      //{
      //  root.OnClick();
      //};
    }
  }

  private void OnClosing()
  {
    surface?.Dispose();
    renderTarget?.Dispose();
    grContext?.Dispose();
    gl?.Dispose();
    root?.Dispose();
  }

  private void OnRender(double delta)
  {
    grContext.ResetContext();

    // RSX START

    root.PropageRender(canvas);

    // RSX END
    canvas.Flush();
  }


  private void OnResize(Vector2D<int> d)
  {
    ConfigureSkiaSurface();
  }

  private void OnLoad()
  {
    gl = window.CreateOpenGL();

    var glInterface = GRGlInterface.Create();
    grContext = GRContext.CreateGl(glInterface);

    inputContext = window.CreateInput();


    root.SetInputContext(inputContext);

    ConfigureSkiaSurface();
    ConfigureInput();

    root.Initialize();
  }

}