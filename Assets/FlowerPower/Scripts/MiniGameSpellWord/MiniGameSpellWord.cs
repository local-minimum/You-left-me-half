using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DeCrawl.Systems;

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

        [SerializeField]
        TMPro.TextMeshProUGUI EnemyName;        

        List<LetterLane> lanes = new List<LetterLane>();

        public void DisableContent()
        {
            CurrencyTracker.Update(CurrencyType.Health, 100, 300);

            PlayingField.SetActive(false);
        }

        public void InitiateFight()
        {
            
            PlayingField.SetActive(true);

            EnemyName.text = ChallengeWord;
            CurrencyTracker.Update(CurrencyType.BossHealth, 100, 100);

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
            .Where(ch => ch != ' ' || ch != '\n' || ch != '\r' || ch != '\b');


        int ApplyOverLanes(System.Func<LetterLane, bool> predicate)
        {
            int count = 0;
            for (int i = 0; i<ChallengeWord.Length; i++)
            {
                if (predicate(lanes[i]))
                {
                    count++;
                }
            }

            return count;
        }

        [SerializeField]
        int omissiongHealthCost = 20;

        [SerializeField]
        int badHealth = 5;

        [SerializeField]
        int goodComboHealthBonus = 10;

        [SerializeField]
        int hitDamage = 5;

        [SerializeField]
        int comboHitDamage = 10;

        private void Update()
        {
            if (!PlayingField.activeSelf) return;

            foreach (var ch in PressedLetters)
            {
                var nGood = ApplyOverLanes(lane => lane.Handle(ch, true));
                if (nGood > 0)
                {
                    if (nGood > 1)
                    {
                        CurrencyTracker.AddAvailable(CurrencyType.Health, goodComboHealthBonus * nGood);
                        CurrencyTracker.SubtractAvailable(CurrencyType.BossHealth, comboHitDamage + hitDamage * nGood);
                    } else
                    {
                        CurrencyTracker.SubtractAvailable(CurrencyType.BossHealth, hitDamage);
                    }
                }
                else
                {
                    var nBad = ApplyOverLanes(lane => lane.Handle(ch, false));
                    if (nBad > 0)
                    {
                        CurrencyTracker.SubtractAvailable(CurrencyType.Health, badHealth * nBad);
                    }
                    else {
                        Debug.Log(ch);
                        CurrencyTracker.SubtractAvailable(CurrencyType.Health, omissiongHealthCost);
                    }
                }
            }
        }
    }
}
