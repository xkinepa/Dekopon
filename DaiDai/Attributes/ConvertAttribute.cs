using System;

namespace DaiDai.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ConvertAttribute : Attribute
    {
        public ConvertAttribute()
        {
        }

        public ConvertAttribute(string pattern)
        {
            Pattern = pattern;
        }

        public string Pattern { get; set; }
    }
}