using System.Collections;
using System.Collections.Generic;
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

        private void Update()
        {
            
        }
    }
}
