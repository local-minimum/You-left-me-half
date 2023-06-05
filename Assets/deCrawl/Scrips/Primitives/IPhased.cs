using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeCrawl.Primitives
{
    public delegate void PhaseChangeEvent(string id, string phase);

    public interface IPhased : IIdentifiable
    {        
        public void RestorePhase(string phase);
        public event PhaseChangeEvent OnPhaseChange;
    }
}
