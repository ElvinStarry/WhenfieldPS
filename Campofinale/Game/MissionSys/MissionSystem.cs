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
    public class MissionSystem
    {
        private class MissionObjectiveBinding
        {
            public string MissionId { get; init; } = "";
            public string QuestId { get; init; } = "";
            public string ConditionId { get; init; } = "";
            public bool Optional { get; init; }
        }

        private static readonly object missionEventBindingLock = new();
        private static bool missionEventBindingsInitialized;
        private static Dictionary<string, Dictionary<string, List<MissionObjectiveBinding>>> missionEventBindings = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, List<MissionObjectiveBinding>> globalEventBindings = new(StringComparer.OrdinalIgnoreCase);

        public Player owner;
        public List<GameMission> missions=new();
        public List<GameQuest> quests=new();
        public string curMission = "e0m0";

        public MissionSystem(Player o)
        {
            owner = o;
            EnsureMissionEventBindings();
        }

        private static void EnsureMissionEventBindings()
        {
            if (missionEventBindingsInitialized)
            {
                return;
            }

            lock (missionEventBindingLock)
            {
                if (missionEventBindingsInitialized)
                {
                    return;
                }

                missionEventBindings = new Dictionary<string, Dictionary<string, List<MissionObjectiveBinding>>>(StringComparer.OrdinalIgnoreCase);
                globalEventBindings = new Dictionary<string, List<MissionObjectiveBinding>>(StringComparer.OrdinalIgnoreCase);

                if (ResourceManager.missionDataTable == null || ResourceManager.missionDataTable.Count == 0)
                {
                    Logger.PrintWarn("[Mission] MissionDataTable not loaded yet, deferring mission event binding initialization");
                    return;
                }

                foreach (MissionDataTable missionData in ResourceManager.missionDataTable)
                {
                    if (missionData == null || string.IsNullOrWhiteSpace(missionData.missionId) || missionData.questDic == null)
                    {
                        continue;
                    }

                    if (!missionEventBindings.TryGetValue(missionData.missionId, out var missionMap))
                    {
                        missionMap = new Dictionary<string, List<MissionObjectiveBinding>>(StringComparer.OrdinalIgnoreCase);
                        missionEventBindings[missionData.missionId] = missionMap;
                    }

                    foreach (var questPair in missionData.questDic)
                    {
                        MissionDataTable.QuestInfo questInfo = questPair.Value;
                        if (questInfo == null || questInfo.objectiveList == null)
                        {
                            continue;
                        }

                        foreach (var objective in questInfo.objectiveList)
                        {
                            string conditionId = objective?.condition?.uniqueId ?? string.Empty;
                            if (string.IsNullOrWhiteSpace(conditionId))
                            {
                                continue;
                            }

                            AddMissionEventBinding(missionMap, conditionId, missionData.missionId, questInfo.questId, conditionId, questInfo.optional);
                            AddGlobalEventBinding(conditionId, missionData.missionId, questInfo.questId, conditionId, questInfo.optional);

                            // Alternate lookup keys to improve flexibility when matching event names.
                            if (!string.IsNullOrWhiteSpace(questInfo.questId))
                            {
                                AddMissionEventBinding(missionMap, questInfo.questId, missionData.missionId, questInfo.questId, conditionId, questInfo.optional);
                                AddGlobalEventBinding(questInfo.questId, missionData.missionId, questInfo.questId, conditionId, questInfo.optional);
                            }

                            string missionScopedKey = $"{missionData.missionId}:{conditionId}";
                            AddMissionEventBinding(missionMap, missionScopedKey, missionData.missionId, questInfo.questId, conditionId, questInfo.optional);
                            AddGlobalEventBinding(missionScopedKey, missionData.missionId, questInfo.questId, conditionId, questInfo.optional);
                        }
                    }
                }

                missionEventBindingsInitialized = true;
            }
        }

        private static void AddMissionEventBinding(Dictionary<string, List<MissionObjectiveBinding>> container, string key, string missionId, string questId, string conditionId, bool optional)
        {
            if (container == null || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(missionId) || string.IsNullOrWhiteSpace(questId) || string.IsNullOrWhiteSpace(conditionId))
            {
                return;
            }

            if (!container.TryGetValue(key, out var list))
            {
                list = new List<MissionObjectiveBinding>();
                container[key] = list;
            }

            if (list.Any(binding => binding.QuestId == questId && binding.ConditionId == conditionId))
            {
                return;
            }

            list.Add(new MissionObjectiveBinding
            {
                MissionId = missionId,
                QuestId = questId,
                ConditionId = conditionId,
                Optional = optional,
            });
        }

        private static void AddGlobalEventBinding(string key, string missionId, string questId, string conditionId, bool optional)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            if (!globalEventBindings.TryGetValue(key, out var list))
            {
                list = new List<MissionObjectiveBinding>();
                globalEventBindings[key] = list;
            }

            if (list.Any(binding => binding.MissionId == missionId && binding.QuestId == questId && binding.ConditionId == conditionId))
            {
                return;
            }

            list.Add(new MissionObjectiveBinding
            {
                MissionId = missionId,
                QuestId = questId,
                ConditionId = conditionId,
                Optional = optional,
            });
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
            });
        }

        public void HandleMissionEventTrigger(string missionId, string eventName, IDictionary<string, DynamicParameter> properties)
        {
            EnsureMissionEventBindings();

            if (string.IsNullOrWhiteSpace(eventName))
            {
                Logger.PrintWarn("[Mission] Received mission event trigger with empty event name");
                return;
            }

            List<MissionObjectiveBinding> candidateBindings = new();
            HashSet<string> processedBindings = new();

            if (!string.IsNullOrWhiteSpace(missionId) && missionEventBindings.TryGetValue(missionId, out var missionMap) && missionMap.TryGetValue(eventName, out var missionSpecificBindings))
            {
                candidateBindings.AddRange(missionSpecificBindings);
            }

            if (candidateBindings.Count == 0 && globalEventBindings.TryGetValue(eventName, out var globalBindingsForEvent))
            {
                if (string.IsNullOrWhiteSpace(missionId))
                {
                    candidateBindings.AddRange(globalBindingsForEvent);
                }
                else
                {
                    candidateBindings.AddRange(globalBindingsForEvent.Where(binding => string.Equals(binding.MissionId, missionId, StringComparison.OrdinalIgnoreCase)));
                }
            }

            if (candidateBindings.Count == 0)
            {
                Logger.PrintWarn($"[Mission] No objective binding found for event '{eventName}'{(string.IsNullOrWhiteSpace(missionId) ? string.Empty : $" within mission '{missionId}'")}");
                return;
            }

            HashSet<string> questsToUpdate = new();
            bool anyProgressUpdated = false;

            foreach (var binding in candidateBindings)
            {
                if (!string.IsNullOrWhiteSpace(missionId) && !string.Equals(binding.MissionId, missionId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string bindingKey = $"{binding.MissionId}|{binding.QuestId}|{binding.ConditionId}";
                if (!processedBindings.Add(bindingKey))
                {
                    continue;
                }

                GameMission missionInstance = GetMissionById(binding.MissionId);
                if (missionInstance == null)
                {
                    continue;
                }

                if (missionInstance.state == MissionState.Completed || missionInstance.state == MissionState.Failed)
                {
                    continue;
                }

                GameQuest quest = GetQuestById(binding.QuestId);
                if (quest == null)
                {
                    Logger.PrintWarn($"[Quest] Quest {binding.QuestId} not found when processing mission event '{eventName}'");
                    continue;
                }

                MissionDataTable.QuestInfo questData = GetQuestData(binding.QuestId);
                if (questData == null)
                {
                    Logger.PrintError($"[Quest] Quest data {binding.QuestId} not found when processing mission event '{eventName}'");
                    continue;
                }

                quest.objectiveProgress ??= new Dictionary<string, int>();
                if (!quest.objectiveProgress.ContainsKey(binding.ConditionId))
                {
                    quest.objectiveProgress[binding.ConditionId] = 0;
                }

                if (quest.state == QuestState.Available)
                {
                    ProcessQuest(quest.questId);
                    quest = GetQuestById(binding.QuestId) ?? quest;
                }

                int delta = ExtractProgressDelta(binding.ConditionId, properties);
                if (delta <= 0)
                {
                    delta = 1;
                }

                int currentValue = quest.objectiveProgress.GetValueOrDefault(binding.ConditionId, 0);
                quest.objectiveProgress[binding.ConditionId] = checked(currentValue + delta);

                questsToUpdate.Add(quest.questId);
                anyProgressUpdated = true;
            }

            if (!anyProgressUpdated)
            {
                return;
            }

            foreach (string questId in questsToUpdate)
            {
                GameQuest quest = GetQuestById(questId);
                if (quest == null)
                {
                    continue;
                }

                MissionDataTable.QuestInfo questData = GetQuestData(questId);
                if (questData == null)
                {
                    Logger.PrintError($"[Quest] Quest data {questId} missing during post-update mission event processing");
                    continue;
                }

                ScQuestObjectivesUpdate objectivesUpdate = BuildObjectivesUpdate(quest, questData);
                owner.Send(ScMsgId.ScQuestObjectivesUpdate, objectivesUpdate);

                bool completed = CheckQuestComplete(quest, questData);
                if (completed)
                {
                    CompleteQuest(questId);
                }
                else if (quest.state == QuestState.Processing)
                {
                    ScQuestStateUpdate stateUpdate = new()
                    {
                        QuestId = quest.questId,
                        QuestState = (int)quest.state,
                        RoleBaseInfo = owner.GetRoleBaseInfo(),
                    };
                    owner.Send(ScMsgId.ScQuestStateUpdate, stateUpdate);
                }
            }

            Save();
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

                EvaluateMissionCompletionForQuest(id);
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

        private void EvaluateMissionCompletionForQuest(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId) || ResourceManager.missionDataTable == null)
            {
                return;
            }

            foreach (var missionData in ResourceManager.missionDataTable)
            {
                if (missionData?.questDic == null || string.IsNullOrWhiteSpace(missionData.missionId))
                {
                    continue;
                }

                if (!missionData.questDic.ContainsKey(questId))
                {
                    continue;
                }

                GameMission mission = GetMissionById(missionData.missionId);
                if (mission == null)
                {
                    continue;
                }

                if (mission.state == MissionState.Completed)
                {
                    continue;
                }

                bool allRequiredCompleted = true;

                foreach (var questEntry in missionData.questDic.Values)
                {
                    if (questEntry.optional)
                    {
                        continue;
                    }

                    GameQuest questInstance = GetQuestById(questEntry.questId);
                    if (questInstance != null && questInstance.state != QuestState.Completed)
                    {
                        allRequiredCompleted = false;
                        break;
                    }
                }

                if (allRequiredCompleted)
                {
                    CompleteMission(missionData.missionId);
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

        private static int ExtractProgressDelta(string conditionId, IDictionary<string, DynamicParameter> properties)
        {
            if (properties == null || properties.Count == 0)
            {
                return 1;
            }

            if (!string.IsNullOrWhiteSpace(conditionId) && properties.TryGetValue(conditionId, out var conditionParam))
            {
                return ExtractDynamicParameterValue(conditionParam, 1);
            }

            if (properties.TryGetValue("value", out var valueParam))
            {
                return ExtractDynamicParameterValue(valueParam, 1);
            }

            if (properties.TryGetValue("Value", out var upperValueParam))
            {
                return ExtractDynamicParameterValue(upperValueParam, 1);
            }

            if (properties.TryGetValue("count", out var countParam))
            {
                return ExtractDynamicParameterValue(countParam, 1);
            }

            if (properties.TryGetValue("Count", out var upperCountParam))
            {
                return ExtractDynamicParameterValue(upperCountParam, 1);
            }

            DynamicParameter firstParam = properties.Values.FirstOrDefault();
            if (firstParam != null)
            {
                return ExtractDynamicParameterValue(firstParam, 1);
            }

            return 1;
        }

        private static int ExtractDynamicParameterValue(DynamicParameter parameter, int defaultValue)
        {
            if (parameter == null)
            {
                return defaultValue;
            }

            if (parameter.ValueIntList != null && parameter.ValueIntList.Count > 0)
            {
                return checked((int)parameter.ValueIntList[0]);
            }

            if (parameter.ValueFloatList != null && parameter.ValueFloatList.Count > 0)
            {
                return (int)Math.Round(parameter.ValueFloatList[0]);
            }

            if (parameter.ValueBoolList != null && parameter.ValueBoolList.Count > 0)
            {
                return parameter.ValueBoolList[0] ? 1 : 0;
            }

            if (parameter.ValueStringList != null && parameter.ValueStringList.Count > 0 && int.TryParse(parameter.ValueStringList[0], out int parsedValue))
            {
                return parsedValue;
            }

            return defaultValue;
        }
    }


}
