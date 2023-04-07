using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingsHUD : MonoBehaviour
{
    [SerializeField]
    GameObject Panel;

    [SerializeField]
    TMPro.TextMeshProUGUI TextField;

    void Start()
    {
        Panel.SetActive(false);
    }

    private void OnEnable()
    {
        MasterOfEndings.OnEnding += MasterOfEndings_OnEnding;
    }

    private void OnDisable()
    {
        MasterOfEndings.OnEnding -= MasterOfEndings_OnEnding;
    }


    private void MasterOfEndings_OnEnding(EndingType type, Ending ending)
    {
        if (type == EndingType.Death)
        {
            switch (ending)
            {
                case Ending.NoHealth:
                    TextField.text = "Death by violence";
                    break;
                case Ending.NoHealthCanister:
                    TextField.text = "Death by curiosity";
                    break;
                case Ending.LostConnection:
                    TextField.text = "Connection Lost";
                    break;
            }
            Panel.SetActive(true);
        }
    }
}
