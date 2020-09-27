using System;
using System.Collections.Generic;

namespace CurePlease.Domain
{
    public class CureTiers
    {
        private readonly Func<string, bool> _canCastSpell;
        private readonly bool _overCureEnabled;
        private readonly bool _underCureEnabled;
        private readonly bool _overCurePriority;

        private static Dictionary<string, Cure> _teirs = new Dictionary<string, Cure>()
        {
            {
                "Cure VI", 
                new Cure(null, "Cure V")
            },
            {
                "Cure V", 
                new Cure("Cure VI", "Cure IV", true)
            },
            {
                "Cure IV", 
                new Cure("Cure V", "Cure III", true)
            },
            {
                "Cure III", 
                new Cure("Cure IV", "Cure II", true)
            },
            {
                "Cure II", 
                new Cure("Cure III", "Cure", true)
            },
            {
                "Cure", 
                new Cure("Cure II", null, true)
            },
            {
                "Curaga V", 
                new Cure(null, "Curaga IV")
            },
            {
                "Curaga IV", 
                new Cure("Curaga V", "Curaga III")
            },
            {
                "Curaga III", 
                new Cure("Curaga IV", "Curaga II")
            },
            {
                "Curaga II", 
                new Cure("Curaga III", "Curaga")
            },
            {
                "Curaga", 
                new Cure("Curaga II", null)
            },
        };

        public CureTiers(Func<string, bool> canCastSpell, bool overCureEnabled, bool underCureEnabled, bool overCurePriority)
        {
            _canCastSpell = canCastSpell;
            _overCureEnabled = overCureEnabled;
            _underCureEnabled = underCureEnabled;
            _overCurePriority = overCurePriority;
        }

        public string Tier(string spellName, bool priorityPlayer = false)
        {
            if (_canCastSpell(spellName))
            {
                return spellName;
            }

            var tier = _teirs[spellName];

            if (tier.OverCure != null)
            {
                if (_overCureEnabled)
                {
                    return tier.OverCure;
                }

                if (_overCurePriority && tier.Priority && priorityPlayer)
                {
                    return tier.OverCure;
                }
            }

            if (_underCureEnabled && tier.UnderCure != null)
            {
                return tier.UnderCure;
            }

            return null;
        }
    }

    internal class Cure
    {
        public Cure(string overCure, string underCure, bool priority = false)
        {
            OverCure = overCure;
            UnderCure = underCure;
            Priority = priority;
        }

        public readonly string OverCure;
        public readonly string UnderCure;
        public readonly bool Priority;
    }
}
