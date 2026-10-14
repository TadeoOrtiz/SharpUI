using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SharpUI.Generator
{
  internal static class AttributeResolver
  {
    public static string GetFormattedAttributeValue(this string value)
    {
      value = value.Trim();

      if (value.StartsWith("{") && value.EndsWith("}"))
      {
        return value.Substring(1, value.Length - 2).Trim();
      }
      
      // 1. Si es un número entero o decimal válido (ej: 10, 10.5, -5)
      if (decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _))
      {
        return value;
      }

      // 2. Si es un booleano (true o false)
      if (bool.TryParse(value, out bool boolResult))
      {
        return boolResult ? "true" : "false";
      }

      // 3. Si el usuario ya lo puso explícitamente entre comillas dobles en el XML (ej: Text=""Hola"")
      // O si quieres permitir que expresiones de C# complejas pasen directo (ej: TituloPrincipal o miMetodo())
      // Puedes decidir que si empieza con ciertas estructuras o ya trae comillas lo respetes.
      // Por defecto, si no es número ni bool, lo tratamos como string envolviéndolo en comillas:

      // Nota: Si quieres bindear una propiedad del frontmatter (ej: Text="TituloPrincipal"), 
      // puedes evaluar si coincide con un identificador válido de C# o si prefieres forzar un símbolo especial.
      // Aquí lo trataremos como string literal seguro escapando las comillas:
      return $"\"{value}\"";
    }
  }
}