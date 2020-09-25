using static EliteMMO.API.EliteAPI;

namespace CurePlease.Xi
{
    public class CharacterHealth
    {
        public CharacterHealth(PartyMember member)
        {
            Name = member.Name;
            HP = member.CurrentHP;
            HPPercent = member.CurrentHPP;
            HPMax = (member.CurrentHP * 100) / (member.CurrentHPP);
        }

        public CharacterHealth(PlayerTools player)
        {
            Name = player.Name;
            HP = player.HP;
            HPPercent = player.HPP;
            HPMax = player.HPMax;
        }

        public readonly string Name;
        public readonly uint HP;
        public readonly uint HPPercent;
        public readonly uint HPMax;
    }
}
