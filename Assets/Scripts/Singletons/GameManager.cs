using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Simulation))]
public class GameManager : MonoBehaviour {

    #region Singleton
    public static GameManager Instance {
        get; private set;
    }

    private void InitSingleton() {
        if (GameManager.Instance != null)
        {
            throw new UnityException("Hay más de una instancia de GameManager.");
        }
        else
        {
            GameManager.Instance = this;
        }
    }
    #endregion

    #region Global attributes
    public Color unclaimedColor = Color.white;
    
    public Canvas canvas;

    public Movement allowedMovement;

    public GameObject armyPrefab;

    public GameObject armyContainer;

    public GameObject architectContainer;
    #endregion

    private Simulation simulation;

    private UIManager uiManager;

    #region Unity
    private void Awake() {
        this.InitSingleton();
        this.simulation = this.GetComponent<Simulation>();
        this.uiManager = GameObject.Find("UI").GetComponent<UIManager>();
        
        WorldState.Init();
    }
    #endregion

    #region Accessors
    public static Simulation Simulation {
        get {
            return GameManager.Instance.simulation;
        }
    }

    public static UIManager UI {
        get {
            return GameManager.Instance.uiManager;
        }
    }
    #endregion
}
