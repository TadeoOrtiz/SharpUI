using System.Text;

namespace SharpUI.Generator
{
  internal static class Utils
  {
    public static StringBuilder AppendUsings(this StringBuilder sb)
    {
      sb.AppendLine("using System;");
      sb.AppendLine("using SharpUI.Core;");
      sb.AppendLine("using SharpUI.Core.Widgets;");
      sb.AppendLine("using SharpUI.Core.Layout;");
      sb.AppendLine("using SharpUI.Core.Signals;");
      sb.AppendLine("using SharpUI.Reactive;");
      sb.AppendLine("using SharpUI.Theme;");
      sb.AppendLine("using SkiaSharp;");
      return sb;
    }
  }
}