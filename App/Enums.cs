﻿namespace CurePlease.App
{

    public enum LoginStatus
    {
        CharacterLoginScreen = 0,
        Loading = 1,
        LoggedIn = 2
    }

    public enum Status : byte
    {
        Standing = 0,
        Fighting = 1,
        Dead1 = 2,
        Dead2 = 3,
        Event = 4,
        Chocobo = 5,
        Healing = 33,
        Synthing = 44,
        Sitting = 47,
        Fishing = 56,
        FishBite = 57,
        Obtained = 58,
        RodBreak = 59,
        LineBreak = 60,
        CatchMonster = 61,
        LostCatch = 62,
        Unknown
    }

    public enum Job
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

    public enum Buff
    {
        Silence = 6,
        Amnesia = 16,
        Mute = 29,
        Impairment = 261,
        Omerta = 262,
    }

    public enum Spell
    {
        Reraise4 = 848,
        Distract3 = 882,
        Frazzle3 = 883,
        FullCure = 893,
        Refresh3 = 894,
        Temper2 = 895,
    }
}