using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace CurePlease.App
{
    public static class EnumExtensionMethods
    {
        public static string GetDescription(this Enum @enum)
        {
            var genericEnumType = @enum.GetType();
            var memberInfo = genericEnumType.GetMember(@enum.ToString());

            if (memberInfo.Length <= 0)
            {
                return @enum.ToString();
            }

            var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Any() ? ((DescriptionAttribute)attributes.ElementAt(0)).Description : @enum.ToString();
        }
    }

    public enum LoginStatus
    {
        CharacterLoginScreen = 0,
        Loading = 1,
        LoggedIn = 2
    }

    public enum Language
    {
        EN = 0,
    }

    public enum Status : byte
    {
        Idle = 0,
        Engaged = 1,
        Dead = 2,
        EngagedDead = 3,
        Event = 4,
        Chocobo = 5,
        DoorOpening = 8,
        DoorClosing = 9,
        ElevatorUp = 10,
        ElevatorDown = 11,
        Resting = 33,
        Locked = 34,
        FishingFighting = 38,
        FishingCaught = 39,
        FishingBrokenRod = 40,
        FishingBrokenLine = 41,
        FishingCaughtMonster = 42,
        FishingLostCatch = 43,
        Crafting = 44,
        Sitting = 47,
        Kneeling = 48,
        Fishing = 50,
        FishingFightingCenter = 51,
        FishingFightingRight = 52,
        FishingFightingLeft = 53,
        FishingRodInWater = 56,
        FishingFishOnHook = 57,
        FishingCaughtFish = 58,
        FishingRodBreak = 59,
        FishingLineBreak = 60,
        FishingMonsterCatch = 61,
        FishingNoCatchOrLost = 62,
        Sitchair0 = 63,
        Sitchair1 = 64,
        Sitchair2 = 65,
        Sitchair3 = 66,
        Sitchair4 = 67,
        Sitchair5 = 68,
        Sitchair6 = 69,
        Sitchair7 = 70,
        Sitchair8 = 71,
        Sitchair9 = 72,
        Sitchair10 = 73,
        Sitchair11 = 74,
        Sitchair12 = 75,
        Mount = 85,
    }

    public enum Job : byte
    {// Only know WHM, RDM and SCH are correct
        WAR = 1,
        MNK = 2,
        WHM = 3,
        BLM = 4,
        RDM = 5,
        THF = 6,
        PLD = 7,
        DRK = 8,
        BST = 9,
        BRD = 10,
        RNG = 11,
        SAM = 12,
        NIN = 13,
        DRG = 14,
        SMN = 15,
        BLU = 16,
        COR = 17,
        PUP = 18,
        DNC = 19,
        SCH = 20,
        GEO = 21,
        RUN = 22,
    }

    public enum SilenceRemovalItem
    {
        [Description("Catholicon")]
        Catholicon,
        [Description("Echo Drops")]
        EchoDrops,
        [Description("Remedy")]
        Remedy,
        [Description("Remedy Ointment")]
        RemedyOintment,
        [Description("Vicar's Drink")]
        VicarsDrink,
    }

    public enum WakeSleepSpell
    {
        Cure,
        Cura,
        Curaga,
    }
}
