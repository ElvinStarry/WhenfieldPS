using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campofinale.Resource.Table
{
    [TableCfgType("TableCfg/SNSChatTable.json", LoadPriority.LOW)]
    public class SNSChatTable
    {
        public string chatId;
        public int chatType;
        public int tagType;
    }
}
