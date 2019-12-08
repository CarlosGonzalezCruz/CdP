using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldState  {

    private Dictionary<Architect, Dictionary<Stat, int>> architectInfo;
    
    private Dictionary<Army, Cell> armyPosition;

    private Dictionary<Cell, Nation> cellClaims;

    private static WorldState current;

    private static bool currentIsUpdated;

    private WorldState() {
        this.architectInfo = new Dictionary<Architect, Dictionary<Stat, int>>();
        this.cellClaims = new Dictionary<Cell, Nation>();
        this.armyPosition = new Dictionary<Army, Cell>();
    }

    public static void Init() {
        GameManager.Simulation.onNextTurn += WorldState.OnNextTurn;
        WorldState.currentIsUpdated = false;
    }

    #region Accessors
    public static WorldState Current {
        get {
            if(!WorldState.currentIsUpdated) {
                WorldState.current = WorldState.CalculateCurrent();
                WorldState.currentIsUpdated = true;
            }
            return WorldState.current;
        }
    }

    public Dictionary<Stat, int> this[Architect architect] {
        get {
            return this.architectInfo[architect];
        }
    }

    public Dictionary<Army, Cell> ArmiesPosition {
        get {
            return this.armyPosition;
        }
    }

    public Dictionary<Cell, Nation> CellClaims {
        get {
            return this.cellClaims;
        }
    }
    #endregion

    public WorldState Clone() {
        WorldState ret = new WorldState();
        foreach(Architect architect in this.architectInfo.Keys) {
            var stats = new Dictionary<Stat, int>();
            foreach(Stat stat in this.architectInfo[architect].Keys) {
                stats[stat] = this.architectInfo[architect][stat];
            }
            ret.architectInfo[architect] = stats;
        }
        foreach(Army army in this.armyPosition.Keys) {
            ret.armyPosition[army] = this.armyPosition[army];
        }
        foreach(Cell cell in this.cellClaims.Keys) {
            ret.cellClaims[cell] = this.cellClaims[cell];
        }
        return ret;
    }

    private static void OnNextTurn(int turn) {
        WorldState.currentIsUpdated = false;
    }

    private static WorldState CalculateCurrent() {
        WorldState ret = new WorldState();
        foreach(Architect architect in GameManager.Instance.architectContainer.GetComponentsInChildren<Architect>()) {
            ret.architectInfo[architect] = architect.GetState();
        }
        foreach(Army army in GameManager.Instance.armyContainer.GetComponentsInChildren<Army>()) {
            ret.armyPosition[army] = army.CurrentCell;
        }
        foreach(Cell cell in BoardManager.Instance.GetComponentsInChildren<Cell>()) {
            ret.cellClaims[cell] = cell.Nation;
        }
        return ret;
    }
}
