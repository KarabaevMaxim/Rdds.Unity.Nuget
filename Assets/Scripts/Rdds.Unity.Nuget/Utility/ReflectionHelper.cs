using System;
using System.Reflection;

namespace Rdds.Unity.Nuget.Utility
{
  internal static class ReflectionHelper
  {
    public static PropertyInfo RequireNonPublicProperty(Type type, string propertyName) =>
      type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic) 
      ?? throw new ArgumentOutOfRangeException($"Property {type.Name}.{propertyName} not found");

    public static void SetNonPublicProperty(object obj, string propertyName, object newValue)
    {
      var property = RequireNonPublicProperty(obj.GetType(), propertyName);
      property.SetValue(obj, newValue);
    }
  }
}