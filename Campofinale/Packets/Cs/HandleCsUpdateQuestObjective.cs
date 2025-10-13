using Campofinale.Network;
using Campofinale.Protocol;
using Campofinale.Resource;
using Campofinale.Resource.Table;

namespace Campofinale.Packets.Cs
{
    public class HandleCsUpdateQuestObjective
    {

        [Server.Handler(CsMsgId.CsUpdateQuestObjective)]
        public static void Handle(Player session, CsMsgId cmdId, Packet packet)
        {
            CsUpdateQuestObjective req = packet.DecodeBody<CsUpdateQuestObjective>();

            Logger.Print($"[Quest] Player {session.roleId} updating quest {req.QuestId} objectives");

            var quest = session.missionSystem.GetQuestById(req.QuestId);
            if (quest == null)
            {
                Logger.PrintError($"[Quest] Quest {req.QuestId} not found for player {session.roleId}");
                return;
            }

            // Get quest data for validation
            var questData = session.missionSystem.GetQuestData(req.QuestId);
            if (questData == null)
            {
                Logger.PrintError($"[Quest] Quest data {req.QuestId} not found in resource table");
                return;
            }

            // Validate quest is in processing state
            if (quest.state != QuestState.Processing)
            {
                Logger.PrintError($"[Quest] Quest {req.QuestId} is not in Processing state (current: {quest.state})");
                return;
            }

            bool progressUpdated = false;

            // Process each objective update
            foreach (var op in req.ObjectiveValueOps)
            {
                // Layer 1 Validation: Check if conditionId exists in quest definition
                var objective = questData.objectiveList.Find(o => o.condition.uniqueId == op.ConditionId);
                if (objective == null)
                {
                    Logger.PrintError($"[Quest] ConditionId {op.ConditionId} not found in quest {req.QuestId}");
                    continue;
                }

                // Layer 1 Validation: Value must be non-negative
                if (op.Value < 0)
                {
                    Logger.PrintError($"[Quest] Invalid negative value {op.Value} for condition {op.ConditionId}");
                    continue;
                }

                // Get current progress
                int currentProgress = quest.objectiveProgress.GetValueOrDefault(op.ConditionId, 0);
                int newProgress;

                if (op.IsAdd)
                {
                    // Incremental update
                    newProgress = currentProgress + op.Value;
                }
                else
                {
                    // Direct set
                    newProgress = op.Value;
                }

                // Layer 2 Validation: Prevent progress rollback
                if (newProgress < currentProgress)
                {
                    Logger.PrintError($"[Quest] Attempted progress rollback for {op.ConditionId}: {currentProgress} -> {newProgress}");
                    continue;
                }

                // Update progress
                quest.objectiveProgress[op.ConditionId] = newProgress;
                progressUpdated = true;

                Logger.Print($"[Quest] Updated {req.QuestId} objective {op.ConditionId}: {currentProgress} -> {newProgress} (IsAdd: {op.IsAdd})");
            }

            // If no progress was updated, still send response but don't check completion
            if (!progressUpdated)
            {
                Logger.Print($"[Quest] No valid progress updates for quest {req.QuestId}");
            }

            // Send objectives update to client
            ScQuestObjectivesUpdate objectivesUpdate = session.missionSystem.BuildObjectivesUpdate(quest, questData);
            session.Send(ScMsgId.ScQuestObjectivesUpdate, objectivesUpdate);

            // Check if quest is now complete (only if autoSucceed is true)
            if (progressUpdated && questData.autoSucceed)
            {
                bool isComplete = session.missionSystem.CheckQuestComplete(quest, questData);
                if (isComplete)
                {
                    Logger.Print($"[Quest] Quest {req.QuestId} auto-completed");
                    session.missionSystem.CompleteQuest(req.QuestId);
                }
            }

            // Save progress
            session.missionSystem.Save();
        }

    }
}
