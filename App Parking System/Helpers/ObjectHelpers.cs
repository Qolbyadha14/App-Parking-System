using System.Reflection;

namespace App_Parking_System.Helpers
{
    public class ObjectHelpers
    {
        public static void Copy<TParent, TChild>(TParent parent, TChild child) where TParent : class where TChild : class
        {
            var cache = new Dictionary<PropertyKey, PropertyInfo>();
            var parentType = parent.GetType();
            var childType = child.GetType();

            foreach (var parentProp in parentType.GetProperties())
            {
                var key = new PropertyKey(parentProp.Name, parentProp.PropertyType);
                if (cache.TryGetValue(key, out var childProp))
                {
                    childProp.SetValue(child, parentProp.GetValue(parent));
                }
                else
                {
                    childProp = childType.GetProperty(parentProp.Name, parentProp.PropertyType);
                    if (childProp != null)
                    {
                        cache[key] = childProp;
                        childProp.SetValue(child, parentProp.GetValue(parent));
                    }
                }
            }
        }

        public static Tresult Convert<TParent, Tresult>(TParent parent) where TParent : class where Tresult : new()
        {
            var cache = new Dictionary<PropertyKey, PropertyInfo>();
            var parentType = parent.GetType();
            var result = new Tresult();

            foreach (var parentProp in parentType.GetProperties())
            {
                var key = new PropertyKey(parentProp.Name, parentProp.PropertyType);
                if (cache.TryGetValue(key, out var resultProp))
                {
                    resultProp.SetValue(result, parentProp.GetValue(parent));
                }
                else
                {
                    resultProp = result.GetType().GetProperty(parentProp.Name, parentProp.PropertyType);
                    if (resultProp != null)
                    {
                        cache[key] = resultProp;
                        resultProp.SetValue(result, parentProp.GetValue(parent));
                    }
                }
            }

            return result;
        }

        class PropertyKey
        {
            public string Name { get; }
            public Type Type { get; }

            public PropertyKey(string name, Type type)
            {
                Name = name;
                Type = type;
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode() ^ Type.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is PropertyKey other)
                {
                    return Name == other.Name && Type == other.Type;
                }
                return false;
            }
        }
    }
}

