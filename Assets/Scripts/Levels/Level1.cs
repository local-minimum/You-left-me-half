using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Level1 : Level
{
    [HideInInspector]
    protected string[] startGrid = new string[] {
        "---xx---xx-------",
        "---xxx--xx----xxx",
        "---x----x----xxxx",
        "---x----x----xxxx",
        "---xxxxxxxxxxxxx-",
        "----------x------",
        "Sxxxxxxxxxx------",
    };

    protected override string[] grid { get => startGrid; }

    public override int lvl => 1;
}