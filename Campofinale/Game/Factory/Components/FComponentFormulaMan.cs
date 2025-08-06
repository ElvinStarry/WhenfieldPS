using Campofinale.Resource;
using static Campofinale.Game.Factory.FactoryNode;

namespace Campofinale.Game.Factory.Components
{
    public class FComponentFormulaMan : FComponent
    {
        public FComponentFormulaMan(uint id) : base(id, FCComponentType.FormulaMan)
        {
        }

        public override void SetComponentInfo(ScdFacCom proto)
        {
            proto.FormulaMan = new()
            {
                
            };
        }
    }
}
