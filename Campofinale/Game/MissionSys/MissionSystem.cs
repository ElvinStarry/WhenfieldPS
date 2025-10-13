using Campofinale.Database;
using Campofinale.Protocol;
using Campofinale.Resource;
using Campofinale.Resource.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Campofinale.Game.MissionSys
{
    public class BlocMissionState
    {
        public Dictionary<string, string> blocMissions = new();
        public long rollCount;
        public long nextRefreshTime;
        public bool rewardGot;
        public int completedNum;
    }

    public class MissionSystem
    {
        public Player owner;
        public List<GameMission> missions=new();
        public List<GameQuest> quests=new();
        public string curMission = "e0m0";
        public BlocMissionState blocMissionState = new();

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
            missions ??= new List<GameMission>();
            quests ??= new List<GameQuest>();
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
                var data = GetQuestData(q.questId);
                if (data == null)
                {
                    Logger.PrintError($"[Mission] Quest data not found for {q.questId}, skipping in ToProto");
                    return;
                }

                Quest quest=new Quest()
                {
                    QuestId = q.questId,
                    QuestState = (int)q.state,

                };
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
            NormalizeState();
            DatabaseManager.db.UpsertMissionData(new MissionData()
            {
                roleId=owner.roleId,
                curMission=curMission,
                missions=missions,
                quests=quests,
                blocMissionState = blocMissionState,
            });
        }
        public void Load()
        {
            MissionData data= DatabaseManager.db.LoadMissionData(owner.roleId);
            if (data != null)
            {
                if (!string.IsNullOrWhiteSpace(data.curMission))
                {
                    curMission = data.curMission;
                }
                missions = data.missions ?? missions;
                quests = data.quests ?? quests;
                blocMissionState = data.blocMissionState ?? blocMissionState;
            }
            NormalizeState();
        }
        private void NormalizeState()
        {
            missions = (missions ?? new List<GameMission>())
                .Where(m => m != null && !string.IsNullOrWhiteSpace(m.missionId))
                .GroupBy(m => m.missionId)
                .Select(g => g.Last())
                .ToList();

            quests = (quests ?? new List<GameQuest>())
                .Where(q => q != null && !string.IsNullOrWhiteSpace(q.questId))
                .GroupBy(q => q.questId)
                .Select(g =>
                {
                    var quest = g.Last();
                    quest.objectiveProgress ??= new Dictionary<string, int>();
                    var data = GetQuestData(quest.questId);
                    if (data != null)
                    {
                        foreach (var objective in data.objectiveList)
                        {
                            quest.objectiveProgress.TryAdd(objective.condition.uniqueId, 0);
                        }
                    }
                    return quest;
                })
                .ToList();

            blocMissionState ??= new BlocMissionState();
            blocMissionState.blocMissions ??= new Dictionary<string, string>();
            if (blocMissionState.blocMissions.Count == 0 && ResourceManager.blocMissionTable.Count > 0)
            {
                string defaultMission = ResourceManager.blocMissionTable.Values.First().missionId;
                foreach (var bloc in ResourceManager.blocDataTable.Keys)
                {
                    if (!string.IsNullOrWhiteSpace(bloc))
                    {
                        blocMissionState.blocMissions.TryAdd(bloc, defaultMission);
                    }
                }
            }
        }
        public GameMission GetMissionById(string id)
        {
            return missions.Find(m => m.missionId == id);
        }
        public void AddMission(string id,MissionState state = MissionState.Available, bool notify=false)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Logger.PrintError("[Mission] Attempted to add mission with empty id");
                return;
            }

            MissionDataTable data = ResourceManager.missionDataTable.Find(m=>m.missionId == id);
            GameMission mission = GetMissionById(id);
            if (mission == null)
            {
                mission = new GameMission(id, state);
                missions.Add(mission);
            }
            else
            {
                mission.state = state;
            }

            if (notify)
            {
                ScMissionStateUpdate s = new()
                {
                    MissionId = mission.missionId,
                    MissionState = (int)mission.state,
                    SucceedId = -1,
                };
                owner.Send(ScMsgId.ScMissionStateUpdate, s);
            }

            if (data == null)
            {
                Logger.PrintWarn($"[Mission] Mission data not found for {id}, skipping quest initialization");
                return;
            }

            foreach (var q in data.questDic.Values)
            {
                AddQuest(q, false);
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
                if (data == null)
                {
                    Logger.PrintError($"[Quest] Quest data not found for {id} in ProcessQuest");
                    return;
                }

                quest.objectiveProgress ??= new Dictionary<string, int>();

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
                if (data == null)
                {
                    Logger.PrintError($"[Quest] Quest data not found for {id} in CompleteQuest");
                    quest.state = QuestState.Completed;
                    quests.Remove(quest);
                    return;
                }
                quest.objectiveProgress ??= new Dictionary<string, int>();
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
                    int progressValue = quest.objectiveProgress.GetValueOrDefault(o.condition.uniqueId, 0);
                    upd.QuestObjectives.Add(new QuestObjective()
                    {
                        ConditionId = o.condition.uniqueId,
                        IsComplete=true,
                        Values =
                        {
                            {o.condition.uniqueId, progressValue > 0 ? progressValue : 1 }
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

            quest.objectiveProgress ??= new Dictionary<string, int>();

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
            GameMission mission = GetMissionById(v);
            MissionDataTable data = ResourceManager.missionDataTable.Find(m => m.missionId == v);
            if (mission == null)
            {
                Logger.PrintError($"[Mission] Mission {v} not found for player {owner.roleId} when completing");
                return;
            }

            mission.state=MissionState.Completed;
            ScMissionStateUpdate s = new()
            {
                MissionId = mission.missionId,
                MissionState = (int)mission.state,
                SucceedId = -1,

            };
            owner.Send(ScMsgId.ScMissionStateUpdate, s);

            if (data != null)
            {
                GiveRewards(data.rewardId);
            }
            else
            {
                Logger.PrintWarn($"[Mission] Mission data not found for {v} while completing, reward skipped");
            }
        }

        public void FailMission(string missionId)
        {
            if (curMission == missionId)
            {
                TrackMission("");
            }

            GameMission mission = GetMissionById(missionId);
            MissionDataTable data = ResourceManager.missionDataTable.Find(m => m.missionId == missionId);

            if (mission == null)
            {
                Logger.PrintError($"[Mission] Mission {missionId} not found for player {owner.roleId} when failing");
                return;
            }

            mission.state = MissionState.Failed;

            ScMissionStateUpdate s = new()
            {
                MissionId = mission.missionId,
                MissionState = (int)mission.state,
                SucceedId = -1,
            };
            owner.Send(ScMsgId.ScMissionStateUpdate, s);

            Logger.Print($"[Mission] Mission {missionId} failed for player {owner.roleId}");

            if (data == null)
            {
                Logger.PrintWarn($"[Mission] Mission data not found for {missionId} while processing failure");
                return;
            }

            bool shouldRestart = false;
            foreach (var questInfo in data.questDic.Values)
            {
                var questInstance = GetQuestById(questInfo.questId);
                if (questInstance != null)
                {
                    questInstance.state = QuestState.Failed;
                }

                if (questInfo.autoRestartWhenFailed)
                {
                    shouldRestart = true;
                }
            }

            if (shouldRestart)
            {
                Logger.Print($"[Mission] Auto-restarting mission {missionId} due to quest autoRestartWhenFailed");
                foreach (var questInfo in data.questDic.Values)
                {
                    var questInstance = GetQuestById(questInfo.questId);
                    if (questInstance != null)
                    {
                        quests.Remove(questInstance);
                    }
                }

                missions.Remove(mission);
                AddMission(missionId, MissionState.Available, notify: true);
            }
        }

        public ScSyncBlocMissionInfo BuildBlocMissionInfo()
        {
            bool hadAssignments = blocMissionState?.blocMissions?.Count > 0;
            NormalizeState();
            if (!hadAssignments && blocMissionState.blocMissions.Count > 0)
            {
                Save();
            }

            ScSyncBlocMissionInfo info = new()
            {
                RewardGot = blocMissionState.rewardGot,
                RollCount = blocMissionState.rollCount,
                NextRefreshTine = blocMissionState.nextRefreshTime,
                CompletedNum = blocMissionState.completedNum,
            };

            foreach (var pair in blocMissionState.blocMissions)
            {
                if (!info.BlocMissions.ContainsKey(pair.Key))
                {
                    info.BlocMissions.Add(pair.Key, pair.Value);
                }
            }

            return info;
        }

        public ScRollBlocMission RollBlocMission(string blocId)
        {
            NormalizeState();

            if (string.IsNullOrWhiteSpace(blocId))
            {
                Logger.PrintError("[Bloc] Attempted to roll bloc mission with empty blocId");
                return new ScRollBlocMission();
            }

            if (!ResourceManager.blocDataTable.ContainsKey(blocId))
            {
                Logger.PrintWarn($"[Bloc] Bloc {blocId} not found in BlocDataTable");
            }

            var missionPool = ResourceManager.blocMissionTable.Values
                .Select(m => m.missionId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToList();

            if (missionPool.Count == 0)
            {
                Logger.PrintError("[Bloc] BlocMissionTable empty, cannot roll mission");
                return new ScRollBlocMission()
                {
                    BlocId = blocId,
                    MissionId = "",
                    RollCount = blocMissionState.rollCount,
                    NextRefreshTine = blocMissionState.nextRefreshTime
                };
            }

            string currentMission = blocMissionState.blocMissions.GetValueOrDefault(blocId);
            var selectable = missionPool.Where(id => id != currentMission).ToList();
            if (selectable.Count == 0)
            {
                selectable = missionPool;
            }

            string nextMission = selectable[Random.Shared.Next(selectable.Count)];

            blocMissionState.blocMissions[blocId] = nextMission;
            blocMissionState.rollCount++;

            int refreshSeconds = ResourceManager.blocMissionConst?.addRefreshNumDuration ?? 0;
            if (refreshSeconds > 0)
            {
                blocMissionState.nextRefreshTime = DateTimeOffset.UtcNow.AddSeconds(refreshSeconds).ToUnixTimeMilliseconds();
            }
            else if (blocMissionState.nextRefreshTime == 0)
            {
                blocMissionState.nextRefreshTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            Save();

            return new ScRollBlocMission()
            {
                BlocId = blocId,
                MissionId = nextMission,
                RollCount = blocMissionState.rollCount,
                NextRefreshTine = blocMissionState.nextRefreshTime
            };
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
