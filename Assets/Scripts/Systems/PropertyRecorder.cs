using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RecrodableProperty
{
    PlayerLevel, PlayerLevelTokens,
}

public class PropertyRecorder : MonoBehaviour
{
    public static int GetInt(RecrodableProperty prop, int defaultValue = 0) {
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

    private static PropertyRecorder _instance;
    private static PropertyRecorder instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PropertyRecorder>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        } else if (_instance != this)
        {
            Destroy(gameObject);
        }        
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private Dictionary<RecrodableProperty, int> intValues = new Dictionary<RecrodableProperty, int>();
}
