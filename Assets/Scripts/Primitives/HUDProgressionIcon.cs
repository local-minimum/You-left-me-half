using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDProgressionIcon : MonoBehaviour
{
    protected Image progressImage;

    private void AlignChildRT(Transform t)
    {
        t.SetParent(transform);
        var rt = t.gameObject.GetComponent<RectTransform>();
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
    }

    private void AssignTexture(GameObject go, Texture2D tex, bool isProgress, Image.FillMethod fillMethod)
    {
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
        sprite.name = isProgress ? "Progress" : "FG";
        var image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        if (isProgress)
        {
            image.fillMethod = fillMethod;
            image.type = Image.Type.Filled;
            progressImage = image;
        }
    }

    protected void Configure(Texture2D progressTex, Texture2D overlayTex, Image.FillMethod fillMethod, float initialFill = 0)
    {
        var goProgress = new GameObject("Progress");
        AssignTexture(goProgress, progressTex, true, fillMethod);
        AlignChildRT(goProgress.transform);

        var goFG = new GameObject("FG");
        AssignTexture(goFG, overlayTex, false, fillMethod);
        AlignChildRT(goFG.transform);

        progressImage.fillAmount = initialFill;
    }
}
