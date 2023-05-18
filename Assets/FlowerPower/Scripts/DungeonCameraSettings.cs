using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.Utils;

namespace FP
{
    public class DungeonCameraSettings : FindingSingleton<DungeonCameraSettings>
    {
        public enum CamSetting {
            [StringValue("FOV")]
            FOV,
            [StringValue("Offset")]
            Offset
        };

        [SerializeField]
        float defaultFOV = 70;

        [SerializeField]
        float defaultOffset = -1.3f;

        float GetDefault(CamSetting setting)
        {
            switch (setting)
            {
                case CamSetting.FOV:
                    return defaultFOV;
                case CamSetting.Offset:
                    return defaultOffset;
                default:
                    return 0;
            }            
        }

        Camera _cam;
        Camera Cam
        {
            get
            {
                if (_cam == null)
                {
                    _cam = GetComponent<Camera>();
                }
                return _cam;
            }
        }

        static readonly string preferenceRoot = "DungeonCam";

        static string StorageLocation(CamSetting camSetting) => $"{preferenceRoot}.{camSetting.GetStringAttribute()}";

        public static void Set(CamSetting setting, float value)
        {
            PlayerPrefs.SetFloat(StorageLocation(setting), value);
            switch (setting)
            {
                case CamSetting.FOV:
                    instance.Cam.fieldOfView = value;
                    break;
                case CamSetting.Offset:
                    var offset = instance.transform.localPosition;
                    offset.z = value;
                    instance.transform.localPosition = offset;
                    break;
                default:
                    Debug.LogWarning($"Support of {setting} not implemented");
                    break;
            }
        }

        public static float Get(CamSetting setting) => PlayerPrefs.GetFloat(StorageLocation(setting), instance.GetDefault(setting));

        private void Start()
        {
            Set(CamSetting.FOV, Get(CamSetting.FOV));
            Set(CamSetting.Offset, Get(CamSetting.Offset));
        }
    }
}
