using System.Collections;
using System.Collections.Generic;
using DeCrawl.Primitives;

namespace DeCrawl.Systems
{
    /// <summary>
    /// Tentative system to track properties and have a single instance to save data from
    /// </summary>
    public class PropertyRecorder : FindingSingleton<PropertyRecorder>
    {
        public enum RecrodableProperty
        {
            PlayerLevel, PlayerLevelTokens,
        }

        public static int GetInt(RecrodableProperty prop, int defaultValue = 0)
        {
            if (instance.intValues.ContainsKey(prop))
            {
                return instance.intValues[prop];
            }

            return defaultValue;
        }

        public static void SetInt(RecrodableProperty prop, int value)
        {
            instance.intValues[prop] = value;
        }

        private Dictionary<RecrodableProperty, int> intValues = new Dictionary<RecrodableProperty, int>();
    }
}
