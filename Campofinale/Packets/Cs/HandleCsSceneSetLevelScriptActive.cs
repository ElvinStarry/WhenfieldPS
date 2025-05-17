using Campofinale.Game.Character;
using Campofinale.Game.Entities;
using Campofinale.Network;
using Campofinale.Protocol;
using Campofinale.Resource;

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
            Logger.Print(req.Properties.ToString());
            if(req.EventName== "#8777e316")
            {
                session.Send(ScMsgId.ScQuestStateUpdate, new ScQuestStateUpdate()
                {
                    QuestId = "e0m0_q#1",
                    QuestState = (int)QuestState.Completed,
                });
                session.Send(ScMsgId.ScQuestStateUpdate, new ScQuestStateUpdate()
                {
                    QuestId = "e0m0_q#2",
                    QuestState = (int)QuestState.Processing,
                });
            }
            if(req.EventName== "#6ea2690d")
            {
                session.Send(ScMsgId.ScQuestStateUpdate, new ScQuestStateUpdate()
                {
                    QuestId = "e0m0_q#2",
                    QuestState = (int)QuestState.Completed,
                });
                session.Send(ScMsgId.ScQuestStateUpdate, new ScQuestStateUpdate()
                {
                    QuestId = "e0m0_q#3",
                    QuestState = (int)QuestState.Processing,
                });
            }
            if (req.EventName == "#bb79de30")
            {
                session.Send(ScMsgId.ScQuestStateUpdate, new ScQuestStateUpdate()
                {
                    QuestId = "e0m0_q#3",
                    QuestState = (int)QuestState.Completed,
                });
                session.Send(ScMsgId.ScQuestStateUpdate, new ScQuestStateUpdate()
                {
                    QuestId = "e0m0_q#4",
                    QuestState = (int)QuestState.Processing,
                });
            }
            if (req.EventName == "#4c76ec3c")
            {
                session.Send(ScMsgId.ScQuestStateUpdate, new ScQuestStateUpdate()
                {
                    QuestId = "e0m0_q#4",
                    QuestState = (int)QuestState.Completed,
                });
                session.Send(ScMsgId.ScQuestStateUpdate, new ScQuestStateUpdate()
                {
                    QuestId = "e0m0_q#5",
                    QuestState = (int)QuestState.Processing,
                });
            }
            if (req.EventName == "#251df3ad")
            {
                session.Send(ScMsgId.ScQuestStateUpdate, new ScQuestStateUpdate()
                {
                    QuestId = "e0m0_q#5",
                    QuestState = (int)QuestState.Completed,
                });
                session.Send(ScMsgId.ScQuestStateUpdate, new ScQuestStateUpdate()
                {
                    QuestId = "e0m0_q#6",
                    QuestState = (int)QuestState.Processing,
                });
            }
            if (req.EventName == "#e6ac322b")
            {
                session.Send(ScMsgId.ScQuestStateUpdate, new ScQuestStateUpdate()
                {
                    QuestId = "e0m0_q#6",
                    QuestState = (int)QuestState.Completed,
                });
                session.Send(ScMsgId.ScQuestStateUpdate, new ScQuestStateUpdate()
                {
                    QuestId = "e0m0_q#7",
                    QuestState = (int)QuestState.Processing,
                });
            }

            ScSceneUpdateLevelScriptProperty update1 = new()
            {
                SceneNumId = req.SceneNumId,
                ScriptId = req.ScriptId,

            };
            session.Send(ScMsgId.ScSceneUpdateLevelScriptProperty, update1);
            ScSceneTriggerClientLevelScriptEvent trigger = new()
            {
                EventName = req.EventName,
                SceneNumId = req.SceneNumId,
                ScriptId = req.ScriptId
            };
            session.Send(ScMsgId.ScSceneTriggerClientLevelScriptEvent, trigger);
            ScSceneUpdateLevelScriptProperty update2 = new()
            {
                SceneNumId = req.SceneNumId,
                ScriptId = req.ScriptId,

            };
            session.Send(ScMsgId.ScSceneUpdateLevelScriptProperty, update2);
            ScSceneLevelScriptEventTrigger rsp = new ScSceneLevelScriptEventTrigger()
            {
                
            };
            
            session.Send(ScMsgId.ScSceneLevelScriptEventTrigger, rsp,packet.csHead.UpSeqid);

        }
    }
}