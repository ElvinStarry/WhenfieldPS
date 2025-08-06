using Campofinale.Resource;
using static Campofinale.Game.Factory.FactoryNode;

namespace Campofinale.Game.Factory.Components
{
    public class FComponentProducer : FComponent
    {
        public FComponentProducer(uint id) : base(id, FCComponentType.Producer)
        {
        }

        public override void SetComponentInfo(ScdFacCom proto)
        {
            proto.Producer = new()
            {
                
            };
        }
    }
}
