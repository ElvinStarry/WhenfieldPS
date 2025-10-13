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

            // No corresponding protocol in Sc, ignore
        }
    }
}
