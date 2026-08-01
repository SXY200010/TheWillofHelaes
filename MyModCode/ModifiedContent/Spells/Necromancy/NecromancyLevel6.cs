using BlueprintCore.Actions.Builder;
using BlueprintCore.Actions.Builder.ContextEx;
using BlueprintCore.Blueprints.Configurators.UnitLogic.ActivatableAbilities;
using BlueprintCore.Blueprints.CustomConfigurators;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Utils;
using BlueprintCore.Utils.Types;
using CruoromancerTweaks.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Utility;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.EntitySystem.Properties.BaseGetter.PropertyContextAccessor;
namespace CruoromancerTweaks.ModifiedContent.Spells.Necromancy
{
    internal class NecromancyLevel6
    {
        private static readonly string UndeathToDeathName = "UndeathToDeath.Name";

        private static readonly string SiphonLifeDescription = "SiphonLife.Description";
        private static readonly string CircleOfDeathDescription = "CircleOfDeath.Description";
        private static readonly string UndeathToDeathDescription = "UndeathToDeath.Description";
        private static readonly string HarmDamageDescription = "HarmDamage.Description";
        private static readonly string EyebiteName = "Eyebite.Name";
        private static readonly string EyebiteDescription = "Eyebite.Description";



        private const string EyebiteAreaGuid = "9EA6CE1EB3EC455E8A66B50F2C43AC97";
        public static void Configure()
        {
            BlueprintAbility SiphonLife = BlueprintTool.Get<BlueprintAbility>("7bd52a86498c7854ebe99bc3cfb85bfe");

            BlueprintBuff Fatigued = BlueprintTool.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");

            AbilityConfigurator.For(SiphonLife)
                .SetDescription(SiphonLifeDescription)
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    foreach (var rootAction in c.Actions.Actions)
                    {
                        if (rootAction is ContextActionDealDamage dealDamage)
                        {
                            dealDamage.Value = new ContextDiceValue()
                            {
                                DiceType = DiceType.D8,
                                DiceCountValue = new ContextValue
                                {
                                    ValueType = ContextValueType.Rank
                                },
                                BonusValue = 0
                            };
                        }
                    }
                })
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    var list = c.Actions.Actions.ToList();

                    list.Add(new ContextActionApplyBuff
                    {
                        m_Buff = Fatigued.ToReference<BlueprintBuffReference>(),
                        DurationValue = new ContextDurationValue
                        {
                            Rate = DurationRate.Rounds,
                            DiceType = DiceType.Zero,
                            DiceCountValue = 0,
                            BonusValue = 0
                        },
                        Permanent = true
                    });

