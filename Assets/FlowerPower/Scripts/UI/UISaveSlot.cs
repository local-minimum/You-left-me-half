using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeCrawl.Systems;
using DeCrawl.UI;


namespace FP
{
    public class UISaveSlot : MonoBehaviour
    {
        [SerializeField]
        TMPro.TextMeshProUGUI Title;

        [SerializeField]
        TMPro.TextMeshProUGUI Info;

        [SerializeField]
        Image ScreenShot;

        [SerializeField]
        protected string SaveSlot = "1";

        [SerializeField]
        Sprite NoSaveImage;

        [SerializeField]
        bool RoundPlayTimeToSeconds = true;

        UIButton Button;

        private void OnEnable()
        {
            if (Button == null) Button = GetComponent<UIButton>();
            SyncWithSave();
            Button.OnClick += Button_OnClick;
        }

        public void OnDisable()
        {
            Button.OnClick -= Button_OnClick;
        }

        virtual protected void Button_OnClick(UIButton button)
        {
            GameSaver.instance.Save(SaveSlot);
            SyncWithSave();
        }

        string DurationText(double duration)
        {
            var timespan = System.TimeSpan.FromSeconds(RoundPlayTimeToSeconds ? System.Math.Floor(duration) : duration);
            return timespan.ToString(@"hh\:mm");
        }


        protected void SyncWithSave()
        {
            if (GameSaver.instance == null)
            {
                Title.text = "-UNKNOWN-";
                Info.text = "Could not access saved games";
                ScreenShot.sprite = NoSaveImage;
                return;
            }

            if (!GameSaver.instance.HasSave(SaveSlot))
            {
                Title.text = "EMPTY SAVE";
                Info.text = "";
                ScreenShot.sprite = NoSaveImage;
                return;
            }

            if (GameSaver.instance.PeakMetadata(SaveSlot, out MetadataRecorder.GameMetadata metadata))
            {
                if (!string.IsNullOrEmpty(metadata.Title))
                {
                    Title.text = metadata.Title;
                } else if (!string.IsNullOrEmpty(metadata.Quest))
                {
                    Title.text = metadata.Quest;
                } else if (!string.IsNullOrEmpty(metadata.Region))
                {
                    Title.text = metadata.Region;
                }

                Info.text = $"{DurationText(metadata.PlayTimeSeconds)}/{metadata.Time.LocalDateTime.ToShortDateString()} {metadata.Time.LocalDateTime.ToShortTimeString()}";
                Info.text += $"\n{metadata.AuxInfo}";

                if (metadata.Screenshot != null)
                {
                    var sprite = Sprite.Create(metadata.Screenshot, new Rect(0, 0, metadata.Screenshot.width, metadata.Screenshot.height), Vector2.one * 0.5f);
                    sprite.name = "Screenshot";
                    ScreenShot.sprite = sprite;
                } else
                {
                    ScreenShot.sprite = NoSaveImage;
                }
                return;
            }
                        
            Title.text = "-UNKNOWN-";
            Info.text = "Could not parse save metadata";
            ScreenShot.sprite = NoSaveImage;            
        }
    }
}
