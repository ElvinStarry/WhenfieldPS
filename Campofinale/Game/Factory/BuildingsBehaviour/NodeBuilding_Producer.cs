using Campofinale.Game.Factory.Components;
using Campofinale.Protocol;
using Campofinale.Resource;
using Campofinale.Resource.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Campofinale.Resource;
using static Campofinale.Resource.ResourceManager;

namespace Campofinale.Game.Factory.BuildingsBehaviour
{
    public class NodeBuilding_Producer : NodeBuildingBehaviour
    {
        public uint inputCacheId = 0;
        public uint outputCacheId = 0;
        public uint producerId = 0;
        public int currentProgress = 0;
        public override void Init(FactoryChapter chapter, FactoryNode node)
        {
            FComponentCache cache1 = (FComponentCache)new FComponentCache(chapter.nextCompV(), FCComponentPos.CacheIn1).Init();
            FComponentCache cache2 = (FComponentCache)new FComponentCache(chapter.nextCompV(), FCComponentPos.CacheOut1).Init();
            FComponentProducer producer = (FComponentProducer)new FComponentProducer(chapter.nextCompV()).Init();
            node.components.Add(producer);
            node.components.Add(new FComponentFormulaMan(chapter.nextCompV()).Init());
            node.components.Add(cache1);
            node.components.Add(cache2);
            inputCacheId = cache1.compId;
            outputCacheId = cache2.compId;
            producerId = producer.compId;
            node.components.Add(new FComponentPortManager(chapter.nextCompV(), 3,cache1).Init());
            node.components.Add(new FComponentPortManager(chapter.nextCompV(), 3, cache2).Init());
        }
        public override void Update(FactoryChapter chapter, FactoryNode node)
        {
           
            if (node.powered)
            {
                FComponentProducer producer = node.GetComponent<FComponentProducer>(producerId);
                FComponentCache inCache = node.GetComponent<FComponentCache>(inputCacheId);
                FComponentCache outCache = node.GetComponent<FComponentCache>(outputCacheId);
                FactoryMachineCraftTable craftingRecipe = null;
                string recipe = ResourceManager.FindFactoryMachineCraftIdUsingCacheItems(inCache.items);
                producer.formulaId = recipe;
                ResourceManager.factoryMachineCraftTable.TryGetValue(producer.formulaId, out craftingRecipe);

                if (craftingRecipe != null)
                {
                    producer.inBlock = outCache.IsFull();
                    if (craftingRecipe.CacheHaveItems(inCache) && !outCache.IsFull())
                    {
                        producer.inProduce = true;
                        
                        producer.lastFormulaId = recipe;
                        producer.progress += craftingRecipe.totalProgress/craftingRecipe.progressRound;
                        currentProgress++;
                        if (currentProgress >= craftingRecipe.progressRound)
                        {
                            currentProgress = 0;
                            List<ItemCount> toConsume = craftingRecipe.GetIngredients();
                            inCache.ConsumeItems(toConsume);
                            craftingRecipe.outcomes.ForEach(e =>
                            {
                                e.group.ForEach(i =>
                                {
                                    outCache.AddItem(i.id, i.count);
                                });

                            });
                        }
                    }
                    else
                    {
                        producer.inBlock = false;
                        producer.inProduce = false;
                        producer.progress = 0;
                    }
                }
                else
                {
                    producer.inBlock = false;
                    producer.inProduce = false;
                    producer.progress = 0;
                }
            }
           /* ScFactoryModifyChapterComponents update = new()
            {
                ChapterId = chapter.chapterId,
                Tms = DateTime.UtcNow.ToUnixTimestampMilliseconds()/1000,
                
            };

            foreach (var comp in node.components)
            {
                update.Components.Add(comp.ToProto());
            }
            chapter.GetOwner().Send(ScMsgId.ScFactoryModifyChapterComponents, update);*/
        }
    }
}
