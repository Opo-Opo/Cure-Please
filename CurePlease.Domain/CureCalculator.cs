using System.Collections.Generic;
using System.Linq;

namespace CurePlease.Domain
{
    public class CureCalculator
    {
        private readonly List<CureSpell> _cureTiers;

        public CureCalculator(List<CureSpell> cureTiers)
        {
            _cureTiers = cureTiers;
        }

        public string CureFor(uint lostHealth)
        {
            return _cureTiers
                .OrderByDescending(s => s.Amount)
                .Where(s => s.Enabled)
                .FirstOrDefault(s => s.Amount <= lostHealth)
                ?.Name;
        }
    }

    public class CureSpell
    {
        public readonly string Name;
        public readonly bool Enabled;
        public readonly int Amount;

        public CureSpell(string name, bool enabled, int amount)
        {
            Name = name;
            Enabled = enabled;
            Amount = amount;
        }
    }
}
