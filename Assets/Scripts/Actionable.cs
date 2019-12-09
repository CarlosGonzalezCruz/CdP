using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Clase base para aquellos elementos que pueden interactuar entre sí y cambiar el estado del mundo

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
        return offset.x >= -1 && offset.x <= 1 && offset.y >= -1 && offset.y <= 1
            && GameManager.Instance.allowedMovement.Allows(offset.GetDirection());
    }

    // Busca al actuable que cumpla un criterio y que sea más cercano a este. Si se le pasa un array, se llenará con
    // sucesivos actuables cercanos que también cumplan el criterio
    public Actionable FindNearest(System.Func<Actionable, bool> criteria, Actionable[] found = null) {
        Actionable ret = null;

        // Ordenamos la lista de actuables primero por cumplimiento del criterio, segundo por heurística
        var heuristics = GameManager.Instance.allowedMovement.GetHeuristics();
        Actionable.all.Sort((a, b) => {
            if(criteria(a) && !criteria(b)) return -1;
            if(criteria(b) && !criteria(a)) return 1;
            if(criteria(a) && criteria(b)) return heuristics(a.coordinates, this.coordinates) - heuristics(b.coordinates, this.coordinates);
            return 0;
        });
        
        // Los actuables más cercanos que cumplen el criterio están en la parte superior de la lista
        // Vamos obteniendo actuables según sea necesario hasta llenar el array especificado
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

        // Si no hemos llegado a llenar el array, vaciamos las posiciones de memoria que queden para no interferir
        // con el resto del programa
        while(found != null && found.Length > arrayIndex) {
            found[arrayIndex++] = null;
        }

        return ret;
    }

    // Igual que el método anterior, pero sólo encuentra actuables del subtipo especificado
    public Actionable FindNearestOfType<T>(System.Func<T, bool> criteria, T[] found = null) where T : Actionable {
        return this.FindNearest((actionable) => actionable is T && criteria((T) actionable), found);
    }

    public virtual void Dispose() {
        Actionable.all.Remove(this);
        GameObject.Destroy(this.gameObject);
    }

    // Calcula la distancia desde este actuable hasta el destino, o devuelve la que ya había calculado previamente si el destino
    // no se ha movido desde la última vez
    public int RequestDistanceFrom(Actionable other) {
        if(!this.CurrentCell.Distances.ContainsKey(other)) {
            Actionable.SpreadDistanceFromOrigin(other, this);
        }

        return this.CurrentCell.Distances[other];
    }

    // Partiendo del destino, asigna a todas las casillas que encuentra de camino al origen la distancia a la que queda
    // Si el destino no se mueve, otras peticiones pueden aprovechar este mismo cálculo y no hay que repetirlo
    private static void SpreadDistanceFromOrigin(Actionable origin, Actionable dest) {
         
        var currentCell = origin.CurrentCell;
        var destCell = dest.CurrentCell;
        var heuristics = GameManager.Instance.allowedMovement.GetHeuristics();

        // Partimos desde la casillas de destino, asignando distancia 0 y predecesor nulo
        currentCell.Distances[origin] = 0;
        Dictionary<Cell, Cell> predecessors = new Dictionary<Cell, Cell>();
        predecessors[currentCell] = null;
        List<Cell> pendingCells = new List<Cell>();
        List<Cell> proccessedCells = new List<Cell>();
        pendingCells.Add(currentCell);

        // Seguimos hasta que encontremos la casillas de destino o hasta que nos quedemos sin casillas que revisar
        while(currentCell != destCell && pendingCells.Count > 0) {
            foreach(Cell neighbour in currentCell.GetNeighbours()) {
                if(neighbour == null) {
                    continue;
                }

                // Marcamos como pendientes para mirar más tarde todas las casillas colindantes a la actual que no
                // hayamos mirado ya
                var neighbourDistance = currentCell.Distances[origin] + 1;

                if(!neighbour.Distances.ContainsKey(origin) || neighbour.Distances[origin] > neighbourDistance) {
                    predecessors[neighbour] = currentCell;
                    neighbour.Distances[origin] = neighbourDistance;

                    // Si la casilla está ahora más cerca que la última vez que lo comprobamos, quizás merezca la
                    // pena revisar esta ruta de nuevo
                    proccessedCells.Remove(neighbour);
                }

                // Si todavía no hemos revisado la casilla y no está marcada para revisar, marcarla
                if(!pendingCells.Contains(neighbour) && !proccessedCells.Contains(neighbour)) {
                    pendingCells.Add(neighbour);
                }
            }
            
            // La casilla actual ya está revisada. Ordenamos la lista siguiendo la heurística y pasamos a la siguiente
            pendingCells.Remove(currentCell);
            proccessedCells.Add(currentCell);
            pendingCells.Sort((a, b) => heuristics(a.coordinates, destCell.coordinates) - heuristics(b.coordinates, destCell.coordinates));
            currentCell = pendingCells[0];
        }
    }

    protected virtual void OnNextTurn(int turn) {
        this.CurrentCell.Distances.Clear();
        //// foreach(Actionable actionable in Actionable.moved) {
        ////    this.CurrentCell.Distances.Remove(actionable);
        ////}
    }

    protected virtual void OnLateNextTurn(int turn) {
        if(Actionable.moved.Count > 0) {
            Actionable.moved.Clear();
        }
    }
}
