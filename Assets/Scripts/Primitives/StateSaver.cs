using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface StateSaver
{
    public string SerializeState();

    public void DeserializeState(string json);
}
