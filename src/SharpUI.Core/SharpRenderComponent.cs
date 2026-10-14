namespace SharpUI.Core;

public abstract class SharpRenderComponent
{
  public SharpRenderComponent? Parent;

  public List<SharpRenderComponent> SharpChildrens = [];
  public List<Component> Childrens = [];

  public abstract Component CreateComponent();

  public void AddChild(Component component)
  {
    Childrens.Add(component);
  }

  public void AddChild(SharpRenderComponent component)
  {
    SharpChildrens.Add(component);
  }

  public Component Build()
  {
    var main = CreateComponent();
    foreach (var child in SharpChildrens)
    {
      main.AddChild(child.CreateComponent());
    }

    //foreach (var child in Childrens)
    //{
    //  main.AddChild(child);
    //}
    return main;
  }
}
