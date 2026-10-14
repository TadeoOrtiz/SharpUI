using SharpUI.Core.Theme;
using SharpUI.Core.Widgets;

namespace SharpUI.Theme;

/// <summary>
/// Contenedor que aplica un tema a todo su subárbol.
/// La resolución del tema activo sube por el árbol hasta encontrarlo.
/// </summary>
public class ThemeProvider : Flex, IThemeProvider
{
  public ITheme theme { get; set; } = new Theme();
}
