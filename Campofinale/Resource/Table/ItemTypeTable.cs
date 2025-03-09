using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campofinale.Resource.Table
{
    [TableCfgType("TableCfg/ItemTypeTable.json", LoadPriority.LOW)]
    public class ItemTypeTable
    {
        public int itemType;
        public ItemStorageSpace storageSpace;
    }
}
