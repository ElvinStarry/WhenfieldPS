using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Cs
{
    internal class HandleCsSetNewMissionTagDone
    {
        [Server.Handler(CsMsgId.CsSetNewMissionTagDone)]
        public static void Handle(Player session, CsMsgId msgId, Packet packet)
        {
            CsSetNewMissionTagDone req = packet.DecodeBody<CsSetNewMissionTagDone>();

            Logger.Print($"[Mission] New mission tags marked as read: {string.Join(", ", req.NewMissionTags)}");

            // No corresponding protocol in Sc, ignore
        }
    }
}
