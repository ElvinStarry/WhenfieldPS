using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Cs
{
    public class HandleCsFactoryHsFb
    {

        [Server.Handler(CsMsgId.CsFactoryHsFb)]
        public static void Handle(Player session, CsMsgId cmdId, Packet packet)
        {
            CsFactoryHsFb req = packet.DecodeBody<CsFactoryHsFb>();
            long curtimestamp = DateTime.UtcNow.ToUnixTimestampMilliseconds();

            ScFactoryHs hs = new()
            {


            };
            session.Send(ScMsgId.ScFactoryHs, hs);
            
        }
       
    }
}
