using BlueprintCore.Blueprints.Configurators;
using BlueprintCore.Blueprints.Configurators.UnitLogic.ActivatableAbilities;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Craft;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Utility;

namespace CruoromancerTweaks.ModifiedContent.Classes
{
    internal class DeathTouched
    {
        public static void Configure()
        {
            BlueprintFeature UndeadAnimalBonusFeature = 
                FeatureConfigurator.New("UndeadAnimalBonusFeature", "D36D3C2B-0C8D-4351-AEF3-10D8F8BE4CC3")
                .SetDisplayName("UndeadAnimalBonusFeature.Name")
                .SetDescription("UndeadAnimalBonusFeature.Description")
                .AddStatBonus(
                    descriptor: ModifierDescriptor.Profane,
                    stat: StatType.Charisma,
                    value: 6)
                .Configure();
            FeatureConfigurator.For("045b1eef7a1577d44884e6f105f35a09")
                .SetDescription("DeathtouchedFortitudeBonus.Description")
                .Configure();
            FeatureConfigurator.For("38570ac838f9e7e48af006800c0fd69c")
                .SetDescription("ResistLevelDrainDhampir.Description")
                .Configure();
            ArchetypeConfigurator.For("3ae8abeef5615294c85a2d0f92f592de")
                .AddToAddFeatures(
                level: 6,
                features: [
                    BlueprintTool.Get<BlueprintFeature>("8f58b4029511b5345981ffaf1da5ea2e")
                ])
                .AddToAddFeatures(
                level: 9,
                features: [
                    BlueprintTool.Get<BlueprintFeature>("38570ac838f9e7e48af006800c0fd69c")
                ])
                .AddToAddFeatures(
                level: 15,
                features: [
                    BlueprintTool.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33"),
                    BlueprintTool.Get<BlueprintFeature>("8a75eb16bfff86949a4ddcb3dd2f83ae"),
                    UndeadAnimalBonusFeature
                ])
                .Configure();

        }
    }
}
