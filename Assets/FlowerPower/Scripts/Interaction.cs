using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeCrawl.Primitives;

namespace FP
{
    public class Interaction : FindingSingleton<Interaction>
    {
        public enum Phase { Prologue, Fight, Epilogue }
        public enum Speaker { Narrator, Interlocutor, Player };

        [System.Serializable]
        public struct TextPrompt
        {
            public Speaker Speaker;
            public string Text;
        }

        [SerializeField]
        Sprite Interlocutor;

        [SerializeField]
        TextPrompt[] prologue;

        [SerializeField]
        TextPrompt[] epilogue;

        [SerializeField]
        TMPro.TextMeshProUGUI TextArea;

        [SerializeField]
        TMPro.TextMeshProUGUI Elipsis;

        [SerializeField]
        Image InterlocutorAvatar;

        [SerializeField]
        GameObject PlayerAvatar;

        [SerializeField, Range(0, 3)]
        float TextTimeBeforeElipsis = 1f;


        Phase phase;
        int phaseStep;

        private void OnEnable()
        {
            InterlocutorAvatar.sprite = Interlocutor;
            StartPhase(Phase.Prologue);
        }

        void StartPhase(Phase phase)
        {
            this.phase = phase;
            phaseStep = -1;
            EnableAvatar(Speaker.Narrator);
            UpdateText("");

            if (phase == Phase.Fight)
            {
                TextArea.transform.parent.gameObject.SetActive(false);
                GetComponent<IFight>().InitiateFight();
            } else
            {
                GetComponent<IFight>().DisableContent();
                TextArea.transform.parent.gameObject.SetActive(true);
            }
        }

        void EnableAvatar(Speaker speaker)
        {
            switch (speaker)
            {
                case Speaker.Narrator:
                    InterlocutorAvatar.gameObject.SetActive(false);
                    PlayerAvatar.gameObject.SetActive(false);
                    break;
                case Speaker.Player:
                    InterlocutorAvatar.gameObject.SetActive(false);
                    PlayerAvatar.gameObject.SetActive(true);
                    break;
                case Speaker.Interlocutor:
                    InterlocutorAvatar.gameObject.SetActive(true);
                    PlayerAvatar.gameObject.SetActive(false);
                    break;
            }
        }

        public void InitiateEpilogue() => StartPhase(Phase.Epilogue);


        void UpdateText(string text)
        {
            TextArea.text = text;
        }

        bool ProgressPrologue()
        {
            phaseStep++;
            if (phaseStep == prologue.Length) return false;

            var step = prologue[phaseStep];
            StartCoroutine(ProgressStep(step));
            return true;
        }

        [SerializeField]
        KeyCode ContinueTalkKey = KeyCode.Space;

        bool allowNextStep = true;

        bool ContinueTalk => allowNextStep && Input.GetKeyDown(ContinueTalkKey);

        IEnumerator<WaitForSeconds> ProgressStep(TextPrompt step)
        {
            if (!allowNextStep) yield break;

            allowNextStep = false;

            Elipsis.gameObject.SetActive(false);

            EnableAvatar(step.Speaker);
            UpdateText(step.Text);

            yield return new WaitForSeconds(TextTimeBeforeElipsis);

            Elipsis.text = "ooo";
            Elipsis.gameObject.SetActive(true);

            allowNextStep = true;
        }

        private void Update()
        {
            switch (phase)
            {
                case Phase.Prologue:
                    if (phaseStep >= 0)
                    {
                        if (ContinueTalk)
                        {
                            if (!ProgressPrologue())
                            {
                                StartPhase(Phase.Fight);
                            }
                        }
                    }
                    else if (!ProgressPrologue())
                    {
                        StartPhase(Phase.Fight);
                    }
                    break;
            }
        }
    }
}
