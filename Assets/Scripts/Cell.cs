using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CellRenderer))]
public class Cell : Actionable {

    [SerializeField]
    private Nation nation;

    [SerializeField]
    private Army army;

    private Suit? scheduledTroop;

    private int scheduledTroopCounter;

    private Dictionary<Direction, Cell> neighbours;

    private Dictionary<Actionable, int> distances;

    private new CellRenderer renderer;

    #region Unity
    protected override void Awake() {
        base.Awake();
        this.renderer = GetComponent<CellRenderer>();
        this.neighbours = new Dictionary<Direction, Cell>();
        this.scheduledTroop = null;
        this.scheduledTroopCounter = 0;
        this.distances = new Dictionary<Actionable, int>();
    }

    protected override void Start() {
        this.RegisterNeighbours();
    }

    protected void OnGUI() {
        UnityEditor.Handles.color = Color.black;
        var style = new GUIStyle();
        style.normal.textColor = Color.black;
        var playerCell = GameObject.Find("Piece red").GetComponent<Army>().testDestination;
        if(this.distances.ContainsKey(playerCell)) {
            UnityEditor.Handles.Label(this.transform.position + Vector3.up, this.distances[playerCell].ToString(), style);
        }
    }
    #endregion

    #region Accessors
    public override Cell CurrentCell {
        get {
            return this;
        }
        set {
            // Ignorar en silencio
        }
    }

    public float RenderedHeight {
        get {
            return this.renderer.RenderedHeight;
        }
        set {
            this.renderer.RenderedHeight = value;
        }
    }

    public Army Army {
        get {
            return this.army;
        }
        set {
            this.army = value;
        }
    }

    public Nation Nation {
        get {
            return this.nation;
        }
        set {
            this.nation = value;
            this.scheduledTroop = null;
        }
    }

    public Dictionary<Actionable, int> Distances {
        get {
            return this.distances;
        }
    }
    #endregion

    public Cell GetNeighbour(Direction direction) {
        return this.neighbours[direction];
    }

    public List<Cell> GetNeighbours() {
        return this.neighbours.Values.ToList();
    }

    public void ScheduleArmy(Suit suit) {
        this.scheduledTroop = suit;
    }

    protected override void OnNextTurn(int turn) {
        base.OnNextTurn(turn);
        this.ApplyNationalColor();
        this.CreateScheduledArmies();       
    }

    private void ApplyNationalColor() {
        if (this.nation != null) {
            this.renderer.Color = this.nation.cellColor;
        } else {
            this.renderer.Color = GameManager.Instance.unclaimedColor;
        }
    }

    private void RegisterNeighbours() {
        foreach(var direction in GameManager.Instance.allowedMovement.GetAllowedDirections()) {
            this.neighbours[direction] = (BoardManager.Instance.GetCell(this.Coordinates + direction.GetVector()));
        }
    }

    private void CreateScheduledArmies() {
        if(this.Army != null &&
          (this.Army.Nation != this.Nation || (this.scheduledTroop.HasValue && this.Army.Suit != this.scheduledTroop.Value))) {
            return;
        }

        if (this.scheduledTroop.HasValue) {
            this.scheduledTroopCounter--;
            if (this.scheduledTroopCounter <= 0) {
                Army.Instantiate(this, this.Nation, this.scheduledTroop.Value);
                this.scheduledTroop = null;
            }
        }
    }
}
