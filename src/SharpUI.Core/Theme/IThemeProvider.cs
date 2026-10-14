namespace SharpUI.Core.Theme;

/// <summary>
/// Un componente que provee un tema a su subárbol (herencia por contexto).
/// </summary>
public interface IThemeProvider
{
  ITheme theme { get; }
}
