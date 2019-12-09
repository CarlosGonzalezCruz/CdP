using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Representa la contraparte relativa al juego de un arquitecto

public class Nation : MonoBehaviour {

    public Color armyColor;

    public Color cellColor;

    private HashSet<Army> armies;

    private HashSet<Cell> cells;

    private HashSet<Cell> adjacentUnclaimedCells;

    private Architect architect;

    #region Unity
    private void Awake() {
        this.GuaranteeInitialization();
        this.architect = this.GetComponent<Architect>();
        this.adjacentUnclaimedCells = new HashSet<Cell>();
        this.architect.onDefeatedBy += this.OnArchitectDefeated;
    }
    #endregion

    #region Accessors
    public int Size {
        get {
            return this.cells.Count;
        }
    }

    public HashSet<Army> Armies {
        get {
            this.GuaranteeInitialization();
            return this.armies;
        }
    }

    public HashSet<Cell> Cells {
        get {
            this.GuaranteeInitialization();
            return this.cells;
        }
    }

    public Architect Architect {
        get {
            return this.architect;
        }
    }

    public HashSet<Cell> AdjacentUnclaimedCells {
        get {
            return this.adjacentUnclaimedCells;
        }
    }
    #endregion

    public int GetArmyAmount() {
        int ret = 0;
        foreach(Army army in this.armies) {
            ret += army.Troops;
        }
        return ret;
    }

    public int GetArmyAmount(Suit suit) {
        int ret = 0;
        foreach(Army army in this.armies) {
            if(army.Suit == suit) {
                ret += army.Troops;
            }
        }
        return ret;
    }

    // Actualiza las celdas adyacentes que pueden ser conquistadas
    public void UpdateAdjacentUnclaimedCells() {
        this.adjacentUnclaimedCells.Clear();
        foreach(var cell in this.cells) {
            foreach(var neighbour in cell.GetNeighbours()) {
                if(neighbour.Nation != this) {
                    this.adjacentUnclaimedCells.Add(neighbour);
                }
            }
        }
    }

    // Devuelve las celdas que, en el estado del mundo indicado, serían adyacentes y podrían ser conquistadas
    public HashSet<Cell> FindAdjacentUnclaimedCellsInWorldState(WorldState state) {
        var ret = new HashSet<Cell>();
        var claimedCells = state.NationClaims[this];
        foreach(var cell in claimedCells) {
            foreach(var neighbour in cell.GetNeighbours()) {
                if(neighbour == null) {
                    continue;
                }
                if(state.CellClaims[neighbour] != this) {
                    ret.Add(neighbour);
                }
            }
        }
        return ret;
    }

    // En caso de que el orden de ejecución se los scripts no esté funcionando correctamente, este método garantiza
    // que al menos los sets importantes ya se han creado
    private void GuaranteeInitialization() {
        if(this.armies == null) {
            this.armies = new HashSet<Army>();
        }
        if(this.cells == null) {
            this.cells = new HashSet<Cell>();
        }
    }

    // Asigna la creación de un ejército del suit indicado a la casilla indicada
    private void ScheduleTroopCreation(Cell cell, Suit suit) {
        cell.ScheduleArmy(suit);
    }

    private void OnArchitectDefeated(Architect other, Architect subject) {
        //// GameObject.Destroy(this.gameObject);
    }
}
