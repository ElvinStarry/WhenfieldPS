using Campofinale.Game.Character;
using Campofinale.Game.Entities;
using Campofinale.Network;
using Campofinale.Protocol;
using Campofinale.Resource;
using Campofinale.Resource.Table;
using Pastel;

namespace Campofinale.Packets.Cs
{
    public class HandleCsSceneSetLevelScriptActive
    {
        [Server.Handler(CsMsgId.CsSceneSetLevelScriptActive)]
        public static void Handle(Player session, CsMsgId cmdId, Packet packet)
        {
            CsSceneSetLevelScriptActive req = packet.DecodeBody<CsSceneSetLevelScriptActive>();
            if (req.IsActive)
            {

                ScSceneLevelScriptStateNotify rsp = new ScSceneLevelScriptStateNotify()
                {
                    SceneNumId = req.SceneNumId,
                    ScriptId = req.ScriptId,

                    State = 3
                };
                session.Send(ScMsgId.ScSceneLevelScriptStateNotify, rsp);
            }
            

        }

        [Server.Handler(CsMsgId.CsSceneSetLevelScriptStart)]
        public static void HandleCsSceneSetLevelScriptStart(Player session, CsMsgId cmdId, Packet packet)
        {
            CsSceneSetLevelScriptStart req = packet.DecodeBody<CsSceneSetLevelScriptStart>();
            if (req.IsStart)
            {
                ScSceneLevelScriptStateNotify rsp = new ScSceneLevelScriptStateNotify()
                {
                    SceneNumId = req.SceneNumId,
                    ScriptId = req.ScriptId,

                    State = 4
                };
                session.Send(ScMsgId.ScSceneLevelScriptStateNotify, rsp);
            }
           

        }
        
        public static void ExecuteEventAction(Player player, ScriptAction action)
        {
            switch (action.action)
            {
                case ScriptActionType.CompleteQuest:
                    player.missionSystem.CompleteQuest(action.valueStr[0]);
                    break;
                case ScriptActionType.ProcessQuest:
                    player.missionSystem.ProcessQuest(action.valueStr[0]);
                    break;
                case ScriptActionType.SpawnEnemy:
                    player.sceneManager.GetCurScene().SpawnEnemy(action.valueUlong[0]);
                    break;
                default:
                    Logger.PrintWarn("Script Action not implemented");
                    break;
            }
        }
        [Server.Handler(CsMsgId.CsSceneLevelScriptEventTrigger)]
        public static void HandleCsSceneLevelScriptEventTrigger(Player session, CsMsgId cmdId, Packet packet)
        {
            
            CsSceneLevelScriptEventTrigger req = packet.DecodeBody<CsSceneLevelScriptEventTrigger>();
            Logger.Print(req.Properties.ToString());

            if (ResourceManager.levelScriptsEvents.TryGetValue(req.EventName, out LevelScriptEvent levelScriptEvent))
            {
                Logger.Print($"Event {req.EventName.Pastel(ConsoleColor.Yellow)} Executed.");
                Logger.Print($"{levelScriptEvent.comment}");
                levelScriptEvent.actions.ForEach(a =>
                {
                    ExecuteEventAction(session, a);
                });
            }
            else
            {
                Logger.PrintWarn($"Event {req.EventName.Pastel(ConsoleColor.White)} is NOT implemented. INFO: [");
                Logger.PrintWarn($" Scene: {req.SceneNumId.ToString().Pastel(ConsoleColor.White)}, Pos: {session.position.ToProto().ToString()} ");
                Logger.PrintWarn($" ScriptID: {req.ScriptId.ToString().Pastel(ConsoleColor.White)} ");
                Logger.PrintWarn($"]");
            }
            /*if(req.EventName== "#8777e316")
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
            }*/

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
                ScriptId = req.ScriptId,
                
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