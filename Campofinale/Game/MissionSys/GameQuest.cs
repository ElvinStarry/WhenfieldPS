using Campofinale.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campofinale.Game.MissionSys
{
    public class GameQuest
    {
        public string questId;
        public QuestState state;
        public Dictionary<string, int> objectiveProgress = new(); // conditionId -> progress value

        public GameQuest()
        {

        }
        public GameQuest(string id, QuestState state = QuestState.Available)
        {
            questId = id;
            this.state = state;
        }
    }
}
