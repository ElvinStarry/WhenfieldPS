using Campofinale.Network;
using Campofinale.Protocol;
using Campofinale.Resource;

namespace Campofinale.Packets.Sc
{
    public class PacketScSyncAllUnlock : Packet
    {

        public PacketScSyncAllUnlock(Player client) {

            List<int> toBlock=new List<int>()
            {
                (int)UnlockSystemType.Activity,
                (int)UnlockSystemType.DomainShop,
                (int)UnlockSystemType.Friend

            };
            ScSyncAllUnlock unlock = new()
            {
                UnlockSystems = {client.unlockedSystems.Except(toBlock)},
                
            };
            
            SetData(ScMsgId.ScSyncAllUnlock, unlock);
        }

    }
}
