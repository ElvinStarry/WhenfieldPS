using Campofinale.Resource;
using static Campofinale.Game.Factory.FactoryNode;

namespace Campofinale.Game.Factory.Components
{
    public class FComponentFormulaMan : FComponent
    {
        public string currentGroup = "group_grinder_normal";
        public string currentMode = "normal";
        public FComponentFormulaMan(uint id) : base(id, FCComponentType.FormulaMan)
        {
        }

        public override void SetComponentInfo(ScdFacCom proto)
        {
            proto.FormulaMan = new()
            {
                CurrentGroup = currentGroup,
                CurrentMode = currentMode,
                FormulaIds = {
                    "grinder_iron_powder_1",
                    "grinder_quartz_powder_1",
                    "grinder_originium_powder_1",
                    "grinder_carbon_powder_1",
                    "grinder_crystal_powder_1",
                    "grinder_plant_moss_powder_1_1",
                    "grinder_plant_moss_powder_2_1",
                    "grinder_plant_moss_powder_3_1",
                    "grinder_plant_bbflower_powder_1_1",
                    "grinder_plant_grass_powder_1_1",
                    "grinder_plant_grass_powder_2_1"
                }
            };
        }
    }
}
