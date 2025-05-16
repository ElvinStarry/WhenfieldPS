using Campofinale.Game.Character;
using Campofinale.Game.Entities;
using Campofinale.Network;
using Campofinale.Protocol;

namespace Campofinale.Packets.Cs
{
    public class HandleCsSceneSetLevelScriptActive
    {
        [Server.Handler(CsMsgId.CsSceneSetLevelScriptActive)]
        public static void Handle(Player session, CsMsgId cmdId, Packet packet)
        {
            CsSceneSetLevelScriptActive req = packet.DecodeBody<CsSceneSetLevelScriptActive>();

            ScSceneLevelScriptStateNotify rsp = new ScSceneLevelScriptStateNotify()
            {
                SceneNumId = req.SceneNumId,
                ScriptId = req.ScriptId,
                State = 3
            };
            session.Send(ScMsgId.ScSceneLevelScriptStateNotify, rsp);

        }

        [Server.Handler(CsMsgId.CsSceneSetLevelScriptStart)]
        public static void HandleCsSceneSetLevelScriptStart(Player session, CsMsgId cmdId, Packet packet)
        {
            CsSceneSetLevelScriptStart req = packet.DecodeBody<CsSceneSetLevelScriptStart>();
            ScSceneLevelScriptStateNotify rsp = new ScSceneLevelScriptStateNotify()
            {
                SceneNumId = req.SceneNumId,
                ScriptId = req.ScriptId,
                State = 4
            };
            session.Send(ScMsgId.ScSceneLevelScriptStateNotify, rsp);

        }
        
            [Server.Handler(CsMsgId.CsSceneLevelScriptEventTrigger)]
        public static void HandleCsSceneLevelScriptEventTrigger(Player session, CsMsgId cmdId, Packet packet)
        {
            CsSceneLevelScriptEventTrigger req = packet.DecodeBody<CsSceneLevelScriptEventTrigger>();
            
            ScSceneLevelScriptEventTrigger rsp = new ScSceneLevelScriptEventTrigger()
            {
                
            };
            
            session.Send(ScMsgId.ScSceneLevelScriptEventTrigger, rsp,packet.csHead.UpSeqid);

        }
    }
}