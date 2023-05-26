using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FP
{
    public interface IUIMenuView
    {        
        public UIMenuSystem.State State { get; }
        public bool Active { set; }
    }
}
