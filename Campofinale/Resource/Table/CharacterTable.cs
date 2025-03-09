using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Campofinale.Resource.ResourceManager;

namespace Campofinale.Resource.Table
{
    [TableCfgType("TableCfg/CharacterTable.json", LoadPriority.LOW)]
    public class CharacterTable : TableCfgResource
    {
        public List<Attributes> attributes;
        public string charId;
        public int weaponType;
        public string engName;
        public int rarity;

    }
}
