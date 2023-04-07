using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Level1 : Level
{
    [HideInInspector]
    protected string[] startGrid = new string[] {
        "--xxxxx-xxxxxx-xxxx-",
        "xxxxxxx-x-xxxx-xxxxx",
        "xxxxxxx-x-xxxx-xxx-x",
        "---xxxx-xxxxxx-xxxxx",
        "xxxxxxx-S-------xxxx",
        "---v----x-----v----x",
        "xxxxxxxvxxxxxxxxxxxx",
        "xxxxxx-----x--------",
        "xxxxxx--xxxxxxxxxxx-",
        "xxxxxx--xxxxxxxxxxx-"
    };

    protected override string[] grid { get => startGrid; }

    public override int lvl => 1;
}