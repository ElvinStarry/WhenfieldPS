using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campofinale.Resource.Table
{
    [TableCfgType("Json/Interactive/InteractiveTable.json", LoadPriority.LOW)]
    public class InteractiveTable : TableCfgResource
    {
        public Dictionary<string, InteractiveTemplate> interactiveDataDict = new();

        public class InteractiveTemplate
        {
            public string templateId;
        }
    }
}
