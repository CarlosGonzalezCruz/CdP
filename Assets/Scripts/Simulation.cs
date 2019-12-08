using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour {

    public event System.Action<int> onNextTurn;

    public event System.Action<int> onLateNextTurn;

    public event System.Action<float, float> onSpeedChanged;

    public event System.Action<bool> onPauseChanged;

    public float minSpeed = 1;

    public float maxSpeed = 5;

    public float deltaSpeed = 1;

    private int turn;

    private float speed;

    private bool paused;

    #region Unity
    private void Start() {
        this.Speed = this.GetDefaultSpeed();
        this.Paused = true;
        this.turn = 0;
        this.StartCoroutine(this.AdvanceTurns());
    }

    private void Update() {
        if(Input.GetButtonDown("Speed")) {
            this.Speed += Input.GetAxisRaw("Speed") * this.deltaSpeed;
        }

        if(Input.GetButtonDown("Pause")) {
            this.Paused = !this.Paused;
        }
    }
    #endregion

    #region Accessors
    public float Speed {
        get {
            return this.speed;
        }
        private set {
            var newValue = Mathf.Clamp(value, this.minSpeed, this.maxSpeed);
            if(newValue != this.speed) {
                this.onSpeedChanged.Invoke(newValue, this.speed);
                this.speed = newValue;
            }
        }
    }
    
    public bool Paused {
        get {
            return this.paused;
        }
        private set {
            if(value != this.paused) {
                this.onPauseChanged.Invoke(value);
                this.paused = value;
            }
        }
    }
    #endregion

    private void TriggerNextTurn() {
        this.onNextTurn.Invoke(this.turn);
        this.onLateNextTurn.Invoke(this.turn);
        this.turn += 1;
    }

    private float GetDefaultSpeed() {
        var rangeWithoutDelta = (this.maxSpeed - this.minSpeed) / this.deltaSpeed;
        var halfRange = Mathf.Round(rangeWithoutDelta * 0.5f);
        var halfRangeWithDelta = halfRange * this.deltaSpeed;
        return halfRangeWithDelta + this.minSpeed;

    }

    private IEnumerator AdvanceTurns() {
        while(true) {

            yield return new WaitForSeconds(1f / speed);
            yield return new WaitIfPaused();

            this.TriggerNextTurn();
        }
    }
}
