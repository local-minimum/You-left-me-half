using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DeCrawl.World;
using DeCrawl.Systems;

namespace FP
{
    public class ScreenGrabber : MonoBehaviour
    {
        [SerializeField]
        bool debugSave = false;

        [SerializeField]
        int targetWidth = 64;

        [SerializeField]
        int targetHeight = 64;

        private void OnEnable()
        {
            GetComponent<IMovingEntity>().OnMove += ScreenGrabber_OnMove;
        }

        private void OnDisable()
        {
            GetComponent<IMovingEntity>().OnMove -= ScreenGrabber_OnMove;
        }

        private void ScreenGrabber_OnMove(string id, Vector3Int position, DeCrawl.Primitives.CardinalDirection lookDirection)
        {
            StartCoroutine(Grab());
        }

        IEnumerator<WaitForEndOfFrame> Grab()
        {
            yield return new WaitForEndOfFrame();

            // Create a texture the size of the screen, RGB24 format
            int width = Screen.width;
            int height = Screen.height;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            // Read screen contents into the texture
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            RenderTexture rt = new RenderTexture(targetWidth, targetWidth, 24);

            Graphics.Blit(tex, rt);

            Texture2D result = new Texture2D(targetWidth, targetHeight);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();

            // Encode texture into PNG
            byte[] bytes = ImageConversion.EncodeToPNG(result);

            Object.Destroy(tex);
            Object.Destroy(rt);

            if (debugSave)
            {
                File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);
            }

            MetadataRecorder.instance.Screenshot = result;            
        }
    }
}
