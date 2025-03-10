using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campofinale.Resource.Table
{
    [TableCfgType("TableCfg/GiftItemTable.json", LoadPriority.LOW)]
    public class GiftItemTable
    {
        public int favorablePoint;
        public string id;
    }
}
