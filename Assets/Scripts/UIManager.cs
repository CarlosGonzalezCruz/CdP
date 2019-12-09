using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Componente contenedor para la UI

public class UIManager : MonoBehaviour {
    
    public GameObject speedCounterItemPrefab;

    private Transform alwaysFront;

    private Transform speedCounter;

    private Transform pauseBox;

    #region Unity
    private void Awake() {
        this.alwaysFront = this.transform.Find("AlwaysFront");
        this.speedCounter = this.transform.Find($"{this.alwaysFront.name}/SpeedCounter");
        this.pauseBox = this.transform.Find($"{this.alwaysFront.name}/PauseBox");
        GameManager.Simulation.onSpeedChanged += this.OnSpeedChanged;
        GameManager.Simulation.onPauseChanged += this.OnPauseChanged;
    }

    private void Update() {
        if(this.transform.GetChild(this.transform.childCount - 1) != this.alwaysFront) {
            this.alwaysFront.SetAsLastSibling();
        }
    }
    #endregion

    private void OnSpeedChanged(float newSpeed, float oldSpeed) {

        foreach(Transform child in this.speedCounter) {
            GameObject.Destroy(child.gameObject);
        }

        var counterItems = Mathf.Round((newSpeed - GameManager.Simulation.minSpeed)
                        / GameManager.Simulation.deltaSpeed) + 1;

        for(var i = 0; i < counterItems; i++) {
            var item = GameObject.Instantiate(this.speedCounterItemPrefab);
            item.transform.SetParent(this.speedCounter.transform);
        }
    }

    private void OnPauseChanged(bool paused) {
        this.pauseBox.gameObject.SetActive(paused);
    }

    public void BringHUDToFront() {
        this.alwaysFront.SetAsLastSibling();
    }
}
