using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Instrucción de cesión personalizada que sólo permite continuar una corutina cuando el juego no está pausado

public class WaitIfPaused : CustomYieldInstruction {

    public override bool keepWaiting {
        get {
            return GameManager.Simulation.Paused;
        }
    }

}
