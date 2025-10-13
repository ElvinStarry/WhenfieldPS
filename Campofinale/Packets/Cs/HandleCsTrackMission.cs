using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Cs
{
    internal class HandleCsTrackMission
    {
        [Server.Handler(CsMsgId.CsTrackMission)]
        public static void Handle(Player session, CsMsgId msgId, Packet packet)
        {
            CsTrackMission req = packet.DecodeBody<CsTrackMission>();
            session.missionSystem.TrackMission(req.MissionId);
            session.missionSystem.Save();
        }
    }
}
