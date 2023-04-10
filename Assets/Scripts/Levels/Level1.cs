using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Level1 : Level
{
    [HideInInspector]
    protected string[] startGrid = new string[] {
        "--xxxxx-x-xxxx-xxxx-",
        "xSxxxxx-x-xxxx-xxxxx",
        "xxxxxxx-x-xxxx-xxx-x",
        "---xxxx-xxxxxx-xxxxx",
        "xxxxxxx-x-------xxxx",
        "---v----x-----v----x",
        "xxxxxxxvxxxxxxxxxxxx",
        "xxxxxx-----x--------",
        "xxxxxx--xxxxxxxxxxx-",
        "xxxxxx--xxxxxxxxxxx-"
    };

    protected override string[] charGrid { get => startGrid; }

    public override int lvl => 1;
}