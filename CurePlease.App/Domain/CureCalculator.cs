using System.Collections.Generic;
using System.Linq;

namespace CurePlease.App.Domain
{
    public class CureCalculator
    {
        private readonly List<CureSpell> _spells;
        private readonly CurePriority _curePriority;

        public CureCalculator(List<CureSpell> spells, CurePriority curePriority)
        {
            _spells = spells;
            _curePriority = curePriority;
        }

        public string CureFor(uint lostHealth, bool priorityPlayer)
        {
            var spellName = _spells
                .OrderByDescending(s => s.Amount)
                .Where(s => s.Enabled)
                .FirstOrDefault(s => s.Amount <= lostHealth)
                ?.Name;

            if (spellName is null) return null;

            return _curePriority.Tier(spellName, priorityPlayer);
        }
    }

    public record CureSpell(string Name, bool Enabled, int Amount);
}