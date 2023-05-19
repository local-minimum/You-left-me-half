using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FP
{
    public class TMPBlurPulse : MonoBehaviour
    {
        [SerializeField]
        TMPro.TextMeshProUGUI ui;

        [SerializeField]
        AnimationCurve pulse;

        [SerializeField, Range(0, 5)]
        float scaleMagnitude = 1f;
        
        private void Start()
        {
            pulse.postWrapMode = WrapMode.Loop;

        }

        void Update()
        {
            ui.fontSharedMaterial.SetFloat(TMPro.ShaderUtilities.ID_OutlineSoftness, scaleMagnitude * pulse.Evaluate(Time.timeSinceLevelLoad));
        }
    }
}