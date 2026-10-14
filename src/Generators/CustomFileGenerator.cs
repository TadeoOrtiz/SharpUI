using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SharpUI.Generator;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Components
{
  [Generator]
  public class CustomFileGenerator : IIncrementalGenerator
  {


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
      var sharpFiles = context.AdditionalTextsProvider
          .Where(file => Path.GetExtension(file.Path).Equals(".sharp", StringComparison.OrdinalIgnoreCase))
          .Select(static (file, ct) =>
          {
            var text = file.GetText(ct)?.ToString() ?? string.Empty;

            string frontmatter = string.Empty;
            string xmlContent = text;

            // Lógica estilo Astro: Buscar bloques separados por "---"
            if (text.StartsWith("---"))
            {
              int secondDashIndex = text.IndexOf("---", 3);
              if (secondDashIndex != -1)
              {
                // Extraemos el código del medio
                frontmatter = text.Substring(3, secondDashIndex - 3).Trim();
                // Lo restante se procesa como XML
                xmlContent = text.Substring(secondDashIndex + 3).Trim();
              }
            }

            xmlContent = Regex.Replace(xmlContent, @"(\w+)=([^""\s>]+)", "$1=\"$2\"");
            
            return (
              FileName: Path.GetFileName(file.Path),
              ClassName: Path.GetFileNameWithoutExtension(file.Path),
              Frontmatter: frontmatter,
              Content: xmlContent);
          });

      // 1. Creamos una lista acumulada con los nombres de TODAS las clases que generarán los .sharp
      var allClassNamesProvider = sharpFiles
          .Select(static (file, ct) => file.ClassName)
          .Collect();

      // 2. Combinamos cada archivo individual con la lista global de nombres y con la compilación
      var pipeline = sharpFiles
          .Combine(allClassNamesProvider)
          .Combine(context.CompilationProvider);

      context.RegisterSourceOutput(pipeline, static (spc, source) =>
      {
        // Desempaquetamos la tupla anidada generada por .Combine()
        var ((data, allGeneratedNames), compilation) = source;

        var sb = new StringBuilder();



        try
        {
          XDocument doc = XDocument.Parse(data.Content);
          var tagCounters = new Dictionary<string, int>();

          sb.AppendUsings();



          sb.AppendLine();

          sb.AppendLine($"public partial class {data.ClassName} : SharpRenderComponent");
          sb.AppendLine("{");


          if (!string.IsNullOrEmpty(data.Frontmatter))
          {
            sb.AppendLine("    // --- Código de Frontmatter ---");
            sb.AppendLine(data.Frontmatter);
            sb.AppendLine("    // -----------------------------");
            sb.AppendLine();
          }


          // Campos para las referencias con id="..."
          var idRefs = new List<(string Id, string Type)>();
          if (doc.Root != null)
            CollectIdRefs(doc.Root, idRefs);

          foreach (var (refId, refType) in idRefs)
            sb.AppendLine($"    private {refType} {refId} = null!;");

          if (idRefs.Count > 0)
            sb.AppendLine();

          sb.AppendLine("    partial void OnCreated();");
          sb.AppendLine();

          sb.AppendLine("    public override Component CreateComponent()");
          sb.AppendLine("    {");

          if (doc.Root != null)
          {
            var rootVar = ProcessNode("", doc.Root, sb, tagCounters, compilation, allGeneratedNames, spc, data.FileName);
            sb.AppendLine("        OnCreated();");
            sb.AppendLine($"    return {rootVar};");
          }

          sb.AppendLine("    }");
          sb.AppendLine("}");

          string sourceCode = sb.ToString();
          spc.AddSource($"{data.ClassName}.g.cs", sourceCode);
        }
        catch (XmlException ex)
        {
          var descriptor = new DiagnosticDescriptor(
              id: "SHARP001",
              title: "Sharp Inválido",
              messageFormat: "El archivo '{0}' no es un Sharp válido: {1}",
              category: "CustomFileGenerator",
              defaultSeverity: DiagnosticSeverity.Warning,
              isEnabledByDefault: true);

          spc.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None, data.FileName, ex.Message));
        }
      });
    }

    private static void CollectIdRefs(XElement node, List<(string Id, string Type)> refs)
    {
      var idAttr = node.Attribute("id");
      if (idAttr != null && !string.IsNullOrWhiteSpace(idAttr.Value) && !refs.Any(r => r.Id == idAttr.Value))
        refs.Add((idAttr.Value, node.Name.LocalName));

      foreach (var child in node.Elements())
        CollectIdRefs(child, refs);
    }

    private static string ProcessNode(
        string parent,
        XElement node,
        StringBuilder sb,
        Dictionary<string, int> tagCounter,
        Compilation compilation,
        ImmutableArray<string> allGeneratedNames,
        SourceProductionContext spc,
        string fileName)
    {
      string tagName = node.Name.LocalName;

      // Verificamos si la etiqueta es válida:
      // 1. ¿Es una clase que será generada por otro archivo .sharp en el proyecto?
      bool isGeneratedComponent = allGeneratedNames.Contains(tagName);

      tagCounter.TryGetValue(tagName, out int count);
      count++;
      tagCounter[tagName] = count;

      var id = node.Attribute("id")?.Value;
      string varName = string.IsNullOrWhiteSpace(id) ? $"{tagName}_{count}" : id!;

      if (string.IsNullOrWhiteSpace(id))
        sb.AppendLine($"            var {varName} = new {tagName}();");
      else
        sb.AppendLine($"            {varName} = new {tagName}();");

      // Attributes
      foreach (XAttribute attr in node.Attributes())
      {
        if (attr.Name.LocalName == "id") continue;

        sb.AppendLine($"            // -> Atributo: {attr.Name.LocalName} = {attr.Value}");
        if (attr.Value.StartsWith("#"))
          sb.AppendLine($"            {varName}.{attr.Name.LocalName} = SKColor.Parse(\"{attr.Value}\");");
        else if (attr.Name.LocalName == "backgroundColor")
          sb.AppendLine($"            {varName}.{attr.Name.LocalName} = SKColors.{attr.Value};");
        else if (attr.Name.LocalName == "display")
          sb.AppendLine($"            {varName}.{attr.Name.LocalName} = Display.{attr.Value};");
        else if (attr.Name.LocalName.StartsWith("On", StringComparison.Ordinal))
          sb.AppendLine($"            {varName}.{attr.Name.LocalName}.Bind({attr.Value.GetFormattedAttributeValue()});");
        else
        {

          sb.AppendLine($"            {varName}.{attr.Name.LocalName} = {attr.Value.GetFormattedAttributeValue()};");
        }
      }

      if (!isGeneratedComponent)
      {
        // Append to parent
        if (!string.IsNullOrEmpty(parent))
          sb.AppendLine($"            {parent}.AddChild({varName});");
        else
          sb.AppendLine($"            AddChild({varName});");
      }
      else
      {
        if (!string.IsNullOrEmpty(parent))
          sb.AppendLine($"            {parent}.AddChild({varName}.CreateComponent());");
        else
          sb.AppendLine($"            AddChild({varName}.CreateComponent());");
      }
      // Recursive childs
      foreach (XElement child in node.Elements())
      {
        ProcessNode(varName, child, sb, tagCounter, compilation, allGeneratedNames, spc, fileName);
      }

      return varName;
    }
  }
}