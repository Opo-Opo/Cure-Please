using System;
using System.Collections.Generic;
using System.Linq;
using CurePlease.Xi;
using EliteMMO.API;

namespace CurePlease.Domain
{
    public class DebuffRemoval
    {
        private readonly Func<string, bool> _removalEnabledCheck;
        private readonly Func<string, bool> _canCastSpell;

        private static IEnumerable<DebuffMap> _map = new List<DebuffMap>()
        {
            new DebuffMap(StatusEffect.Doom, SpellId.Cursna, "Doom"),
            new DebuffMap(StatusEffect.Silence, SpellId.Silena, "Silence"),
            new DebuffMap(StatusEffect.Petrification, SpellId.Stona, "Petrification"),
            new DebuffMap(StatusEffect.Paralysis, SpellId.Paralyna, "Paralysis"),
            new DebuffMap(StatusEffect.Poison, SpellId.Poisona, "Poison"),
            new DebuffMap(StatusEffect.Attack_Down, SpellId.Erase, "AttackDown"),
            new DebuffMap(StatusEffect.Blindness, SpellId.Blindna, "Blindness"),
            new DebuffMap(StatusEffect.Bind, SpellId.Erase, "Bind"),
            new DebuffMap(StatusEffect.Weight, SpellId.Erase, "Weight"),
            new DebuffMap(StatusEffect.Slow, SpellId.Erase, "Slow"),
            new DebuffMap(StatusEffect.Curse, SpellId.Cursna, "Curse"),
            new DebuffMap(StatusEffect.Curse2, SpellId.Cursna, "Curse2"),
            new DebuffMap(StatusEffect.Addle, SpellId.Erase, "Addle"),
            new DebuffMap(StatusEffect.Bane, SpellId.Cursna, "Bane"),
            new DebuffMap(StatusEffect.Plague, SpellId.Viruna, "Plague"),
            new DebuffMap(StatusEffect.Disease, SpellId.Viruna, "Disease"),
            new DebuffMap(StatusEffect.Burn, SpellId.Erase, "Burn"),
            new DebuffMap(StatusEffect.Frost, SpellId.Erase, "Frost"),
            new DebuffMap(StatusEffect.Choke, SpellId.Erase, "Choke"),
            new DebuffMap(StatusEffect.Rasp, SpellId.Erase, "Rasp"),
            new DebuffMap(StatusEffect.Shock, SpellId.Erase, "Shock"),
            new DebuffMap(StatusEffect.Drown, SpellId.Erase, "Drown"),
            new DebuffMap(StatusEffect.Dia, SpellId.Erase, "Dia"),
            new DebuffMap(StatusEffect.Bio, SpellId.Erase, "Bio"),
            new DebuffMap(StatusEffect.STR_Down, SpellId.Erase, "StrDown"),
            new DebuffMap(StatusEffect.DEX_Down, SpellId.Erase, "DexDown"),
            new DebuffMap(StatusEffect.VIT_Down, SpellId.Erase, "VitDown"),
            new DebuffMap(StatusEffect.AGI_Down, SpellId.Erase, "AgiDown"),
            new DebuffMap(StatusEffect.INT_Down, SpellId.Erase, "IntDown"),
            new DebuffMap(StatusEffect.MND_Down, SpellId.Erase, "MndDown"),
            new DebuffMap(StatusEffect.CHR_Down, SpellId.Erase, "ChrDown"),
            new DebuffMap(StatusEffect.Max_HP_Down, SpellId.Erase, "MaxHpDown"),
            new DebuffMap(StatusEffect.Max_MP_Down, SpellId.Erase, "MaxMpDown"),
            new DebuffMap(StatusEffect.Accuracy_Down, SpellId.Erase, "AccuracyDown"),
            new DebuffMap(StatusEffect.Evasion_Down, SpellId.Erase, "EvasionDown"),
            new DebuffMap(StatusEffect.Defense_Down, SpellId.Erase, "DefenseDown"),
            new DebuffMap(StatusEffect.Flash, SpellId.Erase, "Flash"),
            new DebuffMap(StatusEffect.Magic_Acc_Down, SpellId.Erase, "MagicAccDown"),
            new DebuffMap(StatusEffect.Magic_Atk_Down, SpellId.Erase, "MagicAtkDown"),
            new DebuffMap(StatusEffect.Helix, SpellId.Erase, "Helix"),
            new DebuffMap(StatusEffect.Max_TP_Down, SpellId.Erase, "MaxTpDown"),
            new DebuffMap(StatusEffect.Requiem, SpellId.Erase, "Requiem"),
            new DebuffMap(StatusEffect.Elegy, SpellId.Erase, "Elegy"),
            new DebuffMap(StatusEffect.Threnody, SpellId.Erase, "Threnody"),
        };

        public DebuffRemoval(Func<string, bool> removalEnabledCheck, Func<string, bool> canCastSpell)
        {
            _removalEnabledCheck = removalEnabledCheck;
            _canCastSpell = canCastSpell;
        }

        public string Spell(StatusEffect statusEffect, bool targetInPlParty)
        {
            return _map
                .Where(map => FilterPlPartyOnlySpells(targetInPlParty, map))
                .Where(map => _removalEnabledCheck(map.EnabledProperty))
                .Where(map => _canCastSpell(map.RemovalSpell.ToString()))
                .First(map => map.Status == statusEffect)
                .RemovalSpell.ToString();
        }

        private static bool FilterPlPartyOnlySpells(bool targetInPlParty, DebuffMap map)
        {
            return map.RemovalSpell != SpellId.Erase || targetInPlParty;
        }
    }

    internal class DebuffMap
    {
        public DebuffMap(StatusEffect status, SpellId removalSpell, string enabledProperty)
        {
            Status = status;
            EnabledProperty = enabledProperty;
            RemovalSpell = removalSpell;
        }

        public readonly StatusEffect Status;
        public readonly string EnabledProperty;
        public readonly SpellId RemovalSpell;
    }
}
