using System;
using System.Collections.Generic;
using System.Reflection;

namespace DaiDai.Entity
{
    public class EntityDefinition
    {
        public Type Type { get; internal set; }

        public string Table { get; internal set; }
        public string Where { get; internal set; }
        public string SetForDelete { get; internal set; }
        public IList<ColumnDefinition> Columns { get; internal set; }
        public ColumnDefinition IdColumn { get; internal set; }
    }

    public class ColumnDefinition
    {
        public PropertyInfo Property { get; internal set; }
        public string Name { get; internal set; }
        public Func<object, object> Getter { get; internal set; }
        public Action<object, object> Setter { get; internal set; }
        public string Convert { get; internal set; }
        public bool Generated { get; internal set; } // w/ [Generated]
        public bool Key { get; internal set; } // w/ [Key]
        public bool Id { get; internal set; }

        public bool Insert => !Generated;
        public bool Update => !Key;
    }
}