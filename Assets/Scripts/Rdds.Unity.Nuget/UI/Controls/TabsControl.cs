using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Rdds.Unity.Nuget.UI.Controls
{
  internal class TabsControl
  {
    private readonly VisualElement _tabsButtonsBlock;
    private readonly VisualElement _tabsContainer;

    private readonly List<TabControl> _tabs;
    private int? _selectedTabIndex;

    private void AddTab(TabControl tabControl)
    {
      _tabs.Add(tabControl);
      var index = _tabs.Count - 1;
      _tabsButtonsBlock.Add(tabControl.Button);
      tabControl.Button.clickable.clicked += () => SelectTab(index);
    }

    private void SelectTab(int index)
    {
      if (_selectedTabIndex == index)
        return;

      if (_selectedTabIndex.HasValue)
      {
        _tabsContainer.Remove(_tabs[_selectedTabIndex.Value].TabRoot);
        _tabs[_selectedTabIndex.Value].Selected = false;
      }

      _tabsContainer.Add(_tabs[index].TabRoot);
      _tabs[index].Selected = true;
      _selectedTabIndex = index;
    }
    
    public TabsControl(VisualElement controlRoot, params TabControl[] tabs)
    {
      _tabsButtonsBlock = controlRoot.Q<VisualElement>("TabsControlBlock");
      _tabsContainer = controlRoot.Q<VisualElement>("TabsContainer");
      _tabs = new List<TabControl>(tabs.Length);
      
      foreach (var tab in tabs) 
        AddTab(tab);
      
      SelectTab(0);
    }
  }
}