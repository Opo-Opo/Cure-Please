using System.Collections.Generic;
using EliteMMO.API;

namespace CurePlease.App
{
    public record DebuffRemovalSpell(string Name, bool PlEnabled, bool MonitoredEnabled)
    {
        public static Dictionary<StatusEffect, DebuffRemovalSpell> DebuffList()
            => new()
            {
                {
                    StatusEffect.Doom,
                    new("Cursna", OptionsForm.config.plDoom, OptionsForm.config.monitoredDoom)
                },
                {
                    StatusEffect.Paralysis,
                    new("Paralyna", OptionsForm.config.plParalysis, OptionsForm.config.monitoredParalysis)
                },
                //{StatusEffect.Amnesia, "Esuna", OptionsForm.config.plAmnesia, OptionsForm.config.monitoredAmnesia}, //BuffChecker(0, 418)
                {
                    StatusEffect.Poison,
                    new("Poisona", OptionsForm.config.plPoison, OptionsForm.config.monitoredPoison)
                },
                {
                    StatusEffect.Attack_Down,
                    new("Erase", OptionsForm.config.plAttackDown, OptionsForm.config.monitoredAttackDown)
                },
                {
                    StatusEffect.Blindness,
                    new("Blindna", OptionsForm.config.plBlindness, OptionsForm.config.monitoredBlindness)
                },
                {
                    StatusEffect.Bind,
                    new("Erase", OptionsForm.config.plAttackDown, OptionsForm.config.monitoredAttackDown)
                },
                {
                    StatusEffect.Weight,
                    new("Erase", OptionsForm.config.plWeight, OptionsForm.config.monitoredWeight)
                },
                {
                    StatusEffect.Slow,
                    new("Erase", OptionsForm.config.plSlow, OptionsForm.config.monitoredSlow)
                },
                {
                    StatusEffect.Curse,
                    new("Cursna", OptionsForm.config.plCurse, OptionsForm.config.monitoredCurse)
                },
                {
                    StatusEffect.Curse2,
                    new("Cursna", OptionsForm.config.plCurse2, OptionsForm.config.monitoredCurse2)
                },
                {
                    StatusEffect.Addle,
                    new("Erase", OptionsForm.config.plAddle, OptionsForm.config.monitoredAddle)
                },
                {
                    StatusEffect.Bane,
                    new("Cursna", OptionsForm.config.plBane, OptionsForm.config.monitoredBane)
                },
                {
                    StatusEffect.Plague,
                    new("Viruna", OptionsForm.config.plPlague, OptionsForm.config.monitoredPlague)
                },
                {
                    StatusEffect.Disease,
                    new("Viruna", OptionsForm.config.plDisease, OptionsForm.config.monitoredDisease)
                },
                {
                    StatusEffect.Burn,
                    new("Erase", OptionsForm.config.plBurn, OptionsForm.config.monitoredBurn)
                },
                {
                    StatusEffect.Frost,
                    new("Erase", OptionsForm.config.plFrost, OptionsForm.config.monitoredFrost)
                },
                {
                    StatusEffect.Choke,
                    new("Erase", OptionsForm.config.plChoke, OptionsForm.config.monitoredChoke)
                },
                {
                    StatusEffect.Rasp,
                    new("Erase", OptionsForm.config.plRasp, OptionsForm.config.monitoredRasp)
                },
                {
                    StatusEffect.Shock,
                    new("Erase", OptionsForm.config.plShock, OptionsForm.config.monitoredShock)
                },
                {
                    StatusEffect.Drown,
                    new("Erase", OptionsForm.config.plAttackDown, OptionsForm.config.monitoredAttackDown)
                },
                {
                    StatusEffect.Dia,
                    new("Erase", OptionsForm.config.plDia, OptionsForm.config.monitoredDia)
                },
                {
                    StatusEffect.Bio,
                    new("Erase", OptionsForm.config.plBio, OptionsForm.config.monitoredBio)
                },
                {
                    StatusEffect.STR_Down,
                    new("Erase", OptionsForm.config.plStrDown, OptionsForm.config.monitoredStrDown)
                },
                {
                    StatusEffect.DEX_Down,
                    new("Erase", OptionsForm.config.plDexDown, OptionsForm.config.monitoredDexDown)
                },
                {
                    StatusEffect.VIT_Down,
                    new("Erase", OptionsForm.config.plVitDown, OptionsForm.config.monitoredVitDown)
                },
                {
                    StatusEffect.AGI_Down,
                    new("Erase", OptionsForm.config.plAgiDown, OptionsForm.config.monitoredAgiDown)
                },
                {
                    StatusEffect.INT_Down,
                    new("Erase", OptionsForm.config.plIntDown, OptionsForm.config.monitoredIntDown)
                },
                {
                    StatusEffect.MND_Down,
                    new("Erase", OptionsForm.config.plMndDown, OptionsForm.config.monitoredMndDown)
                },
                {
                    StatusEffect.CHR_Down,
                    new("Erase", OptionsForm.config.plChrDown, OptionsForm.config.monitoredChrDown)
                },
                {
                    StatusEffect.Max_HP_Down,
                    new("Erase", OptionsForm.config.plMaxHpDown, OptionsForm.config.monitoredMaxHpDown)
                },
                {
                    StatusEffect.Max_MP_Down,
                    new("Erase", OptionsForm.config.plMaxMpDown, OptionsForm.config.monitoredMaxMpDown)
                },
                {
                    StatusEffect.Accuracy_Down,
                    new("Erase", OptionsForm.config.plAccuracyDown, OptionsForm.config.monitoredAccuracyDown)
                },
                {
                    StatusEffect.Evasion_Down,
                    new("Erase", OptionsForm.config.plEvasionDown, OptionsForm.config.monitoredEvasionDown)
                },
                {
                    StatusEffect.Defense_Down,
                    new("Erase", OptionsForm.config.plDefenseDown, OptionsForm.config.monitoredDefenseDown)
                },
                {
                    StatusEffect.Flash,
                    new("Erase", OptionsForm.config.plFlash, OptionsForm.config.monitoredFlash)
                },
                {
                    StatusEffect.Magic_Acc_Down,
                    new("Erase", OptionsForm.config.plMagicAccDown, OptionsForm.config.monitoredMagicAccDown)
                },
                {
                    StatusEffect.Magic_Atk_Down,
                    new("Erase", OptionsForm.config.plMagicAtkDown, OptionsForm.config.monitoredMagicAtkDown)
                },
                {
                    StatusEffect.Helix,
                    new("Erase", OptionsForm.config.plHelix, OptionsForm.config.monitoredHelix)
                },
                {
                    StatusEffect.Max_TP_Down,
                    new("Erase", OptionsForm.config.plMaxTpDown, OptionsForm.config.monitoredMaxTpDown)
                },
                {
                    StatusEffect.Requiem,
                    new("Erase", OptionsForm.config.plRequiem, OptionsForm.config.monitoredRequiem)
                },
                {
                    StatusEffect.Elegy,
                    new("Erase", OptionsForm.config.plElegy, OptionsForm.config.monitoredElegy)
                },
                {
                    StatusEffect.Threnody,
                    new("Erase", OptionsForm.config.plThrenody, OptionsForm.config.monitoredThrenody)
                },
            };
    }
}