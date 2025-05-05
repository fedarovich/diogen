namespace Diogen.Generators;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class KeyedAttribute(object key) : Attribute
{
    public object Key { get; } = key;
}
