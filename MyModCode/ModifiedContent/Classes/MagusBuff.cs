using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruoromancerTweaks.ModifiedContent.Classes
{
    internal class MagusBuff
    {
        public static void Configure()
        {
            //法术战斗Buff增加免疫借机攻击
            BuffConfigurator.For("91e4b45ab5f29574aa1fb41da4bbdcf2")
                .AddIgnoreAttacksOfOpportunityForSpellList(spellDescriptor: new SpellDescriptorWrapper
                {
                    m_IntValue = 0L
                })
                .Configure();
        }
    }
}
