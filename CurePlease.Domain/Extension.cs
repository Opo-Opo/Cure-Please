using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliteMMO.API;

namespace CurePlease.Domain
{
    public static class Extension
    {
        public static object GetPropertyValue(this object obj, string propertyName, object @default = null)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj) ?? @default;
        }

        public static T GetPropertyValue<T>(this object obj, string propertyName, object @default = null)
        {
            return (T) GetPropertyValue(obj, propertyName, @default);
        }

        public static bool In(this object obj, params object[] options)
        {
            return options.Any(option => option == obj);
        }

        public static bool IsSlept(this EliteAPI.PlayerTools player)
        {
            return player.Buffs.Any(
                status => status.In((short) StatusEffect.Sleep, (short) StatusEffect.Sleep2)
            );
        }
    }
}
