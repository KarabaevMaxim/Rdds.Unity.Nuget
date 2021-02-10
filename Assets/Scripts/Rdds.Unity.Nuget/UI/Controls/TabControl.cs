using UnityEngine.UIElements;

namespace Rdds.Unity.Nuget.UI.Controls
{
  internal class TabControl
  {
    private bool _selected;

    public bool Selected
    {
      get => _selected;
      set
      {
        if (_selected == value)
          return;
        
        _selected = value;

        if (_selected)
        {
          Button.AddToClassList("OpenedTabButton");
          OnSelected();
        }
        else
        {
          Button.RemoveFromClassList("OpenedTabButton");
          OnDeselected();
        }
      }
    }
    
    public Button Button { get; }
    
    public VisualElement TabRoot { get; }
    
    private string Title
    {
      get => Button.text;
      set => Button.text = value;
    }
    
    protected virtual void OnSelected()
    {
    }
    
    protected virtual void OnDeselected()
    {
    }
    
    public TabControl(VisualElement tabRoot, string title)
    {
      Button = new Button();
      Button.AddToClassList("StackExpandedElement");
      Button.AddToClassList("TabButton");
      Title = title;
      TabRoot = tabRoot;
    }
  }
}