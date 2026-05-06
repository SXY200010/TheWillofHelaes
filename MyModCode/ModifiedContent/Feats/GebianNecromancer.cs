using BlueprintCore.Blueprints.Configurators;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Utils;
using BlueprintCore.Utils.Assets;
using BlueprintCore.Utils.Types;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System.Linq;

namespace CruoromancerTweaks.ModifiedContent.Feats
{
    internal class GebianNecromancer
    {
        private static readonly string GebianNecromancerDescription = "GebianNecromancer.Description";

        public static void Configure()
        {
            BlueprintBuff backgroundsGebianNecromancerPetBuff = BlueprintTool.Get<BlueprintBuff>("c2b7fa6ad976b084db711433b6f17716");
            BlueprintFeature backgroundGebianNecromancer = BlueprintTool.Get<BlueprintFeature>("25b4f7ff8723d6e498f9cdc5ef2fad57");

            BuffConfigurator.For(backgroundsGebianNecromancerPetBuff)
                .RemoveComponents(c => c is AddStatBonus)
                .AddStatBonus
                (
                    stat: Kingmaker.EntitySystem.Stats.StatType.AdditionalAttackBonus,
                    value: 2,
                    descriptor: Kingmaker.Enums.ModifierDescriptor.UntypedStackable
                )
                .AddStatBonus
                (
                    stat: Kingmaker.EntitySystem.Stats.StatType.AC,
                    value: 2,
                    descriptor: Kingmaker.Enums.ModifierDescriptor.UntypedStackable
                )
                .AddStatBonus
                (
                    stat: Kingmaker.EntitySystem.Stats.StatType.Initiative,
                    value: 2,
                    descriptor: Kingmaker.Enums.ModifierDescriptor.UntypedStackable
                )
                .Configure();

            FeatureConfigurator.For(backgroundGebianNecromancer)
                .AddIncreaseSpellSchoolDamage(
                    school: SpellSchool.Necromancy,
                    damageBonus: 1
                )
                .AddIncreaseSpellSchoolDC(
                    school: SpellSchool.Necromancy,
                    bonusDC: 1
                )
                .AddSpellPenetrationBonus(
                    value: 1
                )
                .SetDescription(GebianNecromancerDescription)
                .Configure();
        }
    }
}
