using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryRack1 : InventoryRack
{
    private string[] initialCorruption = new string[]
    {
        "10110012",
        "01201020",
        "01000000",
        "03000003",
    };

    protected override string[] InitialCorruption => initialCorruption;
}
