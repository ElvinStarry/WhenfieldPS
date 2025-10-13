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

            // Mark new mission tags as read (for red dot notification system)
            // This could be stored in player data if needed for persistence

            // Acknowledge to client
            ScSetNewMissionTagDone rsp = new();
            rsp.NewMissionTags.AddRange(req.NewMissionTags);

            session.Send(ScMsgId.ScSetNewMissionTagDone, rsp, packet.csHead.UpSeqid);
        }
    }
}
