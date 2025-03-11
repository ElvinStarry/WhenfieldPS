using Campofinale.Network;
using Campofinale.Packets.Sc;
using Campofinale.Protocol;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Campofinale.Packets.Cs
{
    public class HandleCsItemBagAbandonInBag
    {

        [Server.Handler(CsMsgId.CsItemBagAbandonInBag)]
        public static void Handle(Player session, CsMsgId cmdId, Packet packet)
        {
            CsItemBagAbandonInBag req = packet.DecodeBody<CsItemBagAbandonInBag>();
            session.inventoryManager.DropItemsBag(req);
        }
       
    }
}
