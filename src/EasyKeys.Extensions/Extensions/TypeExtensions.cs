using System.Linq;

namespace System
{
    public static class TypeExtensions
    {
        public static string GetGenericTypeName(this Type type)
        {
            if (type.IsGenericType)
            {
                var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
                return $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
            }
            else
            {
                return type.Name;
            }
        }

        public static string GetGenericTypeName(this object atObject)
        {
            return atObject.GetType().GetGenericTypeName();
        }
    }
}
