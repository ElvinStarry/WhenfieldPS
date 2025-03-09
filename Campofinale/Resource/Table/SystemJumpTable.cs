using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campofinale.Resource.Table
{
    [TableCfgType("TableCfg/SystemJumpTable.json", LoadPriority.LOW)]
    public class SystemJumpTable
    {
        public int bindSystem;
    }
}
