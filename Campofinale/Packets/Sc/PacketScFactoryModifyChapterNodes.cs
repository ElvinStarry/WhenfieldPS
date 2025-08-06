using Campofinale.Game.Factory;
using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Sc
{
    public class PacketScFactoryModifyChapterNodes : Packet
    {

        public PacketScFactoryModifyChapterNodes(Player client,string chapterId,FactoryNode node) {
            ScFactoryModifyChapterNodes edit = new()
            {
                ChapterId = chapterId,
                Tms = DateTime.UtcNow.ToUnixTimestampMilliseconds()/1000,
                Nodes =
                {
                    node.ToProto()
                }
            };
            SetData(ScMsgId.ScFactoryModifyChapterNodes, edit);
        }
        public PacketScFactoryModifyChapterNodes(Player client, string chapterId, uint nodeId)
        {
            ScFactoryModifyChapterNodes edit = new()
            {
                ChapterId = chapterId,
                Tms = DateTime.UtcNow.ToUnixTimestampMilliseconds() / 1000,
                RemoveNodes =
                {
                    nodeId
                }
            };
            SetData(ScMsgId.ScFactoryModifyChapterNodes, edit);
        }

    }
}
