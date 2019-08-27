using System;

namespace DaiDai.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GeneratedAttribute : Attribute
    {
    }
}