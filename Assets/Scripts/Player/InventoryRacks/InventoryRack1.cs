using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryRack1 : InventoryRack
{
    private string[] initialCorruption = new string[]
    {
        "10110012",
        "01201010",
        "01110000",
        "03020003",
    };

    protected override string[] InitialCorruption => initialCorruption;
}
