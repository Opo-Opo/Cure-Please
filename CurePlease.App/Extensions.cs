using EliteMMO.API;

namespace CurePlease.App
{
    public static class Extensions
    {
        public static bool IsInSpellCastingRange(this EliteAPI.XiEntity entity)
        {
            return entity.Distance is < 21 and > 0;
        }
    }
}