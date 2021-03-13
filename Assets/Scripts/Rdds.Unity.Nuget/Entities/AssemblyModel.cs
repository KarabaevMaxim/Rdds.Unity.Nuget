using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Rdds.Unity.Nuget.Entities
{
  internal class AssemblyModel
  {
    private const string PrecompiledReferencesName = "precompiledReferences";
    
    public JObject Properties { get; }
    
    public string Name => Properties["name"]!.ToObject<string>()!;
    
    public bool OverrideReferences => Properties["overrideReferences"]!.ToObject<bool>();

    private ObservableCollection<string> _precompiledReferences = null!;
    
    public ObservableCollection<string> PrecompiledReferences
    {
      get => _precompiledReferences;
      private set
      {
        _precompiledReferences = value;
        
        if (!Properties.ContainsKey(PrecompiledReferencesName))
          Properties.Add(PrecompiledReferencesName, JToken.FromObject(value!));
      }
    }

    private JArray JPrecompiledReferences => (JArray) Properties[PrecompiledReferencesName]!;

    public string Path { get; set; } = null!;

    public override bool Equals(object obj) => 
      ReferenceEquals(this, obj) || (obj is AssemblyModel other && Equals(other));
    
    public override int GetHashCode() => Name.GetHashCode();

    private bool Equals(AssemblyModel other) => Name == other.Name;
    
    private void OnPrecompiledReferencesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
      if (args.Action == NotifyCollectionChangedAction.Add)
      {
        JPrecompiledReferences.Add(args.NewItems);
        return;
      }
      
      if (args.Action == NotifyCollectionChangedAction.Remove) 
        JPrecompiledReferences.RemoveAt(args.OldStartingIndex);
    }
    
    public AssemblyModel(JObject properties)
    {
      Properties = properties;
      var references = Properties[PrecompiledReferencesName]?.Select(t => t.Value<string>());
      PrecompiledReferences = references == null 
        ? new ObservableCollection<string>() 
        : new ObservableCollection<string>(references);
      PrecompiledReferences.CollectionChanged += OnPrecompiledReferencesOnCollectionChanged;
    }
  }
}