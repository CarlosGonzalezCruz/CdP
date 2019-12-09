using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PieceRenderer))]
public class Army : Actionable {

    public const int WEAKNESS_DAMAGE = 2;

    public const int REGULAR_DAMAGE = 1;

    private Suit suit;

    private Direction direction;

    [SerializeField]
    private Nation nation;

    [SerializeField]
    private int troops;

    private new PieceRenderer renderer;

    #region Unity
    protected override void Awake() {
        base.Awake();
        this.renderer = this.GetComponent<PieceRenderer>();
    }

    protected override void Start() {
        base.Start();
        this.CurrentCell = this.CurrentCell ?? BoardManager.Instance.GetCell(Vector2Int.zero);
        this.Troops = this.Troops;
        this.nation.Architect.onDefeatedBy += this.OnArchitectDefeated;
        this.nation.Armies.Add(this);
    }

    protected override void Update() {
        base.Update();
        this.transform.position = new Vector3(this.CurrentCell.transform.position.x, this.CurrentCell.RenderedHeight, this.CurrentCell.transform.position.z);
        this.transform.rotation = Quaternion.Euler(0, direction.GetAngle(), 0);
    }
    #endregion

    #region Accessors
    public override Cell CurrentCell {
        set {
            if(this.CurrentCell != null && this.CurrentCell.Army == this) {
                this.CurrentCell.Army = null;
            }

            base.CurrentCell = value;

            if(this.CurrentCell.Army == null) {
                this.CurrentCell.Army = this;
            }
        }
    }

    public int Troops {
        get {
            return this.troops;
        }
        set {
            this.troops = value;
            this.renderer.Number = value;
            
            if(value <= 0) {
                this.Dispose();
            }
        }
    }

    public Direction Direction {
        get {
            return this.direction;
        }
        private set {
            this.direction = value;
        }
    }

    public Suit Suit {
        get {
            return this.suit;
        }
        private set {
            this.suit = value;
        }
    }

    public Nation Nation {
        get {
            return this.nation;
        }
        private set {
            this.nation = value;
            this.ApplyNationalColor();
        }
    }
    #endregion

    #region Orders
    public void Move(Direction direction) {
        this.direction = direction;
        var targetCell = this.CurrentCell.GetNeighbour(direction);
        if(targetCell?.Army == null) {
            this.CurrentCell = targetCell;
        }
    }

    public void Attack(Direction direction) {
        this.direction = direction;
        var target = this.CurrentCell.GetNeighbour(direction)?.Army;
        if(target != null) {
            target.Troops -= target.suit.IsWeakAgainst(this.suit) ? WEAKNESS_DAMAGE : REGULAR_DAMAGE;
            target.Nation.Architect.InvokeAttackedEvent(this.Nation.Architect);
        }
    }

    public void Join(Direction direction) {
        this.direction = direction;
        var target = this.CurrentCell.GetNeighbour(direction)?.Army;
        if(target != null) {
            target.Troops += this.Troops;
            this.Dispose();
        }
    }

    public void Split(Direction direction) {
        this.direction = direction;
        var targetCell = this.CurrentCell.GetNeighbour(direction);
        var newArmyTroops = Mathf.FloorToInt(this.Troops * 0.5f);
        if(targetCell != null && targetCell.Army == null && newArmyTroops > 0) {
            var newArmy = GameObject.Instantiate(this.gameObject).GetComponent<Army>();
            newArmy.Troops = newArmyTroops;
            newArmy.CurrentCell = targetCell;
            this.Troops -= newArmyTroops;
        }
    }

    public void Claim() {
        this.CurrentCell.Nation = this.nation;
    }

    public override void Dispose() {
        base.Dispose();
        this.renderer.Dispose();
        this.CurrentCell.Army = null;
        this.Nation.Armies.Remove(this);
        GameManager.Simulation.onNextTurn -= this.OnNextTurn;
    }
    #endregion

    public override string ToString() {
        return $"Army of suit {this.Suit} at position {this.Coordinates} belonging to nation {this.Nation}";
    }

    public static Army Instantiate(Cell cell, Nation nation, Suit suit) {
        var ret = GameObject.Instantiate(GameManager.Instance.armyPrefab, GameManager.Instance.armyContainer.transform).GetComponent<Army>();
        ret.Nation = nation;
        ret.Suit = suit;
        ret.CurrentCell = cell;
        return ret;
    }

    protected override void OnNextTurn(int turn) {
        base.OnNextTurn(turn);
        this.ApplyNationalColor();
        this.renderer.Suit = this.suit;
    }

    private void ApplyNationalColor() {
        if (this.nation != null) {
            this.renderer.Color = this.nation.armyColor;
        }
        else {
            this.renderer.Color = GameManager.Instance.unclaimedColor;
        }
    }

    private void OnArchitectDefeated(Architect other, Architect subject) {
        this.Nation.Armies.Remove(this);
        this.Nation = other.Nation;
        this.Nation.Armies.Add(this);
    }
}
