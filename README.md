# SharpUI

SharpUI es un framework de UI **declarativa** para .NET, con una sintaxis de componentes inspirada en Astro/JSX. El dibujo se hace con **SkiaSharp** y la ventana nativa se crea con **Silk.NET**.

Los archivos `.sharp` combinan un *frontmatter* en C# con marcado XML. Un **source generator** los compila a clases `SharpRenderComponent` que construyen el árbol de componentes en tiempo de ejecución.

```xml
---
private readonly Signal<int> count = new(0);
public void Increment() => count.Value++;
---
<Flex display="Column" spacing=12 padding=20>
  <Label text="Contador" fontSize=24 />
  <Button text="+" OnClick={Increment} />
</Flex>
```

> Estado: proyecto en desarrollo, API en evolución.

---

## Módulos

El repositorio está dividido en módulos independientes (cada uno es un `.csproj`):

| Proyecto | Descripción |
| --- | --- |
| `src/SharpUI.Core` | Núcleo: `Component`, widgets (`View`, `Flex`, `Label`, `Button`), layout, painters, señales (`Signal`, `Event`) y contratos de tema (`ITheme`). Depende de SkiaSharp. |
| `src/SharpUI.Reactive` | Primitivas reactivas de orden superior: `Computed<T>` y `Effect`. Depende de Core. |
| `src/SharpUI.Theme` | Tema concreto (`Theme`, `Themes.Light/Dark`) y `ThemeProvider`. Depende de Core. |
| `src/SharpUI.Windowing` | Ventana y host de render (`SharpUIWindow`). Depende de Core, Silk.NET y SkiaSharp. |
| `src/Generators` | Source generator (`netstandard2.0`) que compila los `.sharp`. |
| `samples/Sample` | App de ejemplo que usa todo. |

Dependencias:

```
SharpUI.Core
├── SharpUI.Reactive
├── SharpUI.Theme
└── SharpUI.Windowing
```

---

## Requisitos

- **.NET 10** SDK
- SkiaSharp y Silk.NET se resuelven por NuGet (referencias de proyecto o paquetes).

---

## Empezar rápido

1. Referenciar los módulos y el generador en tu `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\src\SharpUI.Core\SharpUI.Core.csproj" />
  <ProjectReference Include="..\src\SharpUI.Reactive\SharpUI.Reactive.csproj" />
  <ProjectReference Include="..\src\SharpUI.Theme\SharpUI.Theme.csproj" />
  <ProjectReference Include="..\src\SharpUI.Windowing\SharpUI.Windowing.csproj" />

  <!-- El generador se usa como analyzer -->
  <ProjectReference Include="..\src\Generators\SharpUI.Generators.csproj"
                    OutputItemType="Analyzer" ReferenceOutputAssembly="false" />

  <!-- Los archivos .sharp entran como AdditionalFiles -->
  <AdditionalFiles Include="**/*.sharp" Watch="true" />
</ItemGroup>
```

2. Crear la ventana:

```csharp
using SharpUI.Windowing;

var window = new SharpUIWindow(900, 600, "Mi app");
window.Run(new App()); // App se genera desde App.sharp
```

3. Escribir componentes en `.sharp` (ver más abajo).

---

## Sintaxis `.sharp`

Un archivo `.sharp` tiene dos partes opcionales separadas por `---`:

- **Frontmatter** (arriba): código C# que se inyecta dentro de la clase generada.
- **Marcado** (abajo): XML que describe el árbol de componentes.

### Frontmatter

```xml
---
private readonly Signal<int> count = new(0);

public void Increment() => count.Value++;

public string Section { get; set; } = "Menú"; // se puede setear desde el padre
---
```

La clase generada es `partial`, así que podés sumar código en tu propio archivo (`partial class`), hooks, etc.

### Marcado

```xml
<Flex display="Column" spacing=12 padding=20 flexGrow="1">
  <Label text="Hola" fontSize=18 />
  <Button text="Aceptar" OnClick={Aceptar} />
</Flex>
```

Detalles:

- Los nombres de archivo definen el nombre de la clase (`Sidebar.sharp` → `class Sidebar`).
- Un tag puede ser cualquier widget de C# o **otro componente `.sharp`** del proyecto (se instancia y se agrega su `CreateComponent()`).

### Atributos

| Atributo | Tipo | Notas |
| --- | --- | --- |
| `display` | `Column` / `Row` | En contenedores `Flex`. |
| `spacing` | `float` | Separación entre hijos. |
| `padding` | `float` | Padding interno del contenedor. |
| `stretchChildren` | `bool` | Estira hijos en el eje cruzado (por defecto `true`). |
| `flexGrow` | `float` | Cuánto crece para ocupar el espacio libre (por defecto `0`). |
| `width` / `height` | `float` | Tamaño explícito (gana sobre el layout). |
| `backgroundColor`, `borderColor` | color | `#RRGGBB` o `SKColors.*`. |
| `borderWidth`, `cornerRadius` | `float` | |
| `text` | `string` | En `Label` y `Button`. |
| `fontSize` | `float` | En `Label`. |
| `textColor` | color | `#RRGGBB` o `SKColors.*`. |
| `OnClick={Metodo}` | handler | Enlaza el evento a un método del frontmatter. |

