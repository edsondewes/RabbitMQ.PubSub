using System.Reflection;

namespace JaegerTracingWeb.Diagnostics
{
    public static class PropertyExtensions
    {
        public static object GetProperty(this object @this, string propertyName)
        {
            return @this.GetType().GetTypeInfo().GetDeclaredProperty(propertyName).GetValue(@this);
        }
    }
}
