using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nation : MonoBehaviour {

    public Color armyColor;

    public Color cellColor;

    private List<Army> armies;

    private List<Cell> cells;

    private Architect architect;

    #region Unity
    private void Awake() {
        this.armies = new List<Army>();
        this.cells = new List<Cell>();
        this.architect = this.GetComponent<Architect>();
    }
    #endregion

    #region Accessors
    public int Size {
        get {
            return this.cells.Count;
        }
    }

    public Architect Architect {
        get {
            return this.architect;
        }
    }
    #endregion

    public int GetArmyAmount() {
        return this.armies.Count;
    }

    public int GetArmyAmount(Suit suit) {
        int ret = 0;
        foreach(Army army in this.armies) {
            if(army.Suit == suit) {
                ret += 1;
            }
        }
        return ret;
    }

    private void ScheduleTroopCreation(Cell cell, Suit suit) {
        cell.ScheduleArmy(suit);
    }
}
