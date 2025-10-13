using Campofinale.Network;
using Campofinale.Protocol;
using Campofinale.Resource;

namespace Campofinale.Packets.Cs
{
    internal class HandleCsAcceptMission
    {
        [Server.Handler(CsMsgId.CsAcceptMission)]
        public static void Handle(Player session, CsMsgId msgId, Packet packet)
        {
            CsAcceptMission req = packet.DecodeBody<CsAcceptMission>();

            Logger.Print($"[Mission] Player {session.roleId} accepting mission: {req.MissionId}");

            // Add mission with Processing state and notify client
            session.missionSystem.AddMission(req.MissionId, MissionState.Processing, notify: true);
            session.missionSystem.Save();
        }
    }
}
