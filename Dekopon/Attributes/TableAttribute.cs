using System;

namespace Dekopon.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TableAttribute : Attribute
    {
        public TableAttribute()
        {
        }

        public TableAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}