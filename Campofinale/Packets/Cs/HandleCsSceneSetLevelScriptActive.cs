using Campofinale.Game.Character;
using Campofinale.Game.Entities;
using Campofinale.Network;
using Campofinale.Protocol;
using Campofinale.Resource;
using Campofinale.Resource.Table;
using Pastel;
using static Campofinale.Resource.ResourceManager.LevelScene.LevelData;

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
                
                session.Send(ScMsgId.ScSceneLevelScriptStateNotify, rsp,packet.csHead.UpSeqid);
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
                case ScriptActionType.UnlockSystem:
                    UnlockSystemType type = (UnlockSystemType)Enum.Parse(typeof(UnlockSystemType), action.valueStr[0]);
                    player.UnlockSystem(type);
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

            ScSceneUpdateLevelScriptProperty update1 = new()
            {
                SceneNumId = req.SceneNumId,
                ScriptId = req.ScriptId,
               
            };
            LevelScriptData levelscript= ResourceManager.GetLevelData(session.curSceneNumId).levelData.levelScripts.Find(l=>l.scriptId == req.ScriptId);
            if (levelscript != null) {
                foreach (var item in req.Properties)
                {
                    int key = levelscript.GetPropertyId(item.Key, new List<int>());
                    update1.Properties.Add(key, item.Value);
                }
            }
            session.Send(ScMsgId.ScSceneUpdateLevelScriptProperty, update1);
            /*ScSceneTriggerClientLevelScriptEvent trigger = new()
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
            session.Send(ScMsgId.ScSceneUpdateLevelScriptProperty, update2);*/
            ScSceneLevelScriptEventTrigger rsp = new ScSceneLevelScriptEventTrigger()
            {
                
            };
            
            session.Send(ScMsgId.ScSceneLevelScriptEventTrigger, rsp,packet.csHead.UpSeqid);

        }
    }
}