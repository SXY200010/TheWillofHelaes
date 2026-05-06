using BlueprintCore.Blueprints.CustomConfigurators;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Blueprints.Classes.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic.FactLogic;

namespace CruoromancerTweaks.ModifiedContent.Classes
{
    internal class BloodSeeker
    {
        public static void Configure()
        {
            //血池点数增加智力
            AbilityResourceConfigurator.For("d776a80421be411e8d3b62fc956b80d2")
                .SetMaxAmount(
                maxAmount: new BlueprintAbilityResource.Amount
                {
                    m_Archetypes = [BlueprintTool.Get<BlueprintArchetype>("11e473bfb29c42f580b9a801b6771788").ToReference<BlueprintArchetypeReference>()],
                    m_Class = [BlueprintTool.Get<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb").ToReference<BlueprintCharacterClassReference>()],
                    IncreasedByLevel = true,
                    IncreasedByStat = true,
                    LevelIncrease = 1,
                    LevelStep = 3,
                    PerStepIncrease = 2,
                    ResourceBonusStat = StatType.Intelligence,
                    StartingIncrease = 2,
                    StartingLevel = 1,
                })
                .Configure();
            //buff加强
            BuffConfigurator.For("2fd9fdab55ae4c08af4a415213019d00")
                .AddStatBonus(
                descriptor: Kingmaker.Enums.ModifierDescriptor.Polymorph,
                stat: StatType.Intelligence,
                value: 2)
                .AddAutoMetamagic(
                abilities: [BlueprintTool.Get<BlueprintAbility>("7b469dfb3ca740e8a45a5ff418979e60")],
                metamagic: Kingmaker.UnitLogic.Abilities.Metamagic.Reach,
                allowedAbilities: AutoMetamagic.AllowedType.Any)
                .AddAutoMetamagic(
                abilities: [BlueprintTool.Get<BlueprintAbility>("7b469dfb3ca740e8a45a5ff418979e60")],
                metamagic: Kingmaker.UnitLogic.Abilities.Metamagic.Quicken,
                allowedAbilities: AutoMetamagic.AllowedType.Any)
                .Configure();
        }
    }
}
