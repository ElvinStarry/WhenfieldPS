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

            Logger.Print($"[Mission] Player {session.roleId} rolling bloc mission: {req.BlocId}");

            ScRollBlocMission rsp = session.missionSystem.RollBlocMission(req.BlocId);
            if (string.IsNullOrWhiteSpace(rsp.BlocId))
            {
                rsp.BlocId = req.BlocId;
            }

            session.Send(ScMsgId.ScRollBlocMission, rsp, packet.csHead.UpSeqid);
            session.Send(ScMsgId.ScSyncBlocMissionInfo, session.missionSystem.BuildBlocMissionInfo());
        }
    }
}