Convención de nombres:

- Propiedades declarativas en **camelCase** (`display`, `spacing`, `flexGrow`, `text`).
- Señales y eventos en **PascalCase** (`OnClick={funcion}`).
- Props internas de runtime en **PascalCase** (`Rect`, `Children`, `Host`).

Colores: cualquier atributo que empiece con `#` se parsea con `SKColor.Parse`. El valor especial `backgroundColor=Red` usa `SKColors.Red`.

### Valores

Todo lo que no sea color, `display` u `On*` se emite como expresión C#:

```xml
<Label text={Section} fontSize={baseSize + 2} />
```

> Nota: `text={expr}` se evalúa **una sola vez**, al construir el árbol (no es un binding reactivo). Para reactividad usá señales + hooks (ver abajo).

### Referencias con `id`

Si ponés `id`, el generador crea un campo con ese nombre, accesible desde el frontmatter:

```xml
<Label id="value" text="Valor: 0" />
```

```csharp
partial void OnCreated()
{
    value.text = "cambiado desde código";
}
```

---

## Reactividad

Las primitivas base viven en **Core** (`SharpUI.Core.Signals`):

- **`Signal<T>`**: valor observable. Al asignar `Value` notifica a los suscriptores solo si cambió. Los widgets exponen señales y propiedades camelCase que las envuelven (p. ej. `Label.Text` ↔ `label.text`).
- **`Event`**: handler múltiple; en markup se enlaza con `OnClick={Metodo}`.

El módulo **Reactive** agrega:

- **`Effect`**: reacciona a las señales que lee y se re-ejecuta cuando cambian.
- **`Computed<T>`**: señal derivada que se recalcula automáticamente.

Ejemplo típico (contador):

```xml
---
private readonly Signal<int> count = new(0);
public void Increment() => count.Value++;

partial void OnCreated()
{
    new Effect(() =>
    {
        value.text = $"Valor: {count.Value}";
        value.textColor = count.Value > 0 ? SKColors.LimeGreen : SKColors.White;
    });
}
---
<Flex display="Column" spacing=12>
  <Label id="value" text="Valor: 0" fontSize=40 />
  <Button text="+" OnClick={Increment} />
</Flex>
```

> `Effect` es `IDisposable`. Si creás efectos/`Computed` de vida larga, guardalos y disponelos cuando corresponda.

---

## Tema

El tema se resuelve **por árbol**: cada componente busca hacia arriba un `IThemeProvider`; si no encuentra ninguno, usa `DefaultTheme`.

```xml
<ThemeProvider theme={Themes.Dark()}>
  <Flex display="Column" padding=20>
    ...
  </Flex>
</ThemeProvider>
```

- `ITheme` (Core): `background`, `foreground`, `accent`, `fontSize`, `cornerRadius`.
- `DefaultTheme` (Core): tema usado si no hay proveedor.
- `Theme` / `Themes.Light()` / `Themes.Dark()` y `ThemeProvider` viven en **SharpUI.Theme**.

El `background`/`cornerRadius` del tema se aplican solo a la **raíz** del árbol (similar a `body` en CSS).

---

## Painters

Cada widget delega su dibujo en un `IPainter<T>` (`ViewPainter`, `LabelPainter`, `ButtonPainter`). La propiedad `painter` es reemplazable, así que el tema (o tu app) puede cambiar el aspecto sin tocar el widget:

```csharp
button.painter = new MiBotonPainter();
```

---

## Ejemplo

El proyecto `samples/Sample` es la app de referencia:

- `App.sharp` — composición raíz (`ThemeProvider` → header + sidebar + contenido + contador).
- `Sidebar.sharp` — componente con propiedad de frontmatter (`Section`) y botones.
- `CounterPanel.sharp` — estado con `Signal<int>`, `OnClick={...}` y un `Effect` en `OnCreated`.
- `Program.cs` — crea el `SharpUIWindow` y corre `App`.

Para ejecutarlo:

```bash
dotnet run --project samples/Sample
```

---

## Limitaciones actuales

- Los `.sharp` requieren que el generador esté referenciado como **analyzer** y los archivos como **`AdditionalFiles`**.
- `OnClick={...}` espera un **método con nombre**; no soporta lambdas en el markup (usá un método del frontmatter).
- `text={expr}` y demás valores son **estáticos**: se evalúan al construir el árbol. La reactividad se hace con señales, `Effect`, `Computed` y refs por `id`.
- Aún no se genera un hook `OnDispose`; los recursos de vida larga (efectos) hay que disponerlos a mano.
- `SharpUIWindow` requiere un entorno con OpenGL (no está pensado para headless).

---

## Empaquetado (pendiente)

La idea es publicar los módulos como paquetes NuGet (`SharpUI.Core`, `SharpUI.Reactive`, `SharpUI.Theme`, `SharpUI.Windowing`) más un meta paquete `SharpUI` que incluya el generador y auto-registre los `**/*.sharp`. Todavía no implementado.
