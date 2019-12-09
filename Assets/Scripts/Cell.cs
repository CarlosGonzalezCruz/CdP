using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CellRenderer))]
public class Cell : Actionable {

    [SerializeField]
    private Nation nation;

    [SerializeField]
    private Army army;

    [SerializeField]
    private Suit? scheduledTroop;

    [SerializeField]
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
        if(this.nation != null) {
            this.ConnectToNation();
        }
    }

    protected override void Start() {
        this.RegisterNeighbours();
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
            if(this.nation == value) {
                return;
            }
            if(this.nation != null) {
                this.nation.Cells.Remove(this);
            }
            value.Cells.Add(this);
            if(this.nation != null && this.nation.Size == 0) {
                this.nation.Architect.InvokeDefeatedEvent(value.Architect, this.nation.Architect);
            }
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

    public void ConnectToNation() {
        this.Nation = this.nation;
        this.Nation.Cells.Add(this);
    }

    public void ScheduleArmy(Suit suit) {
        if(this.scheduledTroopCounter <= 0) {
            if(Random.value < 0.1) {
                var values = System.Enum.GetValues(typeof(Suit));
                this.scheduledTroop = (Suit) values.GetValue(Mathf.RoundToInt(Random.Range(0, values.Length)));
            } else {
                this.scheduledTroop = suit;
            }
            this.scheduledTroopCounter = 3;
        }
    }

    public override string ToString() {
        return $"Cell at position {this.Coordinates} belonging to nation {this.Nation}";
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
                var army = Army.Instantiate(this, this.Nation, this.scheduledTroop.Value).GetComponent<Army>();
                army.Troops = 10;
                this.scheduledTroop = null;
            }
        }
    }
}
