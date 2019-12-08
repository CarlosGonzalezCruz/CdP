using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitIfPaused : CustomYieldInstruction {

    public override bool keepWaiting {
        get {
            return GameManager.Simulation.Paused;
        }
    }

}
