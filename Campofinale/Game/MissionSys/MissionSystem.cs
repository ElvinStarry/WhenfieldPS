using Campofinale.Database;
using Campofinale.Protocol;
using Campofinale.Resource;
using Campofinale.Resource.Table;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Campofinale.Game.MissionSys
{
    public class MissionSystem
    {
        public Player owner;
        public List<GameMission> missions=new();
        public List<GameQuest> quests=new();
        public string curMission = "e0m0";

        public MissionSystem(Player o)
        {
            owner = o;
        }
        public ScSyncAllMission ToProto()
        {
            ScSyncAllMission sync = new ScSyncAllMission();
            sync.TrackMissionId = curMission;
            missions.ForEach(m =>
            {
                sync.Missions.Add(m.missionId, new Mission()
                {
                    MissionId = m.missionId,
                    MissionState = (int)m.state,
                    
                });
            });
            
            quests.ForEach(q =>
            {
                Quest quest=new Quest()
                {
                    QuestId = q.questId,
                    QuestState = (int)q.state,

                };
                var data = GetQuestData(q.questId);
                data.objectiveList.ForEach(o =>
                {
                    quest.QuestObjectives.Add(new QuestObjective()
                    {
                        ConditionId = o.condition.uniqueId,
                        Values =
                        {
                            {o.condition.uniqueId,0 }
                        }
                    });
                });
                sync.CurQuests.Add(q.questId, quest);
                
            });
            return sync;
        }
        public MissionDataTable.QuestInfo GetQuestData(string id)
        {
            MissionDataTable.QuestInfo quest = null;
            foreach(MissionDataTable m in ResourceManager.missionDataTable)
            {
                if(m.questDic.TryGetValue(id, out quest))
                {
                    return quest;
                }
            };

            return quest;
        }
        public void Save()
        {

        }
        public void Load()
        {
            //TODO Saving and first initialization
            AddMission("e0m0",MissionState.Processing);
        }
        public void AddMission(string id,MissionState state = MissionState.Available, bool notify=false)
        {
            MissionDataTable data = ResourceManager.missionDataTable.Find(m=>m.missionId == id);
            if (data != null)
            {
                missions.Add(new GameMission(id, state));
                if (notify)
                {
                    ScMissionStateUpdate s = new()
                    {
                        MissionId = data.missionId,
                        MissionState = (int)state,
                        SucceedId=-1,

                    };
                }
                
                int i = 0;
                foreach (var q in data.questDic.Values)
                {
                    AddQuest(q, false);
                }
            }
        }
        public GameQuest GetQuestById(string id)
        {
            return quests.Find(q => q.questId == id);
        }
        public void AddQuest(MissionDataTable.QuestInfo data,bool notify=false)
        {
            GameQuest quest = GetQuestById(data.questId);
            if (quest == null)
            {
                quest = new GameQuest(data.questId);
                quest.state = QuestState.Available;
                if (notify)
                {
                    ScQuestObjectivesUpdate upd = new()
                    {
                        QuestId = data.questId,
                        
                    };
                    data.objectiveList.ForEach(o =>
                    {
                        upd.QuestObjectives.Add(new QuestObjective()
                        {
                            ConditionId=o.condition.uniqueId,
                            Values =
                            {
                                {o.condition.uniqueId,0 }
                            }
                        });
                    });
                    ScQuestStateUpdate update = new()
                    {
                        QuestId = quest.questId,
                        QuestState = (int)quest.state,
                        RoleBaseInfo = owner.GetRoleBaseInfo()
                    };
                    owner.Send(ScMsgId.ScQuestStateUpdate, update);
                    owner.Send(ScMsgId.ScQuestObjectivesUpdate, upd);
                }
               
                quests.Add(quest);
            }
        }
        public void ProcessQuest(string id)
        {
            GameQuest quest = GetQuestById(id);
            if (quest != null)
            {
               
                quest.state = QuestState.Processing;
                var data = GetQuestData(id);
                ScQuestStateUpdate update = new()
                {
                    QuestId = quest.questId,
                    QuestState = (int)quest.state,
                    RoleBaseInfo = owner.GetRoleBaseInfo()
                };
                ScQuestObjectivesUpdate upd = new()
                {
                    QuestId = data.questId,

                };
                data.objectiveList.ForEach(o =>
                {
                    upd.QuestObjectives.Add(new QuestObjective()
                    {
                        ConditionId = o.condition.uniqueId,
                        Values =
                        {
                            {o.condition.uniqueId,0 }
                        }
                    });
                });
                owner.Send(ScMsgId.ScQuestObjectivesUpdate, upd);
                owner.Send(ScMsgId.ScQuestStateUpdate, update);
                
            }
        }
        public void CompleteQuest(string id)
        {
            GameQuest quest = GetQuestById(id);
            if (quest != null)
            {
                quest.state = QuestState.Completed;
                var data = GetQuestData(id);
                ScQuestStateUpdate update = new()
                {
                    QuestId = quest.questId,
                    QuestState=(int)quest.state,
                };
                ScQuestObjectivesUpdate upd = new()
                {
                    QuestId = data.questId,

                };
                data.objectiveList.ForEach(o =>
                {
                    upd.QuestObjectives.Add(new QuestObjective()
                    {
                        ConditionId = o.condition.uniqueId,
                        IsComplete=true,
                        Values =
                        {
                            {o.condition.uniqueId,1 }
                        }
                    });
                });
                owner.Send(ScMsgId.ScQuestObjectivesUpdate, upd);
                owner.Send(ScMsgId.ScQuestStateUpdate, update);
                quests.Remove(quest);
            }
        }
    }
    public class GameQuest
    {
        public string questId;
        public QuestState state;
        public GameQuest()
        {

        }
        public GameQuest(string id, QuestState state = QuestState.Available)
        {
            questId = id;
            this.state = state;
        }
    }
    public class GameMission
    {
        public string missionId;
        public MissionState state;

        public GameMission()
        {

        }
        public GameMission(string id, MissionState state = MissionState.Available)
        {
            missionId = id;
            this.state = state;
        }
    }
}
