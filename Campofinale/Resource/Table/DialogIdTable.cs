using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Campofinale.Resource.ResourceManager;

namespace Campofinale.Resource.Table
{
    [TableCfgType("Json/GameplayConfig/DialogIdTable.json", LoadPriority.LOW)]
    public class DialogIdTable : StrIdNumTable
    {
    }
}
