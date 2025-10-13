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
            if (!Server.config.serverOptions.missionsEnabled)
            {
                string json1 = File.ReadAllText("44_ScSyncAllMission.json");
                ScSyncAllMission m = Newtonsoft.Json.JsonConvert.DeserializeObject<ScSyncAllMission>(json1);
                m.TrackMissionId = "";
                return m;
            }
            ScSyncAllMission sync = new ScSyncAllMission();
            sync.TrackMissionId = curMission;
            missions.ForEach(m =>
            {
                if(!sync.Missions.ContainsKey(m.missionId))
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
                    int progressValue = q.objectiveProgress.GetValueOrDefault(o.condition.uniqueId, 0);
                    quest.QuestObjectives.Add(new QuestObjective()
                    {
                        ConditionId = o.condition.uniqueId,
                        IsComplete = progressValue > 0,
                        Values =
                        {
                            {o.condition.uniqueId, progressValue }
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
            DatabaseManager.db.UpsertMissionData(new MissionData()
            {
                roleId=owner.roleId,
                curMission=curMission,
                missions=missions,
                quests=quests,
            });
        }
        public void Load()
        {
            MissionData data= DatabaseManager.db.LoadMissionData(owner.roleId);
            if (data != null)
            {
                curMission = data.curMission;
                missions = data.missions;
                quests = data.quests;
            }
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
                    owner.Send(ScMsgId.ScMissionStateUpdate, s);
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

                // Initialize progress for all objectives to 0
                foreach (var objective in data.objectiveList)
                {
                    quest.objectiveProgress[objective.condition.uniqueId] = 0;
                }

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
                            IsComplete = false,
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

                // Ensure progress is initialized for all objectives
                foreach (var objective in data.objectiveList)
                {
                    if (!quest.objectiveProgress.ContainsKey(objective.condition.uniqueId))
                    {
                        quest.objectiveProgress[objective.condition.uniqueId] = 0;
                    }
                }

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
                    int progressValue = quest.objectiveProgress.GetValueOrDefault(o.condition.uniqueId, 0);
                    upd.QuestObjectives.Add(new QuestObjective()
                    {
                        ConditionId = o.condition.uniqueId,
                        IsComplete = progressValue > 0,
                        Values =
                        {
                            {o.condition.uniqueId, progressValue }
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

                // Give quest rewards
                GiveRewards(data.rewardId);
            }
        }

        public void FailQuest(string questId)
        {
            GameQuest quest = GetQuestById(questId);
            if (quest == null)
            {
                Logger.PrintError($"[Quest] Quest {questId} not found for player {owner.roleId}");
                return;
            }

            var questData = GetQuestData(questId);
            if (questData == null)
            {
                Logger.PrintError($"[Quest] Quest data {questId} not found in resource table");
                return;
            }

            quest.state = QuestState.Failed;

            ScQuestStateUpdate update = new()
            {
                QuestId = quest.questId,
                QuestState = (int)quest.state,
                RoleBaseInfo = owner.GetRoleBaseInfo()
            };

            owner.Send(ScMsgId.ScQuestStateUpdate, update);

            Logger.Print($"[Quest] Quest {questId} failed for player {owner.roleId}");

            // Check if quest has autoRestartWhenFailed flag
            if (questData.autoRestartWhenFailed)
            {
                Logger.Print($"[Quest] Auto-restarting quest {questId} due to autoRestartWhenFailed");
                quests.Remove(quest);

                // Re-add the quest in Available state
                AddQuest(questData, notify: true);
            }
            else
            {
                // Keep the failed quest in the list for tracking
                // Remove it only if explicitly requested or if quest system cleans up failed quests
            }
        }

        public void TrackMission(string v)
        {
            curMission = v;
            owner.Send(ScMsgId.ScTrackMissionChange, new ScTrackMissionChange()
            {
                MissionId = curMission,
            });
        }

        private void GiveRewards(string rewardId)
        {
            if (string.IsNullOrEmpty(rewardId))
            {
                return; // No reward for this mission
            }

            if (!ResourceManager.rewardTable.ContainsKey(rewardId))
            {
                Logger.PrintError($"[Mission] Reward ID {rewardId} not found in RewardTable!");
                return;
            }

            // Give rewards at player's current position, sourceType=1 means mission reward
            owner.inventoryManager.AddRewards(rewardId, owner.position, sourceType: 1);
            Logger.Print($"[Mission] Rewarded player {owner.roleId} with {rewardId}");
        }

        public void CompleteMission(string v)
        {
            if(curMission == v)
            {
                TrackMission("");
            }
            GameMission mission = missions.Find(m => m.missionId == v);
            MissionDataTable data = ResourceManager.missionDataTable.Find(m => m.missionId == v);
            if (mission != null && data != null)
            {
                mission.state=MissionState.Completed;
                ScMissionStateUpdate s = new()
                {
                    MissionId = mission.missionId,
                    MissionState = (int)mission.state,
                    SucceedId = -1,

                };
                owner.Send(ScMsgId.ScMissionStateUpdate, s);
                GiveRewards(data.rewardId);
            }
        }

        public void FailMission(string missionId)
        {
            if (curMission == missionId)
            {
                TrackMission("");
            }

            GameMission mission = missions.Find(m => m.missionId == missionId);
            MissionDataTable data = ResourceManager.missionDataTable.Find(m => m.missionId == missionId);

            if (mission != null && data != null)
            {
                mission.state = MissionState.Failed;

                ScMissionStateUpdate s = new()
                {
                    MissionId = mission.missionId,
                    MissionState = (int)mission.state,
                    SucceedId = -1,
                };
                owner.Send(ScMsgId.ScMissionStateUpdate, s);

                Logger.Print($"[Mission] Mission {missionId} failed for player {owner.roleId}");

                // TODO: Trigger onMissionFailedId event if event system is implemented
                // if (data.onMissionFailedId > 0) { TriggerEvent(data.onMissionFailedId); }

                // Check for autoRestartWhenFailed in quests (future-proofing)
                foreach (var quest in data.questDic.Values)
                {
                    if (quest.autoRestartWhenFailed)
                    {
                        Logger.Print($"[Mission] Auto-restarting mission {missionId} due to quest autoRestartWhenFailed");
                        missions.Remove(mission);
                        AddMission(missionId, MissionState.Available, notify: true);
                        return;
                    }
                }
            }
        }

        public bool CheckQuestComplete(GameQuest quest, MissionDataTable.QuestInfo data)
        {
            // Check if objectiveConditionNum is specified (complete N objectives)
            if (data.objectiveConditionNum > 0)
            {
                int completedCount = 0;
                foreach (var objective in data.objectiveList)
                {
                    if (quest.objectiveProgress.TryGetValue(objective.condition.uniqueId, out int value) && value > 0)
                    {
                        completedCount++;
                    }
                }
                return completedCount >= data.objectiveConditionNum;
            }

            // Otherwise, all objectives must be completed
            foreach (var objective in data.objectiveList)
            {
                if (!quest.objectiveProgress.TryGetValue(objective.condition.uniqueId, out int value) || value == 0)
                {
                    return false; // At least one objective not complete
                }
            }

            return true;
        }

        public ScQuestObjectivesUpdate BuildObjectivesUpdate(GameQuest quest, MissionDataTable.QuestInfo data)
        {
            ScQuestObjectivesUpdate upd = new()
            {
                QuestId = quest.questId,
            };

            foreach (var objective in data.objectiveList)
            {
                int progressValue = quest.objectiveProgress.GetValueOrDefault(objective.condition.uniqueId, 0);
                bool isComplete = progressValue > 0; // Assume > 0 means complete (can be refined)

                upd.QuestObjectives.Add(new QuestObjective()
                {
                    ConditionId = objective.condition.uniqueId,
                    IsComplete = isComplete,
                    Values =
                    {
                        { objective.condition.uniqueId, progressValue }
                    }
                });
            }

            return upd;
        }
    }


}
