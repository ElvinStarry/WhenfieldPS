using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Cs
{
    internal class HandleCsMissionEventTrigger
    {
        [Server.Handler(CsMsgId.CsMissionEventTrigger)]
        public static void Handle(Player session, CsMsgId msgId, Packet packet)
        {
            CsMissionEventTrigger req = packet.DecodeBody<CsMissionEventTrigger>();

            Logger.Print($"[Mission] Event triggered - Mission: {req.MissionId}, Event: {req.EventName}");

            // Echo back to client
            ScMissionEventTrigger rsp = new()
            {
                MissionId = req.MissionId,
                EventName = req.EventName
            };
            session.Send(ScMsgId.ScMissionEventTrigger, rsp, packet.csHead.UpSeqid);

            // TODO: Implement event-based quest progression logic
            // This could trigger quest updates, unlock new quests, etc.
        }
    }
}
