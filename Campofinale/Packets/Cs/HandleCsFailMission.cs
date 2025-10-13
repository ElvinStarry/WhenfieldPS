using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Cs
{
    internal class HandleCsFailMission
    {
        [Server.Handler(CsMsgId.CsFailMission)]
        public static void Handle(Player session, CsMsgId msgId, Packet packet)
        {
            CsFailMission req = packet.DecodeBody<CsFailMission>();

            Logger.Print($"[Mission] Player {session.roleId} failing mission: {req.MissionId}");

            // Call FailMission method (needs to be implemented in MissionSystem)
            session.missionSystem.FailMission(req.MissionId);
            session.missionSystem.Save();
        }
    }
}
