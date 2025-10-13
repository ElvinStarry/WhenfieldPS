using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Cs
{
    internal class HandleCsMissionClientTriggerDone
    {
        [Server.Handler(CsMsgId.CsMissionClientTriggerDone)]
        public static void Handle(Player session, CsMsgId msgId, Packet packet)
        {
            CsMissionClientTriggerDone req = packet.DecodeBody<CsMissionClientTriggerDone>();

            Logger.Print($"[Mission] Client trigger done - Mission: {req.MissionId}, Scene: {req.SceneName}");

            // Acknowledge client trigger completion
            ScMissionClientTriggerDone rsp = new()
            {
                MissionId = req.MissionId,
                SceneName = req.SceneName
            };
            session.Send(ScMsgId.ScMissionClientTriggerDone, rsp, packet.csHead.UpSeqid);
        }
    }
}
