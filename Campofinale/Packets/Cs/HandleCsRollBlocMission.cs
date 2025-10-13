using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Cs
{
    internal class HandleCsRollBlocMission
    {
        [Server.Handler(CsMsgId.CsRollBlocMission)]
        public static void Handle(Player session, CsMsgId msgId, Packet packet)
        {
            CsRollBlocMission req = packet.DecodeBody<CsRollBlocMission>();

            Logger.Print($"[Mission] Player {session.roleId} rolling bloc mission: {req.BlocId} (not implemented)");

            // Echo back the request, no actual mission rolled
            ScRollBlocMission rsp = new()
            {
                BlocId = req.BlocId,
                MissionId = "",
                RollCount = 0,
                NextRefreshTine = 0
            };

            session.Send(ScMsgId.ScRollBlocMission, rsp, packet.csHead.UpSeqid);
        }
    }
}
