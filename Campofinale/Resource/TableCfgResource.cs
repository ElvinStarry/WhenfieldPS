using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campofinale.Resource
{
    public abstract class TableCfgResource
    {

        public void OnLoad()
        {

        }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TableCfgTypeAttribute : Attribute
    {
        public string Name { get; }
        public LoadPriority Priority { get; }

        public TableCfgTypeAttribute(string name, LoadPriority priority)
        {
            Name = name;
            Priority = priority;
        }
    }

    public enum LoadPriority
    {
        HIGH,
        MEDIUM,
        LOW
    }
}
