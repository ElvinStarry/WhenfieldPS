using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Sc
{
    public class PacketScFactoryOpRet : Packet
    {

        public PacketScFactoryOpRet(Player client, uint nodeId,FactoryOpType type) {

            ScFactoryOpRet proto = new ScFactoryOpRet()
            {
                RetCode=FactoryOpRetCode.Ok,
                OpType=type,

            };
            if(type == FactoryOpType.Place)
            {
                proto.Place = new()
                {
                    NodeId = nodeId
                };
                proto.Index = "CHANNLE_BUILDING";
            }
            
            SetData(ScMsgId.ScFactoryOpRet, proto);
        }

    }
}
