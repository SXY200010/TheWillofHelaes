using BlueprintCore.Blueprints.Configurators.Items.Ecnchantments;
using BlueprintCore.Blueprints.CustomConfigurators;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.Classes.Selection;
using BlueprintCore.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem;
using BlueprintCore.Conditions.Builder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.Kingdom.Settlements.SettlementGridTopology;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using HarmonyLib;

namespace CruoromancerTweaks.ModifiedContent.Classes
{
    internal class BloodLine
    {
        private static readonly string[] bloodLineUndeadProgressionGuids = [
            "a1a8bf61cadaa4143b2d4966f2d1142e", 
            "a56252fbbd7b45db97129187ee1fa3ba",
            "618c6b173f6947843a62e0cbbed86d16",
            "5bc63fdb68b539f4fa500cfb2d0fe0f6"
        ];
        private static readonly string bloodLineUndeadFeatureGuid = "8a59e4af9f32950418260034c8b477fa";
        private static readonly string bloodLineUndeadArcanaGuid = "1a5e7191279e7cd479b17a6ca438498c";
        private static readonly string displayNameBloodLine = "BloodLineUndead.Name";
        private static readonly string displayNameBloodLineArcana = "BloodLineUndeadArcana.Name";
        private static readonly string displayDescriptionBloodLine = "BloodLineUndead.Description";

        private static readonly string bloodLineClassSkillGuid = "461a5980bfe5579468ba99d369f75308";
        private static readonly string bloodLineClassSkillDescription = "BloodlineUndeadClassSkill.Description";
        private static readonly string BloodlineUndeadSpellLevel1Guid = "3e4080a48cbd3154aac907befca64801";
        private static readonly string BloodlineUndeadSpellLevel1DisplayName = "BloodlineUndeadSpellLevel1.Name";
        private static readonly string BloodlineUndeadSpellLevel1Description = "BloodlineUndeadSpellLevel1.Description";
        
        private static readonly string immunityPrecisionDamageDisplayName = "ImmunityPrecisionDamage.Name";
        private static readonly string immunityPrecisionDamageDescription = "ImmunityPrecisionDamage.Description";

