using Campofinale.Resource;
using static Campofinale.Game.Factory.FactoryNode;

namespace Campofinale.Game.Factory.Components
{
    public class FComponentCache : FComponent
    {
        
        public FComponentCache(uint id) : base(id, FCComponentType.Cache)
        {
        }

        public override void SetComponentInfo(ScdFacCom proto)
        {
            proto.Cache = new()
            {
                
            };
        }
    }
}
