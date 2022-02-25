using System.Collections.Generic;
using EliteMMO.API;

namespace CurePlease.App
{
    public record DebuffRemovalSpell(string Name, bool PlEnabled, bool MonitoredEnabled, bool PartyEnabled)
    {
        public static Dictionary<StatusEffect, DebuffRemovalSpell> DebuffList()
            => new()
            {
                {
                    StatusEffect.Doom,
                    new(
                        "Cursna",
                        OptionsForm.config.plDoom,
                        OptionsForm.config.monitoredDoom,
                        OptionsForm.config.naCurse
                    )
                },
                {
                    StatusEffect.Paralysis,
                    new(
                        "Paralyna",
                        OptionsForm.config.plParalysis,
                        OptionsForm.config.monitoredParalysis,
                        OptionsForm.config.naParalysis
                    )
                },
                //{StatusEffect.Amnesia, "Esuna", OptionsForm.config.plAmnesia, OptionsForm.config.monitoredAmnesia}, //BuffChecker(0, 418)
                {
                    StatusEffect.Poison,
                    new(
                        "Poisona",
                        OptionsForm.config.plPoison,
                        OptionsForm.config.monitoredPoison,
                        OptionsForm.config.naPoison
                    )
                },
                {
                    StatusEffect.Attack_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plAttackDown,
                        OptionsForm.config.monitoredAttackDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_AttackDown
                    )
                },
                {
                    StatusEffect.Blindness,
                    new(
                        "Blindna",
                        OptionsForm.config.plBlindness,
                        OptionsForm.config.monitoredBlindness,
                        OptionsForm.config.naBlindness
                    )
                },
                {
                    StatusEffect.Bind,
                    new(
                        "Erase",
                        OptionsForm.config.plAttackDown,
                        OptionsForm.config.monitoredAttackDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_Bind
                    )
                },
                {
                    StatusEffect.Weight,
                    new(
                        "Erase",
                        OptionsForm.config.plWeight,
                        OptionsForm.config.monitoredWeight,
                        OptionsForm.config.naErase && OptionsForm.config.na_Weight
                    )
                },
                {
                    StatusEffect.Slow,
                    new(
                        "Erase",
                        OptionsForm.config.plSlow,
                        OptionsForm.config.monitoredSlow,
                        OptionsForm.config.naErase && OptionsForm.config.na_Slow
                    )
                },
                {
                    StatusEffect.Curse,
                    new(
                        "Cursna",
                        OptionsForm.config.plCurse,
                        OptionsForm.config.monitoredCurse,
                        OptionsForm.config.naCurse
                    )
                },
                {
                    StatusEffect.Curse2,
                    new(
                        "Cursna",
                        OptionsForm.config.plCurse,
                        OptionsForm.config.monitoredCurse,
                        OptionsForm.config.naCurse
                    )
                },
                {
                    StatusEffect.Addle,
                    new(
                        "Erase",
                        OptionsForm.config.plAddle,
                        OptionsForm.config.monitoredAddle,
                        OptionsForm.config.naErase && OptionsForm.config.na_Addle
                    )
                },
                {
                    StatusEffect.Bane,
                    new(
                        "Cursna",
                        OptionsForm.config.plBane,
                        OptionsForm.config.monitoredBane,
                        OptionsForm.config.naCurse
                    )
                },
                {
                    StatusEffect.Plague,
                    new(
                        "Viruna",
                        OptionsForm.config.plPlague,
                        OptionsForm.config.monitoredPlague,
                        OptionsForm.config.naDisease
                    )
                },
                {
                    StatusEffect.Disease,
                    new(
                        "Viruna",
                        OptionsForm.config.plDisease,
                        OptionsForm.config.monitoredDisease,
                        OptionsForm.config.naDisease
                    )
                },
                {
                    StatusEffect.Burn,
                    new(
                        "Erase",
                        OptionsForm.config.plBurn,
                        OptionsForm.config.monitoredBurn,
                        OptionsForm.config.naErase && OptionsForm.config.na_Burn
                    )
                },
                {
                    StatusEffect.Frost,
                    new(
                        "Erase",
                        OptionsForm.config.plFrost,
                        OptionsForm.config.monitoredFrost,
                        OptionsForm.config.naErase && OptionsForm.config.na_Frost
                    )
                },
                {
                    StatusEffect.Choke,
                    new(
                        "Erase",
                        OptionsForm.config.plChoke,
                        OptionsForm.config.monitoredChoke,
                        OptionsForm.config.naErase && OptionsForm.config.na_Choke
                    )
                },
                {
                    StatusEffect.Rasp,
                    new(
                        "Erase",
                        OptionsForm.config.plRasp,
                        OptionsForm.config.monitoredRasp,
                        OptionsForm.config.naErase && OptionsForm.config.na_Rasp
                    )
                },
                {
                    StatusEffect.Shock,
                    new(
                        "Erase",
                        OptionsForm.config.plShock,
                        OptionsForm.config.monitoredShock,
                        OptionsForm.config.naErase && OptionsForm.config.na_Shock
                    )
                },
                {
                    StatusEffect.Drown,
                    new(
                        "Erase",
                        OptionsForm.config.plAttackDown,
                        OptionsForm.config.monitoredAttackDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_Drown
                    )
                },
                {
                    StatusEffect.Dia,
                    new(
                        "Erase",
                        OptionsForm.config.plDia,
                        OptionsForm.config.monitoredDia,
                        OptionsForm.config.naErase && OptionsForm.config.na_Dia
                    )
                },
                {
                    StatusEffect.Bio,
                    new(
                        "Erase",
                        OptionsForm.config.plBio,
                        OptionsForm.config.monitoredBio,
                        OptionsForm.config.naErase && OptionsForm.config.na_Bio
                    )
                },
                {
                    StatusEffect.STR_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plStrDown,
                        OptionsForm.config.monitoredStrDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_StrDown
                    )
                },
                {
                    StatusEffect.DEX_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plDexDown,
                        OptionsForm.config.monitoredDexDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_DexDown
                    )
                },
                {
                    StatusEffect.VIT_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plVitDown,
                        OptionsForm.config.monitoredVitDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_VitDown
                    )
                },
                {
                    StatusEffect.AGI_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plAgiDown,
                        OptionsForm.config.monitoredAgiDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_AgiDown
                    )
                },
                {
                    StatusEffect.INT_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plIntDown,
                        OptionsForm.config.monitoredIntDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_IntDown
                    )
                },
                {
                    StatusEffect.MND_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plMndDown,
                        OptionsForm.config.monitoredMndDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_MndDown
                    )
                },
                {
                    StatusEffect.CHR_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plChrDown,
                        OptionsForm.config.monitoredChrDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_ChrDown
                    )
                },
                {
                    StatusEffect.Max_HP_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plMaxHpDown,
                        OptionsForm.config.monitoredMaxHpDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_MaxHpDown
                    )
                },
                {
                    StatusEffect.Max_MP_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plMaxMpDown,
                        OptionsForm.config.monitoredMaxMpDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_MaxMpDown
                    )
                },
                {
                    StatusEffect.Accuracy_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plAccuracyDown,
                        OptionsForm.config.monitoredAccuracyDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_MagicAccDown
                    )
                },
                {
                    StatusEffect.Evasion_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plEvasionDown,
                        OptionsForm.config.monitoredEvasionDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_EvasionDown
                    )
                },
                {
                    StatusEffect.Defense_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plDefenseDown,
                        OptionsForm.config.monitoredDefenseDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_DefenseDown
                    )
                },
                {
                    StatusEffect.Flash,
                    new(
                        "Erase",
                        OptionsForm.config.plFlash,
                        OptionsForm.config.monitoredFlash,
                        OptionsForm.config.naErase
                    )
                },
                {
                    StatusEffect.Magic_Acc_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plMagicAccDown,
                        OptionsForm.config.monitoredMagicAccDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_MagicAccDown
                    )
                },
                {
                    StatusEffect.Magic_Atk_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plMagicAtkDown,
                        OptionsForm.config.monitoredMagicAtkDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_MagicAttackDown
                    )
                },
                {
                    StatusEffect.Helix,
                    new(
                        "Erase",
                        OptionsForm.config.plHelix,
                        OptionsForm.config.monitoredHelix,
                        OptionsForm.config.naErase && OptionsForm.config.na_Helix
                    )
                },
                {
                    StatusEffect.Max_TP_Down,
                    new(
                        "Erase",
                        OptionsForm.config.plMaxTpDown,
                        OptionsForm.config.monitoredMaxTpDown,
                        OptionsForm.config.naErase && OptionsForm.config.na_MaxTpDown
                    )
                },
                {
                    StatusEffect.Requiem,
                    new(
                        "Erase",
                        OptionsForm.config.plRequiem,
                        OptionsForm.config.monitoredRequiem,
                        OptionsForm.config.naErase && OptionsForm.config.na_Requiem
                    )
                },
                {
                    StatusEffect.Elegy,
                    new(
                        "Erase",
                        OptionsForm.config.plElegy,
                        OptionsForm.config.monitoredElegy,
                        OptionsForm.config.naErase && OptionsForm.config.na_Elegy
                    )
                },
                {
                    StatusEffect.Threnody,
                    new(
                        "Erase",
                        OptionsForm.config.plThrenody,
                        OptionsForm.config.monitoredThrenody,
                        OptionsForm.config.naErase && OptionsForm.config.na_Threnody
                    )
                },
            };
    }
}