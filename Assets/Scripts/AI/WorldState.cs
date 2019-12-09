using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Colección de stats para simular el mundo
public class WorldState  {

    // Valor de cada stat para cada arquitecto en este estado
    private Dictionary<Architect, Dictionary<Stat, int>> architectInfo;
    
    // Posición de cada arma en este estado
    private Dictionary<Army, Cell> armyPosition;
    
    // Casillas bajo el mando de cada nación en este estado
    private Dictionary<Nation, HashSet<Cell>> nationClaims;

    // Representa la misma información que el anterior, pero permite hacer la búsqueda inversa de forma eficiente
    private Dictionary<Cell, Nation> cellClaims;

    // Estado actual
    private static WorldState current;

    // ¿El estado actual está actualizado? En caso negativo, no es el caso actual
    private static bool currentIsUpdated;

    private WorldState() {
        this.architectInfo = new Dictionary<Architect, Dictionary<Stat, int>>();
        this.nationClaims = new Dictionary<Nation, HashSet<Cell>>();
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

    public Dictionary<Architect, Dictionary<Stat, int>> Architects {
        get {
            return this.architectInfo;
        }
    }

    public Dictionary<Army, Cell> ArmiesPosition {
        get {
            return this.armyPosition;
        }
    }

    public Dictionary<Nation, HashSet<Cell>> NationClaims {
        get {
            return this.nationClaims;
        }
    }

    public Dictionary<Cell, Nation> CellClaims {
        get {
            return this.cellClaims;
        }
    }
    #endregion

    // Realiza una copia profunda de este estado del mundo, para poder simular sobre ella las consecuencias de las acciones
    // sin afectar a este estado del mundo
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

        foreach(Nation nation in this.nationClaims.Keys) {
            ret.nationClaims[nation] = new HashSet<Cell>();
            foreach(var cell in this.nationClaims[nation]) {
                ret.nationClaims[nation].Add(cell);
            }
        }

        foreach(Cell cell in this.cellClaims.Keys) {
            ret.cellClaims[cell] = this.cellClaims[cell];
        }

        return ret;
    }

    private static void OnNextTurn(int turn) {
        WorldState.currentIsUpdated = false;
    }

    // Genera un estado del mundo partiendo de los datos actuales de la partida
    private static WorldState CalculateCurrent() {
        
        WorldState ret = new WorldState();

        foreach(Architect architect in GameManager.Instance.architectContainer.GetComponentsInChildren<Architect>()) {
            ret.architectInfo[architect] = architect.GetState();
            ret.nationClaims[architect.Nation] = new HashSet<Cell>();
        }

        foreach(Army army in GameManager.Instance.armyContainer.GetComponentsInChildren<Army>()) {
            ret.armyPosition[army] = army.CurrentCell;
        }

        foreach(Cell cell in BoardManager.Instance.GetComponentsInChildren<Cell>()) {
            if(cell.Nation != null) {
                ret.nationClaims[cell.Nation].Add(cell);
            }
            ret.cellClaims[cell] = cell.Nation;
        }
        
        return ret;
    }
}
