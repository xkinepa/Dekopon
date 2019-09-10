using System;

namespace Daidai.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GeneratedAttribute : Attribute
    {
    }
}