        public static void Configure()
        {
            BlueprintAbility RayOfenfeeblementAbility = BlueprintTool.Get<BlueprintAbility>("450af0402422b0b4980d9c2175869612");
            BlueprintFeature undeadType = BlueprintTool.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");
            BlueprintFeature undeadImmunities = BlueprintTool.Get<BlueprintFeature>("8a75eb16bfff86949a4ddcb3dd2f83ae");
            BlueprintFeature undeadTypeLevelUp = FeatureConfigurator.New("undeadTypeFeature15", "C3C79895-5F86-4942-82BC-D5EFC10A5493")
                .CopyFrom(undeadType,
                component =>
                {
                    return true;
                })
                .SetIcon(BlueprintTool.Get<BlueprintAbility>("57fcf8016cf04da4a8b33d2add14de7e").Icon)
                .SetDescription("undeadType.Description")
                .Configure(delayed: true);
            BlueprintFeature undeadImmunitiesLevelUp = FeatureConfigurator.New("undeadImmunitiesFeature", "52623305-1245-420E-8D60-D103F9C361E5")
                .CopyFrom(undeadImmunities,
                component =>
                {
                    return true;
                })
                .SetHideInCharacterSheetAndLevelUp(true)
                .SetHideInUI(true)
                .Configure(delayed: true);
            //修改血承名称，添加15/20级被动
            foreach (string guid in bloodLineUndeadProgressionGuids)
            {
                ProgressionConfigurator.For(guid)
                    .SetDisplayName(displayNameBloodLine)
                    .SetDescription(displayDescriptionBloodLine)
                    .AddToLevelEntry(15, [undeadTypeLevelUp, undeadImmunitiesLevelUp])
                    .Configure(delayed: true);
            }
            //修改血承名称
             FeatureConfigurator.For(bloodLineUndeadFeatureGuid)
                .SetDisplayName(displayNameBloodLine)
                .Configure(delayed: true);
            FeatureConfigurator.For(bloodLineUndeadArcanaGuid)
                .SetDisplayName(displayNameBloodLineArcana)
                .Configure(delayed: true);
            //修改血承本职技能描述
            FeatureConfigurator.For(bloodLineClassSkillGuid)
                .SetDescription(bloodLineClassSkillDescription)
                .Configure(delayed: true);
            //修改血承专长选择
            FeatureSelectionConfigurator.For("a29b72a804f7cb243b01e99c42452636")
                .AddToAllFeatures(BlueprintTool.Get<BlueprintFeature>("7f2b282626862e345935bbea5e66424b"))
                .SetDescription("UndeadFeatSelection.Description")
                .Configure();
            //修改血承3级技能
            FeatureConfigurator.For(BloodlineUndeadSpellLevel1Guid)
                .SetDisplayName(BloodlineUndeadSpellLevel1DisplayName)
                .SetDescription(BloodlineUndeadSpellLevel1Description)
                .SetIcon(RayOfenfeeblementAbility.Icon)
                .EditComponent<AddKnownSpell>(c => 
                    c.m_Spell = RayOfenfeeblementAbility.ToReference<BlueprintAbilityReference>()
                )
                .Configure(delayed: true);
            //修改17级凋死术图标
            FeatureConfigurator.For("ac5bf30c6b7e76248a2507047bd5703d")
                .SetIcon(BlueprintTool.Get<BlueprintAbility>("08323922485f7e246acb3d2276515526").Icon)
                .Configure(delayed: true);
            //修改19级吸能术图标
            FeatureConfigurator.For("273ac94653a5f3f4cafcac11499c2016")
                .SetIcon(BlueprintTool.Get<BlueprintAbility>("37302f72b06ced1408bf5bb965766d46").Icon)
                .Configure(delayed: true);
            //修改20级亡者之列
            FeatureConfigurator.For("b3e403ebbdad8314386270fefc4b4cc8")
                .AddImmunityToPrecisionDamage()
                .AddFacts([
                    undeadTypeLevelUp,
                    undeadImmunitiesLevelUp
                ])
                .SetDescription("UndeadOneOfUs.Description")
                .Configure(delayed: true);
            //幽冥之击增加污邪伤害
            BlueprintWeaponEnchantment LessBaneLivingEnchantment = 
            WeaponEnchantmentConfigurator.New("LessBaneLivingEnchantment", "F32B4F7A-A1FF-46DA-BB12-7FF821C2755C")
                .AddWeaponConditionalDamageDice(
                conditions: new ConditionsChecker
                {
                    Conditions = [
                        new ContextConditionHasFact{
                            m_Fact = BlueprintTool.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33").ToReference<BlueprintUnitFactReference>(),
                            Not = true
                        },
                        new ContextConditionHasFact{
                            m_Fact = BlueprintTool.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f").ToReference<BlueprintUnitFactReference>(),
                            Not = true
                        }
                    ]
                },
                damage: new DamageDescription
                {
                    Dice = new DiceFormula
                    {
                        m_Dice = DiceType.D6,
                        m_Rolls = 1
                    },
                    TypeDescription = new DamageTypeDescription
                    {
                        Energy = Kingmaker.Enums.Damage.DamageEnergyType.Unholy,
                        Type = DamageType.Energy
                    }
                },
                isBane: true)
                .Configure();
            FeatureConfigurator.For("84634e9e49342ce44ae6dd13b4279838")
                .AddBuffEnchantAnyWeapon(
                enchantmentBlueprint: LessBaneLivingEnchantment,
                slot: Kingmaker.UI.GenericSlot.EquipSlotBase.SlotType.PrimaryHand)
                .SetDescription("BloodragerUndeadGhostStrike.Description")
                .Configure();
            //额外法术修改
            BlueprintFeature BloodragerUndeadSpell7 = 
            FeatureConfigurator.For("ab8bb07310679d54b9023398df67bea9")
                .SetDisplayName("BloodragerUndeadSpell7.Name")
                .EditComponents<AddKnownSpell>(
                edit: c =>
                {
                    c.m_Spell = BlueprintTool.Get<BlueprintAbility>("dc6af3b4fd149f841912d8a3ce0983de").ToReference<BlueprintAbilityReference>();
                    c.SpellLevel = 4;
                },
                predicate: c => true)
                .Configure();
            ProgressionConfigurator.For("9f4ea90e9b9c27c48b541dbef184b3b7")
                .RemoveFromLevelEntries(
                level: 7,
                features: [BloodragerUndeadSpell7])
                .RemoveFromLevelEntries(
                level: 10,
                features: [BlueprintTool.Get<BlueprintFeature>("0898cf59a35ab7246a66dcec66a1b724")])
                .RemoveFromLevelEntries(
                level: 13,
                features: [BlueprintTool.Get<BlueprintFeature>("52d004368e4dfca40b79d40899a9d4d3")])
                .AddToLevelEntry(
                level: 7,
                features: [BlueprintTool.Get<BlueprintFeature>("0898cf59a35ab7246a66dcec66a1b724")])
                .AddToLevelEntry(
                level: 10,
                features: [BlueprintTool.Get<BlueprintFeature>("52d004368e4dfca40b79d40899a9d4d3")])
                .AddToLevelEntry(
                level: 13,
                features: [BloodragerUndeadSpell7])
                .Configure();
            //修改额外专长
            FeatureSelectionConfigurator.For("d54d36748d84548469653d09dfb312a4")
                .SetDescription("BloodragerUndeadFeatSelectionGreenrager.Description")
                .SetAllFeatures([
                    BlueprintTool.Get<BlueprintFeature>("86669ce8759f9d7478565db69b8c19ad"),
                    BlueprintTool.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5"),
                    BlueprintTool.Get<BlueprintFeature>("54ee847996c25cd4ba8773d7b8555174"),
                    BlueprintTool.Get<BlueprintFeature>("2a6091b97ad940943b46262600eaeaeb"),
                    BlueprintTool.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5"),
                    BlueprintTool.Get<BlueprintFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e"),
                    BlueprintTool.Get<BlueprintFeature>("31470b17e8446ae4ea0dacd6c5817d86"),
                    BlueprintTool.Get<BlueprintFeature>("8ac59959b1b23c347a0361dc97cc786d")
                ])
                .Configure();
            FeatureSelectionConfigurator.For("6980b5eb325e5fb47b12e54b31caff9e")
                .SetDescription("BloodragerUndeadFeatSelectionGreenrager.Description")
                .SetAllFeatures([
                    BlueprintTool.Get<BlueprintFeature>("86669ce8759f9d7478565db69b8c19ad"),
                    BlueprintTool.Get<BlueprintFeature>("97e216dbb46ae3c4faef90cf6bbe6fd5"),
                    BlueprintTool.Get<BlueprintFeature>("54ee847996c25cd4ba8773d7b8555174"),
                    BlueprintTool.Get<BlueprintFeature>("2a6091b97ad940943b46262600eaeaeb"),
                    BlueprintTool.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5"),
                    BlueprintTool.Get<BlueprintFeature>("1e1f627d26ad36f43bbd26cc2bf8ac7e"),
                    BlueprintTool.Get<BlueprintFeature>("31470b17e8446ae4ea0dacd6c5817d86"),
                    BlueprintTool.Get<BlueprintFeature>("8ac59959b1b23c347a0361dc97cc786d")
                ])
                .Configure();
            //亦生亦死增加不死生物
            FeatureConfigurator.For("47cc5432a72f71e49969c0396fbf24e8")
                .EditComponents<AddFacts>(
                edit: c =>
                {
                    var list = c.m_Facts.ToList(); 
                    list.Add(BlueprintTool.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33").ToReference<BlueprintUnitFactReference>());
                    list.Add(BlueprintTool.Get<BlueprintFeature>("8a75eb16bfff86949a4ddcb3dd2f83ae").ToReference<BlueprintUnitFactReference>());
                    c.m_Facts = list.ToArray();
                },
                predicate: c => true)
                .Configure();
        }
    }
}
