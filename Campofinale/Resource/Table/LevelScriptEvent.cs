using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campofinale.Resource.Table
{
    [TableCfgType("Json/LevelScriptEvents.json", LoadPriority.LOW)]
    public class LevelScriptEvent : TableCfgResource
    {
        public string eventName;
        public string comment;
        public List<ScriptAction> actions;
    }
    public class ScriptAction
    {
        public ScriptActionType action;
        public string[] valueStr;
        public ulong[] valueUlong;
    }
    public enum ScriptActionType
    {
        None = 0,
        CompleteQuest = 1,
        ProcessQuest = 2,
        SpawnEnemy = 3,
        UnlockSystem = 4,
        EnterScene = 5,
        AddMission = 6,
        CompleteMission = 7,
    }
}
