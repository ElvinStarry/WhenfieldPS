using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Cs
{
    internal class HandleCsStopTrackingMission
    {
        [Server.Handler(CsMsgId.CsStopTrackingMission)]
        public static void Handle(Player session, CsMsgId msgId, Packet packet)
        {
            CsStopTrackingMission req = packet.DecodeBody<CsStopTrackingMission>();

            Logger.Print($"[Mission] Player {session.roleId} stopping mission tracking");

            // Stop tracking by setting empty mission ID
            session.missionSystem.TrackMission("");
            session.missionSystem.Save();
        }
    }
}
