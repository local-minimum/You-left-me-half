using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TMProEffect : MonoBehaviour
{

    [SerializeField] float refreshRate = 0.1f;

    [SerializeField] TMPro.TextMeshProUGUI textGUI;

    private void Start()
    {
        if (textGUI == null)
        {
            textGUI = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        }

        StartCoroutine(Animate());
    }

    [SerializeField]
    Color32 pulseColorA = Color.black;
    [SerializeField]
    Color32 pulseColorB = Color.white;
    [SerializeField, Tooltip("Value 0 means color A, 1 means color B. Allows for extrapolation")]
    AnimationCurve pulseAnimation;
    [SerializeField, Range(0, 100)]
    float pulseCharacterLength = 10;

    private void ApplyColor(
        TMPro.TMP_CharacterInfo characterInfo, 
        TMPro.TMP_MeshInfo[] meshInfo,
        Color32 color
    )
    {
        int meshIndex = characterInfo.materialReferenceIndex;
        int vertexIndex = characterInfo.vertexIndex;
        Color32[] vertexColors = meshInfo[meshIndex].colors32;
        for (int i = 0; i<4; i++)
        {
            vertexColors[vertexIndex + i] = color;
        }
    }

    private void PulseCharacterColor(TMPro.TMP_CharacterInfo characterInfo, TMPro.TMP_MeshInfo[] meshInfo)
    {
        // TODO: We shouldn't calculate this every character, or cycle
        var animationDuration = pulseAnimation.keys[pulseAnimation.length - 1].time;

        // END TODO

        var t = (
            // How long a pulse takes
            animationDuration * 
            // How far into the pulse the character is placed
            (characterInfo.index % pulseCharacterLength) / pulseCharacterLength             
            + Time.timeSinceLevelLoad
        ) % animationDuration;

        var color = Color.LerpUnclamped(pulseColorA, pulseColorB, pulseAnimation.Evaluate(t));

        ApplyColor(characterInfo, meshInfo, color);
    }

    private void ApplyMeshManipulation(
        TMPro.TMP_CharacterInfo characterInfo, 
        Vector3[] vertices,
        System.Func<Vector3, int, Vector3> predicate
    )
    {
        int vertexIndex = characterInfo.vertexIndex;

        for (int i = 0; i < 4; i++)
        {
            vertices[vertexIndex + i] = predicate(vertices[vertexIndex + i], i);
        }

    }

    [SerializeField]
    float wobbleMagnitude = 0.1f;
    
    private Vector3 RandomOffset(float magnitude) => new Vector3(
            Random.Range(-wobbleMagnitude, wobbleMagnitude),
            Random.Range(-wobbleMagnitude, wobbleMagnitude)
        );

    private void Wobble(TMPro.TMP_CharacterInfo characterInfo, Vector3[] vertices)
    {
        var offset = RandomOffset(wobbleMagnitude);

        ApplyMeshManipulation(characterInfo, vertices, (pos, _) => pos + offset);
    }

    private void DistortWobble(TMPro.TMP_CharacterInfo characterInfo, Vector3[] vertices)
    {
        ApplyMeshManipulation(characterInfo, vertices, (pos, _) => pos + RandomOffset(wobbleMagnitude));
    }

    IEnumerator<WaitForSeconds> Animate()
    {
        while (true)
        {
            var mesh = textGUI.mesh;
            var vertices = mesh.vertices;
            bool meshManipulation = true;

            if (textGUI.enabled)
            {
                for (int i=0; i<textGUI.textInfo.characterCount; i++)
                {
                    var character = textGUI.textInfo.characterInfo[i];
                    if (!character.isVisible) continue;

                    DistortWobble(character, vertices);
                    // Wobble(character, vertices);
                    // PulseCharacterColor(character, textGUI.textInfo.meshInfo);
                }

                if (meshManipulation)
                {
                    mesh.SetVertices(vertices);
                    textGUI.UpdateGeometry(mesh, 0);
                }
                // textGUI.UpdateVertexData(TMPro.TMP_VertexDataUpdateFlags.Colors32);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
