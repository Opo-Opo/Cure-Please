using System.Linq;
using CurePlease.Xi;
using EliteMMO.API;

namespace CurePlease.Domain
{
    public class CanCastSpell
    {
        private static readonly int[] SilencedStatuses = {
            (int) StatusEffect.Silence, 
            (int) StatusEffect.Mute, 
            (int) StatusEffect.No_Magic_Casting,
        };

        private readonly EliteAPI _pl;

        public CanCastSpell(EliteAPI pl)
        {
            _pl = pl;
        }

        public bool CanCast(EliteAPI.ISpell spell)
        {
            if (Silenced())
            {
                return false;
            }

            if (spell.ID == (int)SpellId.HonorMarch)
            {
                return true;
            }

            if (_pl.Player.MP < spell.MPCost)
            {
                return false;
            }

            if (!MinimumLevel(spell))
            {
                return false;
            }

            return _pl.Player.HasSpell(spell.Index);
        }

        private bool Silenced()
        {
            return _pl.Player.GetPlayerInfo().Buffs
                .Any(b => SilencedStatuses.Contains(b));
        }

        private bool MinimumLevel(EliteAPI.ISpell spell)
        {

            var mainJob = _pl.Player.MainJob;
            var mainJobLevelRequired = spell.LevelRequired[mainJob];
            var subJobLevelRequired = spell.LevelRequired[_pl.Player.SubJob];

            if (mainJobLevelRequired == -1 && subJobLevelRequired == -1)
            {// -1 means the job doesn't have access to the spell
                return false;
            }

            return mainJobLevelRequired <= _pl.Player.MainJobLevel 
                   && subJobLevelRequired <= _pl.Player.SubJobLevel;
        }
    }
}
