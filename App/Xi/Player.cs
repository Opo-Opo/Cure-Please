using EliteMMO.API;
using System.Linq;
using static EliteMMO.API.EliteAPI;

namespace CurePlease.App.Xi
{
    public class Player
    {
        private readonly EliteAPI eliteApi;

        public Player(EliteAPI eliteApi)
        {
            this.eliteApi = eliteApi;
        }

        public bool HasBuff(Buff buff)
        {
            return eliteApi.Player.GetPlayerInfo().Buffs.Any(b => b == (int)buff);
        }

        public bool HasBuff(params Buff[] buffs)
        {
            return buffs.Any(HasBuff);
        }

        public bool CanUseAbility(string name)
        {
            if (HasBuff(Buff.Amnesia, Buff.Impairment))
            {
                return false;
            }

            return eliteApi.Player.HasAbility(eliteApi.Resources.GetAbility(name, 0).ID);
        }

        public bool CanUseSpell(string name)
        {
            if (HasBuff(Buff.Mute, Buff.Omerta, Buff.Silence))
            {
                return false;
            }

            if (name.Trim().ToLower() == "honor march")
            {
                return true;
            }

            var spell = eliteApi.Resources.GetSpell(name, 0);

            return eliteApi.Player.HasSpell(spell.Index) && JobCanUseSpell(spell);
        }

        private bool JobCanUseSpell(ISpell spell)
        {
            int mainjobLevelRequired = spell.LevelRequired[(eliteApi.Player.MainJob)];
            int subjobLevelRequired = spell.LevelRequired[(eliteApi.Player.SubJob)];

            if (mainjobLevelRequired == -1 && subjobLevelRequired == -1)
            {// -1 means the job can't use the spell
                return false;
            }

            if (mainjobLevelRequired <= eliteApi.Player.MainJobLevel)
            {
                return true;
            }
            
            if (subjobLevelRequired <= eliteApi.Player.SubJobLevel)
            {
                return true;
            }

            if (mainjobLevelRequired <= 99)
            {// Not a job point spell
                return false;
            }

            if (eliteApi.Player.MainJobLevel != 99)
            {// Main job not lv99 for job point spells
                return false;
            }

            PlayerJobPoints jobPoints = eliteApi.Player.GetJobPoints(eliteApi.Player.MainJob);

            if (spell.ID == (int)Spell.Reraise4)
            {
                return eliteApi.Player.MainJob == (int)Job.WHM && jobPoints.SpentJobPoints >= 100;
            }

            if (spell.ID == (int)Spell.FullCure)
            {
                return eliteApi.Player.MainJob == (int)Job.WHM && jobPoints.SpentJobPoints >= 1200;
            }

            if (spell.ID == (int) Spell.Refresh3 || spell.ID == (int) Spell.Temper2)
            {
                return eliteApi.Player.MainJob == (int)Job.RDM && jobPoints.SpentJobPoints >= 1200;
            }
            
            if (spell.ID == (int) Spell.Distract3 || spell.ID == (int) Spell.Frazzle3)
            {
                return eliteApi.Player.MainJob == (int)Job.RDM && jobPoints.SpentJobPoints >= 550;
            }
            
            if (spell.Name[0].ToLower().Contains("storm ii"))
            {
                return eliteApi.Player.MainJob == (int)Job.SCH && jobPoints.SpentJobPoints >= 100;
            }

            return false;
        }
    }
}
