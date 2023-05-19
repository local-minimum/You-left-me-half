using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace FP
{
    public class MiniGameSpellWord : MonoBehaviour, IFight
    {
        [SerializeField]
        GameObject PlayingField;

        [SerializeField]
        RectTransform LetterLanesArea;

        [SerializeField]
        string ChallengeWord;

        [SerializeField]
        LetterLane LetterLanePrefab;

        List<LetterLane> lanes = new List<LetterLane>();

        public void DisableContent()
        {
            PlayingField.SetActive(false);
        }

        public void InitiateFight()
        {
            PlayingField.SetActive(true);

            var nLanes = lanes.Count;
            for (int i = 0, l = Mathf.Max(nLanes, ChallengeWord.Length); i < l; i++)
            {
                if (i >= nLanes)
                {
                    var lane = Instantiate(LetterLanePrefab, LetterLanesArea);
                    lane.Configure(ChallengeWord[i], ChallengeWord.ToCharArray());
                    lanes.Add(lane);
                }
                else if (i < ChallengeWord.Length)
                {
                    var lane = lanes[i];
                    lane.Configure(ChallengeWord[i], ChallengeWord.ToCharArray());
                    lane.gameObject.SetActive(true);
                }
                else
                {
                    lanes[i].gameObject.SetActive(false);
                }
            }
        }

        IEnumerable<char> PressedLetters => Input.inputString
            .ToUpper()
            .ToCharArray()
            .Where(ch => ch != '\n' || ch != '\r' || ch != '\b');


        bool ApplyOverLanes(System.Func<LetterLane, bool> predicate)
        {
            bool anySuccess = false;
            for (int i = 0; i<ChallengeWord.Length; i++)
            {
                anySuccess = predicate(lanes[i]) || anySuccess;
            }

            return anySuccess;
        }

        private void Update()
        {
            if (!PlayingField.activeSelf) return;

            foreach (var ch in PressedLetters)
            {
                var anyGood = ApplyOverLanes(lane => lane.Handle(ch, true));
                if (!anyGood)
                {
                    if (ApplyOverLanes(lane => lane.Handle(ch, false)))
                    {
                        Debug.Log($"Player pressed {ch} but this was bad");
                    }
                    else {
                        Debug.Log($"Player pressed {ch} but was not present");
                    }
                }
            }
        }
    }
}
