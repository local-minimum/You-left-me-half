using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanisterHUD : MonoBehaviour
{
    public Canister canister;
    Image progressImage;
    
    void AlignChildRT(Transform t)
    {
        t.SetParent(transform);
        var rt = t.gameObject.GetComponent<RectTransform>();
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
    }

    void AssignTexture(GameObject go, Texture2D tex, bool isProgress, Image.FillMethod fillMethod)
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

    private void Start()
    {
        Configure();    
    }

    void Configure()
    {
        var goProgress = new GameObject("Progress");
        AssignTexture(goProgress, canister.textureProgress, true, canister.fillMethod);
        AlignChildRT(goProgress.transform);

        var goFG = new GameObject("FG");
        AssignTexture(goFG, canister.textureOverlay, false, canister.fillMethod);
        AlignChildRT(goFG.transform);

        progressImage.fillAmount = canister.Fill;
    }

    private void Update()
    {
        progressImage.fillAmount = canister.Fill;
    }
}
