using System;

namespace DaiDai.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class WhereAttribute : Attribute
    {
        public string Clause { get; set; }
    }
}