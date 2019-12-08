using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actionable : MonoBehaviour {

    [SerializeField]
    private Vector2Int coordinates;

    private Cell cell;

    private static List<Actionable> all;

    private static List<Actionable> moved;

    #region Unity
    protected virtual void Awake() {
        if(Actionable.moved == null) {
            Actionable.moved = new List<Actionable>();
        }
        if(Actionable.all == null) {
            Actionable.all = new List<Actionable>();
        } 
        Actionable.all.Add(this);
        GameManager.Simulation.onNextTurn += this.OnNextTurn;
        GameManager.Simulation.onLateNextTurn += this.OnLateNextTurn;
    }

    protected virtual void Start() {
        // Nada
    }

    protected virtual void Update() {
        // Nada
    }
    #endregion

    #region Accessors
    public virtual Vector2Int Coordinates {
        get {
            return this.coordinates;
        }
        set {
            if(value == this.coordinates) {
                Actionable.moved.Add(this);
            }
            this.coordinates = value;
            this.cell = BoardManager.Instance.GetCell(value);
        }
    }

    public virtual Cell CurrentCell {
        get {
            return this.cell;
        }
        set {
            if(value == this.cell) {
                Actionable.moved.Add(this);
            }
            this.cell = value;
            this.coordinates = value.coordinates;
        }
    }
    #endregion

    public void InitPosition(Vector2Int coordinates) {
        this.coordinates = coordinates;
        if(this.GetType() == typeof(Cell)) {
            this.cell = this as Cell;
        } else {
            this.cell = BoardManager.Instance.GetCell(coordinates);
        }
    }

    public bool IsAdjacentTo(Actionable other) {
        var offset = other.coordinates - this.coordinates;
        return GameManager.Instance.allowedMovement.Allows(offset.GetDirection());
    }

    public Actionable FindNearest(System.Func<Actionable, bool> criteria, Actionable[] found = null) {
        Actionable ret = null;

        var heuristics = GameManager.Instance.allowedMovement.GetHeuristics();
        Actionable.all.Sort((a, b) => {
            if(criteria(a) && !criteria(b)) return -1;
            if(criteria(b) && !criteria(a)) return 1;
            if(criteria(a) && criteria(b)) return heuristics(a.coordinates, this.coordinates) - heuristics(b.coordinates, this.coordinates);
            return 0;
        });
        
        var arrayIndex = 0;
        foreach(var possibleOutcome in Actionable.all) {
            if(criteria(possibleOutcome)) {
                if(ret == null) {
                    ret = possibleOutcome;
                }
                if(found != null && found.Length > arrayIndex) {
                    found[arrayIndex++] = possibleOutcome;
                    arrayIndex++;
                } else {
                    break;
                }
            }
        }

        while(found != null && found.Length > arrayIndex) {
            found[arrayIndex++] = null;
        }

        return ret;
    }

    public Actionable FindNearest<T>(System.Func<T, bool> criteria, T[] found = null) where T : Actionable {
        return this.FindNearest((actionable) => actionable is T && criteria((T) actionable), found);
    }

    public virtual void Dispose() {
        Actionable.all.Remove(this);
        GameObject.Destroy(this.gameObject);
    }

    public int RequestDistanceFrom(Actionable other) {
        if(!this.CurrentCell.Distances.ContainsKey(other)) {
            Actionable.SpreadDistanceFromOrigin(other, this);
        }

        return this.CurrentCell.Distances[other];
    }

    private static void SpreadDistanceFromOrigin(Actionable origin, Actionable dest) {
         
        var currentCell = origin.CurrentCell;
        var destCell = dest.CurrentCell;
        var heuristics = GameManager.Instance.allowedMovement.GetHeuristics();

        currentCell.Distances[origin] = 0;
        Dictionary<Cell, Cell> predecessors = new Dictionary<Cell, Cell>();
        predecessors[currentCell] = null;
        List<Cell> pendingCells = new List<Cell>();
        List<Cell> proccessedCells = new List<Cell>();
        pendingCells.Add(currentCell);

        while(currentCell != destCell && pendingCells.Count > 0) {
            foreach(Cell neighbour in currentCell.GetNeighbours()) {
                var neighbourDistance = currentCell.Distances[origin] + 1; //TODO Cambiar esto cuando se implemente altura del terreno

                if(!neighbour.Distances.ContainsKey(origin) || neighbour.Distances[origin] > neighbourDistance) {
                    predecessors[neighbour] = currentCell;
                    neighbour.Distances[origin] = neighbourDistance;
                    proccessedCells.Remove(neighbour);
                }

                if(!pendingCells.Contains(neighbour) && !proccessedCells.Contains(neighbour)) {
                    pendingCells.Add(neighbour);
                }
            }
            
            pendingCells.Remove(currentCell);
            proccessedCells.Add(currentCell);
            pendingCells.Sort((a, b) => heuristics(a.coordinates, destCell.coordinates) - heuristics(b.coordinates, destCell.coordinates));
            currentCell = pendingCells[0];
        }
    }

    protected virtual void OnNextTurn(int turn) {
        foreach(Actionable actionable in Actionable.moved) {
            this.CurrentCell.Distances.Remove(actionable);
        }
    }

    protected virtual void OnLateNextTurn(int turn) {
        if(Actionable.moved.Count > 0) {
            Actionable.moved.Clear();
        }
    }
}
