using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Sc
{
    public class PacketScFactoryOpRet : Packet
    {

        public PacketScFactoryOpRet(Player client, uint val,CsFactoryOp op) {

            ScFactoryOpRet proto = new ScFactoryOpRet()
            {
                RetCode=FactoryOpRetCode.Ok,
                OpType=op.OpType,

            };
            if(op.OpType == FactoryOpType.Place)
            {
                proto.Place = new()
                {
                    NodeId = val
                };
            }
            if (op.OpType == FactoryOpType.MoveNode)
            {
                proto.MoveNode = new()
                {
                    
                };
            }
            if (op.OpType == FactoryOpType.AddConnection)
            {
                proto.AddConnection = new()
                {
                    Index = val,
                };
            }
            if (op.OpType == FactoryOpType.Dismantle)
            {
                proto.Dismantle = new()
                {
                    
                };
            }
            if (op.OpType == FactoryOpType.SetTravelPoleDefaultNext)
            {
                proto.SetTravelPoleDefaultNext = new()
                {
                    
                };
            }
            proto.Index=op.Index;
            SetData(ScMsgId.ScFactoryOpRet, proto);
        }

    }
}