                    c.Actions.Actions = list.ToArray();
                })
                .Configure();

            BlueprintAbility Eyebite = BlueprintTool.Get<BlueprintAbility>("3167d30dd3c622c46b0c0cb242061642");
            BlueprintAbility EyebiteAbility = BlueprintTool.Get<BlueprintAbility>("582009cf6013790469d6e98e5210477a");

            BlueprintBuff EyebiteBuff = BlueprintTool.Get<BlueprintBuff>("50827f87d113b194f9fc772a47ae2b58");
            BlueprintBuff Sickened = BlueprintTool.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            BlueprintBuff Frightened = BlueprintTool.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf");
            BlueprintBuff Paralyzed = BlueprintTool.Get<BlueprintBuff>("af1e2d232ebbb334aaf25e2a46a92591");
            BlueprintBuff Nauseated = BlueprintTool.Get<BlueprintBuff>("956331dba5125ef48afe41875a00ca0e");

            BlueprintAbilityAreaEffect EyebiteArea =
                AbilityAreaEffectConfigurator.New("EyebiteArea", EyebiteAreaGuid)
                    .SetSize(30.Feet())
                    .SetShape(AreaEffectShape.Cylinder)
                    .SetTargetType(BlueprintAbilityAreaEffect.TargetType.Any)
                    .SetAffectEnemies(true)
                    .SetAggroEnemies(true)
                    .AddContextRankConfig(
                        ContextRankConfigs.CasterLevel()
                    )
                    .AddComponent<AbilityAreaEffectRunAction>(c =>
                    {
                        c.UnitEnter = CreateEyebiteAreaAction(Sickened, Frightened, Paralyzed, Nauseated);
                        c.Round = CreateEyebiteAreaAction(Sickened, Frightened, Paralyzed, Nauseated);
                    })
                    .Configure();

            BuffConfigurator.For(EyebiteBuff)
                .AddAreaEffect(EyebiteArea.ToReference<BlueprintAbilityAreaEffectReference>())
                .Configure();

            AbilityConfigurator.For(Eyebite)
                .SetRange(AbilityRange.Personal)
                .SetCanTargetEnemies(false)
                .SetCanTargetFriends(false)
                .SetCanTargetSelf(true)
                .RemoveComponents(c => c is AbilityEffectRunAction)
                .SetDisplayName(EyebiteName)
                .SetDescription(EyebiteDescription)
                .AddComponent<AbilityEffectRunAction>(c =>
                {
                    c.Actions = new ActionList
                    {
                        Actions = new GameAction[]
                        {
                            new ContextActionApplyBuff
                            {
                                m_Buff = EyebiteBuff.ToReference<BlueprintBuffReference>(),
                                ToCaster = true,
                                DurationValue = new ContextDurationValue
                                {
                                    Rate = DurationRate.Rounds,
                                    DiceType = DiceType.Zero,
                                    DiceCountValue = 0,
                                    BonusValue = new ContextValue
                                    {
                                        ValueType = ContextValueType.Rank,
                                        ValueRank = AbilityRankType.Default
                                    }
                                }
                            }
                        }
                    };
                })
                .Configure();

            AbilityConfigurator.For(EyebiteAbility)
                .SetDisplayName(EyebiteName)
                .SetDescription(EyebiteDescription)
                .Configure();

            BlueprintAbility CircleOfDeath = BlueprintTool.Get<BlueprintAbility>("a89dcbbab8f40e44e920cc60636097cf");
            AbilityConfigurator.For(CircleOfDeath)
                .SetDescription(CircleOfDeathDescription)
                .EditComponents<ContextCalculateSharedValue>(
                    c =>
                    {
                        c.Value = new ContextDiceValue
                        {
                            DiceType = DiceType.D6,
                            DiceCountValue = new ContextValue
                            {
                                ValueType = ContextValueType.Rank,
                                ValueRank = AbilityRankType.Default,
                            },
                            BonusValue = 0
                        };
                    },
                    c => c.ValueType != AbilitySharedValue.Heal
                )
                .AddComponent<ContextRankConfig>(c =>
                {
                    c.m_Type = AbilityRankType.DamageBonus;
                    c.m_BaseValueType = ContextRankBaseValueType.CasterLevel;
                    c.m_Progression = ContextRankProgression.AsIs;
                    c.m_Max = 10;
                })
                .AddComponent<ContextCalculateSharedValue>(c =>
                {
                    c.ValueType = AbilitySharedValue.Heal; 
                    c.Value = new ContextDiceValue
                    {
                        DiceType = DiceType.Zero,
                        DiceCountValue = 0,
                        BonusValue = new ContextValue
                        {
                            ValueType = ContextValueType.Rank,
                            ValueRank = AbilityRankType.DamageBonus,
                            Value = 9
                        }
                    };
                })
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    foreach (var rootAction in c.Actions.Actions)
                    {
                        ActionTreeUtils.Walk(rootAction, a =>
                        {
                            if (a is Conditional cond)
                            {
                                foreach (var condition in cond.ConditionsChecker.Conditions)
                                {
                                    if (condition is ContextConditionHitDice hpCond && hpCond.HitDice == 9)
                                    {
                                        hpCond.SharedValue = AbilitySharedValue.Heal;
                                    }
                                }
                            }
                        });
                    }
                })
                .Configure();

            BlueprintAbility UndeathToDeath = BlueprintTool.Get<BlueprintAbility>("a9a52760290591844a96d0109e30e04d");
            AbilityConfigurator.For(UndeathToDeath)
                .SetDescription(UndeathToDeathDescription)
                .SetDisplayName(UndeathToDeathName)
                .EditComponents<ContextCalculateSharedValue>(
                    c =>
                    {
                        c.Value = new ContextDiceValue
                        {
                            DiceType = DiceType.D6,
                            DiceCountValue = new ContextValue
                            {
                                ValueType = ContextValueType.Rank,
                                ValueRank = AbilityRankType.Default,
                            },
                            BonusValue = 0
                        };
                    },
                    c => c.ValueType != AbilitySharedValue.Heal
                )
                .AddComponent<ContextRankConfig>(c =>
                {
                    c.m_Type = AbilityRankType.DamageBonus;
                    c.m_BaseValueType = ContextRankBaseValueType.CasterLevel;
                    c.m_Progression = ContextRankProgression.AsIs;
                    c.m_Max = 10;
                })
                .AddComponent<ContextCalculateSharedValue>(c =>
                {
                    c.ValueType = AbilitySharedValue.Heal;
                    c.Value = new ContextDiceValue
                    {
                        DiceType = DiceType.Zero,
                        DiceCountValue = 0,
                        BonusValue = new ContextValue
                        {
                            ValueType = ContextValueType.Rank,
                            ValueRank = AbilityRankType.DamageBonus,
                            Value = 9
                        }
                    };
                })
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    foreach (var rootAction in c.Actions.Actions)
                    {
                        ActionTreeUtils.Walk(rootAction, a =>
                        {
                            if (a is Conditional cond)
                            {
                                foreach (var condition in cond.ConditionsChecker.Conditions)
                                {
                                    if (condition is ContextConditionHitDice hpCond && hpCond.HitDice == 9)
                                    {
                                        hpCond.SharedValue = AbilitySharedValue.Heal;
                                    }
                                }
                            }
                        });
                    }
                })
                .Configure();


            BlueprintAbility HarmDamage = BlueprintTool.Get<BlueprintAbility>("3da67f8b941308348b7101e7ef418f52");
            AbilityConfigurator.For(HarmDamage)
                .SetDescription(HarmDamageDescription)
                .EditComponent<ContextRankConfig>(c =>
                {
                    c.m_Max = 200;
                })
                .Configure();
        }

        private static ActionList CreateEyebiteAreaAction(
        BlueprintBuff sickened,
        BlueprintBuff frightened,
        BlueprintBuff paralyzed,
        BlueprintBuff nauseated)
        {
            return new ActionList
            {
                Actions = new GameAction[]
                {
                    new Conditional
                    {
                        ConditionsChecker = new ConditionsChecker
                        {
                            Operation = Operation.And,
                            Conditions = new Condition[]
                            {
                                new ContextConditionIsEnemy()
                            }
                        },

                        IfTrue = new ActionList
                        {
                            Actions = new GameAction[]
                            {
                                new ContextActionSavingThrow
                                {
                                    Type = SavingThrowType.Fortitude,
                                    Actions = new ActionList
                                    {
                                        Actions = new GameAction[]
                                        {
                                            new ContextActionConditionalSaved
                                            {
                                                Succeed = new ActionList
                                                {
                                                    Actions = Array.Empty<GameAction>()
                                                },

                                                Failed = new ActionList
                                                {
                                                    Actions = new GameAction[]
                                                    {
                                                        // HD <= 9：Sickened + Frightened
                                                        new Conditional
                                                        {
                                                            ConditionsChecker = new ConditionsChecker
                                                            {
                                                                Operation = Operation.And,
                                                                Conditions = new Condition[]
                                                                {
                                                                    new ContextConditionHitDice
                                                                    {
                                                                        HitDice = 9,
                                                                        SharedValue = AbilitySharedValue.Damage,
                                                                        Not = false
                                                                    }
                                                                }
                                                            },

                                                            IfTrue = new ActionList
                                                            {
                                                                Actions = new GameAction[]
                                                                {
                                                                    new ContextActionApplyBuff
                                                                    {
                                                                        m_Buff = sickened.ToReference<BlueprintBuffReference>(),
                                                                        DurationValue = new ContextDurationValue
                                                                        {
                                                                            Rate = DurationRate.TenMinutes,
                                                                            DiceType = DiceType.Zero,
                                                                            DiceCountValue = 0,
                                                                            BonusValue = new ContextValue
                                                                            {
                                                                                ValueType = ContextValueType.Rank,
                                                                                ValueRank = AbilityRankType.Default
                                                                            }
                                                                        }
                                                                    },

                                                                    new ContextActionApplyBuff
                                                                    {
                                                                        m_Buff = frightened.ToReference<BlueprintBuffReference>(),
                                                                        DurationValue = new ContextDurationValue
                                                                        {
                                                                            Rate = DurationRate.Rounds,
                                                                            DiceType = DiceType.D4,
                                                                            DiceCountValue = 1,
                                                                            BonusValue = 0
                                                                        }
                                                                    },

                                                                    // HD <= 4：额外 Paralyzed
                                                                    new Conditional
                                                                    {
                                                                        ConditionsChecker = new ConditionsChecker
                                                                        {
                                                                            Operation = Operation.And,
                                                                            Conditions = new Condition[]
                                                                            {
                                                                                new ContextConditionHitDice
                                                                                {
                                                                                    HitDice = 4,
                                                                                    SharedValue = AbilitySharedValue.Damage,
                                                                                    Not = false
                                                                                }
                                                                            }
                                                                        },

                                                                        IfTrue = new ActionList
                                                                        {
                                                                            Actions = new GameAction[]
                                                                            {
                                                                                new ContextActionApplyBuff
                                                                                {
                                                                                    m_Buff = paralyzed.ToReference<BlueprintBuffReference>(),
                                                                                    DurationValue = new ContextDurationValue
                                                                                    {
                                                                                        Rate = DurationRate.TenMinutes,
                                                                                        DiceType = DiceType.Zero,
                                                                                        DiceCountValue = 0,
                                                                                        BonusValue = new ContextValue
                                                                                        {
                                                                                            ValueType = ContextValueType.Rank,
                                                                                            ValueRank = AbilityRankType.Default
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        },

                                                                        IfFalse = new ActionList
                                                                        {
                                                                            Actions = Array.Empty<GameAction>()
                                                                        }
                                                                    }
                                                                }
                                                            },

                                                            // HD 10 以上：如果已经 Sickened，再次失败变 Nauseated；否则先 Sickened
                                                            IfFalse = new ActionList
                                                            {
                                                                Actions = new GameAction[]
                                                                {
                                                                    new Conditional
                                                                    {
                                                                        ConditionsChecker = new ConditionsChecker
                                                                        {
                                                                            Operation = Operation.And,
                                                                            Conditions = new Condition[]
                                                                            {
                                                                                new ContextConditionHasFact
                                                                                {
                                                                                    m_Fact = sickened.ToReference<BlueprintUnitFactReference>(),
                                                                                    Not = false
                                                                                }
                                                                            }
                                                                        },

                                                                        IfTrue = new ActionList
                                                                        {
                                                                            Actions = new GameAction[]
                                                                            {
                                                                                new ContextActionApplyBuff
                                                                                {
                                                                                    m_Buff = nauseated.ToReference<BlueprintBuffReference>(),
                                                                                    DurationValue = new ContextDurationValue
                                                                                    {
                                                                                        Rate = DurationRate.TenMinutes,
                                                                                        DiceType = DiceType.Zero,
                                                                                        DiceCountValue = 0,
                                                                                        BonusValue = new ContextValue
                                                                                        {
                                                                                            ValueType = ContextValueType.Rank,
                                                                                            ValueRank = AbilityRankType.Default
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        },

                                                                        IfFalse = new ActionList
                                                                        {
                                                                            Actions = new GameAction[]
                                                                            {
                                                                                new ContextActionApplyBuff
                                                                                {
                                                                                    m_Buff = sickened.ToReference<BlueprintBuffReference>(),
                                                                                    DurationValue = new ContextDurationValue
                                                                                    {
                                                                                        Rate = DurationRate.TenMinutes,
                                                                                        DiceType = DiceType.Zero,
                                                                                        DiceCountValue = 0,
                                                                                        BonusValue = new ContextValue
                                                                                        {
                                                                                            ValueType = ContextValueType.Rank,
                                                                                            ValueRank = AbilityRankType.Default
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },

                        IfFalse = new ActionList
                        {
                            Actions = Array.Empty<GameAction>()
                        }
                    }
                }
            };
        }
    }
}
