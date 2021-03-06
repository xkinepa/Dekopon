﻿using System;

namespace Dekopon.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class KeyAttribute : Attribute
    {
        public bool IsIdentity { get; set; }
    }
}