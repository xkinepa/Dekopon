using System;

namespace Daidai.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DeleteAttribute : Attribute
    {
        public string Set { get; set; }
    }
}