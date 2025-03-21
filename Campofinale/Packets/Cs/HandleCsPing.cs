using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Cs
{
    public class HandleCsPing
    {

        [Server.Handler(CsMsgId.CsPing)]
        public static void Handle(Player session, CsMsgId cmdId, Packet packet)
        {
            CsPing req = packet.DecodeBody<CsPing>();
            long curtimestamp = DateTime.UtcNow.ToUnixTimestampMilliseconds();

            session.Send(Packet.EncodePacket((int)ScMsgId.ScPing, new ScPing()
            {
                ClientTs = req.ClientTs,
                ServerTs = (ulong)curtimestamp,
            }));
            /*ScFactoryHsSync s = new()
            {
                Blackboard = new()
                {
                    InventoryNodeId = 0,
                    Power = new()
                    {
                        
                    }
                },
                CcList =
                {

                },
                Tms = curtimestamp / 1000,
                ChapterId = session.GetCurrentChapter()
            };
            
            session.Send(ScMessageId.ScFactoryHsSync,s);*/

            //Logger.Print("Server: " + curtimestamp + " client: " + req.ClientTs);
        }
       
    }
}
