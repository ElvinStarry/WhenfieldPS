using Campofinale.Game;
using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Sc
{
    public class PacketScFriendListSimpleSync : Packet
    {

        public PacketScFriendListSimpleSync(Player client) {

            ScFriendListSimpleSync proto = new ScFriendListSimpleSync()
            {
                FriendList =
                {
                    new ScdFriendFriendSimpleInfo()
                    {
                        AdventureLevel=1,
                        Name="Campofinale",
                        Online=true,    
                        RoleId=(ulong)GameConstants.SERVER_UID.Item1,
                        Signature="Campofinale Console",
                        
                    }
                }
                
            };
           
            SetData(ScMsgId.ScFriendListSimpleSync, proto);
        }

    }
}
