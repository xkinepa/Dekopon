using System;

namespace Daidai.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class WhereAttribute : Attribute
    {
        public string Clause { get; set; }
    }
}