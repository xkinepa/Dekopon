using System;

namespace Dekopon.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GeneratedAttribute : Attribute
    {
    }
}