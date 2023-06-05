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
        string Enemy;

        [SerializeField]
        LetterLane LetterLanePrefab;

        [SerializeField]
        TMPro.TextMeshProUGUI EnemyName;

        Inventory PlayerInventory => PlayerController.instance.GetComponentInChildren<Inventory>();

        private void OnEnable()
        {
            LetterLane.OnMissedLetter += LetterLane_OnMissedLetter;
            CurrencyTracker.OnChange += CurrencyTracker_OnChange;
        }

        private void OnDisable()
        {
            LetterLane.OnMissedLetter -= LetterLane_OnMissedLetter;
            CurrencyTracker.OnChange -= CurrencyTracker_OnChange;
        }

        public bool Reward { get; private set; }
        public char RewardLetter { get; private set; }
        private void CurrencyTracker_OnChange(CurrencyType type, int available, int capacity)
        {
            if (available == 0 && (type == CurrencyType.Health || type == CurrencyType.BossHealth))
            {
                foreach (var lane in lanes)
                {
                    lane.Stop = true;
                }

                if (type == CurrencyType.BossHealth)
                {
                    var options = ChallengeWord
                        .Where(ch => !ConfusionLetters.Contains(ch))
                        .ToArray();

                    if (options.Length == 0)
                    {
                        Debug.LogWarning($"All letters from '{ChallengeWord}' already known");
                        RewardLetter = LetterLoot.NoLetterChar;
                        Reward = false;
                    } else
                    {
                        RewardLetter = options[Random.Range(0, options.Length)];
                        Debug.Log($"Planning reward '{RewardLetter}'");
                        Reward = true;
                    }
                } else
                {
                    Debug.Log("Player died");
                }
            }
        }

        private void LetterLane_OnMissedLetter(char letter)
        {
            CurrencyTracker.SubtractAvailable(CurrencyType.Health, omissionHealthCost);
        }

        List<LetterLane> lanes = new List<LetterLane>();

        public void DisableContent()
        {           
            PlayingField.SetActive(false);
        }

        private char[] ConfusionLetters { get; set; }

        public void Configure(string enemy, string challengeWord, int bossHealth)
        {
            ChallengeWord = challengeWord;
            Enemy = enemy;
            CurrencyTracker.Update(CurrencyType.BossHealth, bossHealth, bossHealth);
            CurrencyTracker.ReEmit(CurrencyType.Health);
            ConfusionLetters = PlayerInventory
                .Where(loot => loot is LetterLoot)
                .Select(loot => ((LetterLoot)loot).Letter)
                .Where(letter => letter != LetterLoot.NoLetterChar && !challengeWord.Contains(letter))
                .ToArray();

            Reward = false;
        }

        public void InitiateFight()
        {            
            PlayingField.SetActive(true);

            EnemyName.text = Enemy;

            var nLanes = lanes.Count;
            for (int i = 0, l = Mathf.Max(nLanes, ChallengeWord.Length); i < l; i++)
            {
                var letter = ChallengeWord[i];
                var otherChallangeLetters = ChallengeWord.Where(ch => ch != ChallengeWord[i]).ToArray();

                if (i >= nLanes)
                {
                    var lane = Instantiate(LetterLanePrefab, LetterLanesArea);
                    lane.Configure(letter, otherChallangeLetters, ConfusionLetters);
                    lanes.Add(lane);
                }
                else if (i < ChallengeWord.Length)
                {
                    var lane = lanes[i];
                    lane.Configure(letter, otherChallangeLetters, ConfusionLetters);
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
            .Where(ch => (short)ch >= 64 && ch != ' ' && ch != '\n' && ch != '\r' && ch != '\b');


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
        int omissionHealthCost = 20;

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
                    // Debug.Log($"Good letter {ch} {nGood}");
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
                    // Debug.Log($"Bad letter {ch} {nBad}");
                    if (nBad > 0)
                    {
                        CurrencyTracker.SubtractAvailable(CurrencyType.Health, badHealth * nBad);
                    }
                    else {
                        // Pressing something else not involved in the game
                        CurrencyTracker.SubtractAvailable(CurrencyType.Health, omissionHealthCost);
                    }
                }
            }
        }

        public void RewardPlayer()
        {
            if (!Reward)
            {
                Debug.Log("There was no reward planned");
                return;
            }

            if (RewardLetter == LetterLoot.NoLetterChar)
            {
                Debug.LogWarning($"There's no reward from {name} because all options already looted");
                return;
            }

            var loot = LootTable.instance.CreateLootById(LetterLoot.AsId(RewardLetter));
            loot.Loot(DeCrawl.Primitives.LootOwner.Player);
        }
    }
}
