using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace CruoromancerTweaks.ModifiedContent.Classes
{
    internal class Warpriest
    {
        public static void Configure()
        {
            //坟场来客增加负能量亲和与不死生物
            BlueprintBuff NegativeEnergyAffinityBuff =
                BuffConfigurator.New("NegativeEnergyAffinityBuff", "713B037A-6936-4271-98DE-12EAF0049108")
                .AddEnergyImmunity(DamageEnergyType.NegativeEnergy)
                .SetFlags(BlueprintBuff.Flags.HiddenInUi)
                .Configure();
            BlueprintBuff UndeadBuff =
                BuffConfigurator.New("UndeadBuff", "EBCDCD92-4087-40CE-8B18-F65DFD3AE5AF")
                .AddFacts([
                    BlueprintTool.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33"),
                    BlueprintTool.Get<BlueprintFeature>("8a75eb16bfff86949a4ddcb3dd2f83ae")
                    ])
                //.SetFlags(BlueprintBuff.Flags.HiddenInUi)
                .Configure();
            AbilityConfigurator.For("542a9661ed997ff418322ff7376bab8c")
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    var list = c.Actions.Actions.ToList();
                    list.Add(
                        new Conditional
                        {
                            ConditionsChecker = new ConditionsChecker
                            {
                                Conditions = [
                                    new ContextConditionCompare
                                    {
                                        m_Type = ContextConditionCompare.Type.GreaterOrEqual,
                                        CheckValue = new ContextValue
                                        {
                                            ValueType = ContextValueType.CasterProperty,
                                            Property = UnitProperty.Level
                                        },
                                        TargetValue = new ContextValue
                                        {
                                            ValueType = ContextValueType.Simple,
                                            Value = 8
                                        }
                                    }
                                ]
                            },
                            IfTrue = new ActionList
                            {
                                Actions = [
                                    new ContextActionApplyBuff
                                    {
                                        m_Buff = NegativeEnergyAffinityBuff.ToReference<BlueprintBuffReference>(),
                                        DurationValue = new ContextDurationValue
                                        {
                                            BonusValue = new ContextValue
                                            {
                                                Value = 1
                                            },
                                            DiceCountValue = new ContextValue{
                                                Value = 0
                                            },
                                            DiceType = DiceType.Zero,
                                            Rate = DurationRate.Minutes
                                        }
                                    }
                                ]
                            }
                        });
                    list.Add(
                        new Conditional
                        {
                            ConditionsChecker = new ConditionsChecker
                            {
                                Conditions = [
                                    new ContextConditionCompare
                                    {
                                        m_Type = ContextConditionCompare.Type.GreaterOrEqual,
                                        CheckValue = new ContextValue
                                        {
                                            ValueType = ContextValueType.CasterProperty,
                                            Property = UnitProperty.Level
                                        },
                                        TargetValue = new ContextValue
                                        {
                                            ValueType = ContextValueType.Simple,
                                            Value = 15
                                        }
                                    }
                                ]
                            },
                            IfTrue = new ActionList
                            {
                                Actions = [
                                    new ContextActionApplyBuff
                                    {
                                        m_Buff = UndeadBuff.ToReference<BlueprintBuffReference>(),
                                        DurationValue = new ContextDurationValue
                                        {
                                            BonusValue = new ContextValue
                                            {
                                                Value = 1
                                            },
                                            DiceCountValue = new ContextValue{
                                                Value = 0
                                            },
                                            DiceType = DiceType.Zero,
                                            Rate = DurationRate.Minutes
                                        }
                                    }
                                ]
                            }
                        });
                    c.Actions.Actions = list.ToArray();
                })
                .Configure();
            //死亡之触增强
            AbilityConfigurator.For("04dd71ac77051bc46aab114d200e65dd")
                .SetDescription("DeathBlessingMajorAbility.Description")
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    c.Actions.Actions = [
                        new ContextActionDealDamage{
                            CriticalSharedValue = AbilitySharedValue.Damage,
                            DamageType = new DamageTypeDescription{
                                Physical = new DamageTypeDescription.PhysicalData{
                                    Form = PhysicalDamageForm.Slashing
                                },
                                Type = DamageType.Physical
                            },
                            Duration = new ContextDurationValue{
                                BonusValue = new ContextValue{
                                    Value = 1
                                },
                                DiceCountValue = new ContextValue{
                                    Value = 0
                                },
                                DiceType = DiceType.Zero,
                                Rate = DurationRate.Hours
                            },
                            EnergyDrainType = EnergyDrainType.Temporary,
                            m_Type = ContextActionDealDamage.Type.EnergyDrain,
                            PreRolledSharedValue = AbilitySharedValue.Damage,
                            ResultSharedValue = AbilitySharedValue.Damage,
                            Value = new ContextDiceValue{
                                BonusValue = new ContextValue{
                                    Value = 0
                                },
                                DiceType = DiceType.D4,
                                DiceCountValue = new ContextValue{
                                    Value = 1
                                }
                            }
                        }
                    ];
                })
                .Configure();
            AbilityConfigurator.For("d3dba848088e1a64582f76108b778fd0")
                .SetDescription("DeathBlessingMajorAbility.Description")
                .Configure();
            BuffConfigurator.For("84cd1f14dff45f4479603d753dcfbf0b")
                .SetDescription("DeathBlessingMajorAbility.Description")
                .EditComponent<AddInitiatorAttackWithWeaponTrigger>(c =>
                {
                    var list = c.Action.Actions.ToList();
                    list.RemoveRange(0, 1);
                    list.Add(new ContextActionDealDamage
                    {
                        CriticalSharedValue = AbilitySharedValue.Damage,
                        DamageType = new DamageTypeDescription
                        {
                            Physical = new DamageTypeDescription.PhysicalData
                            {
                                Form = PhysicalDamageForm.Slashing
                            },
                            Type = DamageType.Physical
                        },
                        Duration = new ContextDurationValue
                        {
                            BonusValue = new ContextValue
                            {
                                Value = 1
                            },
                            DiceCountValue = new ContextValue
                            {
                                Value = 0
                            },
                            DiceType = DiceType.Zero,
                            Rate = DurationRate.Hours
                        },
                        EnergyDrainType = EnergyDrainType.Temporary,
                        m_Type = ContextActionDealDamage.Type.EnergyDrain,
                        PreRolledSharedValue = AbilitySharedValue.Damage,
                        ResultSharedValue = AbilitySharedValue.Damage,
                        Value = new ContextDiceValue
                        {
                            BonusValue = new ContextValue
                            {
                                Value = 0
                            },
                            DiceType = DiceType.D4,
                            DiceCountValue = new ContextValue
                            {
                                Value = 1
                            }
                        }
                    });
                    c.Action.Actions = list.ToArray();
                })
                .Configure();
        }
    }
}
