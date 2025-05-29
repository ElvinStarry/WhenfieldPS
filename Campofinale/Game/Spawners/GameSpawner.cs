using Campofinale.Game.Entities;
using Campofinale.Resource;
using Campofinale.Resource.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campofinale.Game.Spawners
{
    public class GameSpawner
    {
        public Scene scene;
        public int curWave = 1;
        public int curGroup = 1;
        public string configId;
        public bool spawned;

        public SpawnerConfig GetConfig()
        {
            return ResourceManager.spawnerConfigs.Find(s => s.configId == configId);
        }
        public SpawnerConfig.WaveGroup GetCurrentWaveGroup()
        {
            return GetConfig().waveMap[$"{curWave}"].groupMap[$"{curGroup}"];
        }
        public int GetEnemiesOfCurrentWave()
        {
            return scene.entities.FindAll(e => e.dependencyGroupId == GetCurrentWaveGroup().groupId).Count;
        }
        
        public void Update(Player player)
        {
            if (spawned)
            {
                if (GetEnemiesOfCurrentWave() < 1)
                {
                    if (GetConfig().waveMap.ContainsKey($"{curWave + 1}"))
                    {
                        curWave++;
                        spawned = false;
                    }
                }
            }
            else
            {
                
                foreach (var item in GetCurrentWaveGroup().actionMap.Values)
                {
                    Logger.Print($"Debug: Spawning {item.libraryKey}");
                    scene.entities.Add(new EntityMonster(item.libraryKey, 1, player.roleId, item.position, item.rotation, scene.sceneNumId)
                    {
                        dependencyGroupId = GetCurrentWaveGroup().groupId,
                        defaultHide = false,
                        spawned = false
                    });
                   
                    
                }
                spawned = true;

            }
            
        }
    }
}
