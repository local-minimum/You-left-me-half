using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DeCrawl.Primitives;
using DeCrawl.Systems;

namespace FP
{
    public class Interaction : FindingSingleton<Interaction>
    {
        public enum Phase { Prologue, Fight, Epilogue, Done }
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
        Image AvatarBackground;

        [SerializeField]
        GameObject PlayerAvatar;

        [SerializeField, Range(0, 3)]
        float TextTimeBeforeElipsis = 1f;


        Phase phase;
        int phaseStep;

        public void Configure(Sprite interlocutorSprite, TextPrompt[] prologue, TextPrompt[] epilogue)
        {
            Interlocutor = interlocutorSprite;
            this.prologue = prologue;
            this.epilogue = epilogue;
            InterlocutorAvatar.sprite = Interlocutor;
        }

        private void OnEnable()
        {
            InterlocutorAvatar.sprite = Interlocutor;
            Game.OnChangeStatus += Game_OnChangeStatus;
            CurrencyTracker.OnChange += CurrencyTracker_OnChange;            
        }

        private void OnDisable()
        {
            CurrencyTracker.OnChange -= CurrencyTracker_OnChange;
            Game.OnChangeStatus -= Game_OnChangeStatus;
        }

        private void Game_OnChangeStatus(GameStatus status, GameStatus oldStatus)
        {
            if (status == GameStatus.FightScene)
            {
                StartPhase(Phase.Prologue);
            }
        }

        bool playerAlive = true;

        private void CurrencyTracker_OnChange(CurrencyType type, int available, int capacity)
        {
            if (phase != Phase.Fight && (type != CurrencyType.BossHealth || type != CurrencyType.Health)) return;


            if (available == 0)
            {
                if (type == CurrencyType.Health)
                {
                    playerAlive = false;
                    StartCoroutine(DelayedPhaseChange(Phase.Done));
                } else
                {
                    StartCoroutine(DelayedPhaseChange(Phase.Epilogue));
                }
            }
        }

        [SerializeField, Range(0, 5)]
        float fightEndTime = 1;

        IEnumerator<WaitForSeconds> DelayedPhaseChange(Phase newPhase)
        {
            yield return new WaitForSeconds(fightEndTime);
            StartPhase(newPhase);
        }

        void StartPhase(Phase phase)
        {
            this.phase = phase;

            phaseStep = -1;
            EnableAvatar(Speaker.Narrator);
            UpdateText("");

            if (phase == Phase.Done)
            {
                TextArea.transform.parent.gameObject.SetActive(false);
                var fight = GetComponent<IFight>();
                fight.RewardPlayer();
                fight.DisableContent();
                Game.Status = playerAlive ? GameStatus.Playing : GameStatus.GameOver;
                return;
            }
            else if (phase == Phase.Fight)
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
                    AvatarBackground.gameObject.SetActive(false);
                    InterlocutorAvatar.gameObject.SetActive(false);
                    PlayerAvatar.gameObject.SetActive(false);
                    break;
                case Speaker.Player:
                    AvatarBackground.gameObject.SetActive(true);
                    InterlocutorAvatar.gameObject.SetActive(false);
                    PlayerAvatar.gameObject.SetActive(true);
                    break;
                case Speaker.Interlocutor:
                    AvatarBackground.gameObject.SetActive(true);
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
        bool ProgressEpilogue()
        {
            phaseStep++;
            if (phaseStep == epilogue.Length) return false;

            var step = epilogue[phaseStep];
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
                case Phase.Epilogue:
                    if (phaseStep >= 0)
                    {
                        if (ContinueTalk)
                        {
                            if (!ProgressEpilogue())
                            {
                                StartPhase(Phase.Done);
                            }
                        }
                    } else if (!ProgressEpilogue())
                    {
                        StartPhase(Phase.Done);
                    }
                    break;

            }
        }
    }
}
