using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Nation))]
public class Architect : MonoBehaviour {

    public Profiles.Enum profileMode;

    public event System.Action<Architect> onAttackedBy;

    public event System.Action<Architect, Architect> onDefeatedBy;

    private bool defeated;

    private Nation nation;

    private List<Architect> rivals;

    private Profile profile;

    #region Unity
    private void Awake() {
        this.nation = this.GetComponent<Nation>();
        this.rivals = new List<Architect>();
        this.profile = (Profile) this.profileMode;
        this.plan = new List<PlanUnit>();
        this.onAttackedBy += this.OnAttackedBy;
        this.onDefeatedBy += this.OnSelfDefeated;
        GameManager.Simulation.onNextTurn += this.OnNextTurn;
    }
    #endregion

    #region Accessors
    public Nation Nation {
        get {
            return this.nation;
        }
    }

    public List<Architect> Rivals {
        get {
            return this.rivals;
        }
    }

    public bool Defeated {
        get {
            return this.defeated;
        }
    }
    #endregion

    public int GetArmyAmount() {
        return this.Nation.GetArmyAmount();
    }

    public int GetArmyAmount(Suit suit) {
        return this.Nation.GetArmyAmount(suit);
    }

    public Dictionary<Stat, int> GetState() {
        var ret = new Dictionary<Stat, int>();
        foreach(Stat stat in Stats.GetAll()) {
            ret.Add(stat, stat.Read(this));
        }
        return ret;
    }

    private void OnNextTurn(int turn) {
        if(this.defeated) {
            return;
        }
        if(this.plan.Count == 0) {
            this.CreatePlan();
        }
        this.FollowPlan();
    }

    #region Eventos de ataque y derrota
    public void InvokeAttackedEvent(Architect other) {
        if(this.onAttackedBy != null) {
            this.onAttackedBy.Invoke(other);
        }
    }

    public void InvokeDefeatedEvent(Architect other, Architect subject) {
        this.defeated = true;
        if(this.onDefeatedBy != null) {
            this.onDefeatedBy.Invoke(other, subject);
        }
    }

    private void OnAttackedBy(Architect other) {
        this.rivals.Add(other);
        other.onDefeatedBy += this.OnRivalDefeated;
    }

    private void OnRivalDefeated(Architect other, Architect rival) {
        this.rivals.Remove(rival);
        rival.onDefeatedBy -= this.OnRivalDefeated;
    }

    private void OnSelfDefeated(Architect other, Architect rival) {
        //// GameObject.Destroy(this.gameObject);
    }
    #endregion

    #region Planificación
    private struct PlanUnit {
        public Action action;
        public Order order;
        public OrderInfo orderInfo;

        public PlanUnit(Action action, Order order, OrderInfo orderInfo) {
            this.action = action;
            this.order = order;
            this.orderInfo = orderInfo;
        }
    }

    private List<PlanUnit> plan;

    private void FollowPlan() {
        List<PlanUnit> removeUnits = new List<PlanUnit>();
        foreach(var planUnit in this.plan) {
            planUnit.order.Execute(planUnit.orderInfo);
            removeUnits.Add(planUnit);
        }
        foreach(var toRemove in removeUnits) {
            plan.Remove(toRemove);
        }
    }

    private void CreatePlan() {
        this.plan.Clear();
        var worldState = WorldState.Current;

        #region Ordenar acciones por consecuencias abstractas

        Dictionary<Action, float> actionConvenience = new Dictionary<Action, float>();

        Architect mostConvenientTargetArchitect = null;
        var maximumConvenience = -Mathf.Infinity;

        ActionInfo abstractInfo = new ActionInfo(subjectArchitect: this);
        foreach(var action in Actions.GetAll()) {
            foreach(var targetArchitect in GameManager.Instance.architectContainer.GetComponentsInChildren<Architect>()) {
                if(targetArchitect.Defeated) {
                    continue;
                }

                var nextWorldState = worldState.Clone();
                abstractInfo.targetArchitect = targetArchitect;
                action.ApplyConsequences(abstractInfo, nextWorldState);
                var convenience = this.profile.GetConvenienceFor(nextWorldState);
                if(convenience > maximumConvenience) {
                    maximumConvenience = convenience;
                    mostConvenientTargetArchitect = targetArchitect;
                }
            }
            actionConvenience[action] = maximumConvenience;
        }

        List<Action> possibleActions = new List<Action>(Actions.GetAll());
        #endregion

        #region Escoger posibles Actionables sujetos
        HashSet<Actionable> possibleSubjectActionables = new HashSet<Actionable>();

        possibleSubjectActionables.UnionWith(this.Nation.Armies);
        possibleSubjectActionables.UnionWith(this.Nation.Cells);
        #endregion

        #region Escoger posibles Actionables objetivos
        HashSet<Actionable> possibleTargetActionables = new HashSet<Actionable>();
        
        possibleTargetActionables.UnionWith(this.Nation.FindAdjacentUnclaimedCellsInWorldState(worldState));

        foreach(var rival in this.rivals) {
            possibleTargetActionables.UnionWith(rival.Nation.Armies);
        }

        var nearbyEnemies = new Army[16];
        foreach(var army in this.Nation.Armies) {
            this.FindClosestEnemiesToCell(army.CurrentCell, nearbyEnemies);
            possibleTargetActionables.UnionWith(nearbyEnemies);
        }

        possibleTargetActionables.Remove(null);
        #endregion

        Dictionary<Action, PlanUnit> possiblePlanUnits = new Dictionary<Action, PlanUnit>();
        
        for(var i = 0; i < possibleActions.Count; i++) {

            var action = possibleActions[i];

            #region Encontrar combinación de sujeto y objetivo más conveniente
            Actionable mostConvenientSubject = null;
            Actionable mostConvenientTarget = null;
            Order mostConvenientOrder = null;
            var bestConvenience = -Mathf.Infinity;

            foreach(var subject in possibleSubjectActionables) {

                if(action.RequiredSubjectType != null && subject.GetType() != action.RequiredSubjectType) {
                    continue;
                }

                foreach(var target in possibleTargetActionables) {

                    if(action.RequiredTargetType != null && target.GetType() != action.RequiredTargetType) {
                        continue;
                    }

                    var order = action.GetOrderForActionable(new ActionInfo(
                        subjectArchitect: this,
                        subject: subject,
                        targetArchitect: target is Army ? ((Army) target).Nation.Architect : ((Cell) target).Nation?.Architect,
                        target: target), worldState);
                    if(order.RequiredTargetType != null && target.GetType() != order.RequiredTargetType) {
                        continue;
                    }
                    var convenience = this.profile.GetConvenienceFor(subject, order, target, worldState);
                    if(convenience > bestConvenience) {
                        mostConvenientSubject = subject;
                        mostConvenientTarget = target;
                        mostConvenientOrder = order;
                        bestConvenience = convenience;
                    }
                }
            }
            #endregion

            if(mostConvenientOrder != null) {
                PlanUnit planUnitForThisAction = new PlanUnit(action, mostConvenientOrder,
                new OrderInfo(subject: mostConvenientSubject, target: mostConvenientTarget));

                possiblePlanUnits[action] = planUnitForThisAction;
                actionConvenience[action] += bestConvenience;
            } else {
                actionConvenience[action] = -Mathf.Infinity;
            }
        }

        possibleActions.Sort((a, b) => Mathf.RoundToInt(actionConvenience[b] - actionConvenience[a]));

        HashSet<Army> assignedArmies = new HashSet<Army>();
        foreach(var action in possibleActions) {
            if(!possiblePlanUnits.ContainsKey(action)) {
                continue;
            }
            var planUnit = possiblePlanUnits[action];

            if(!assignedArmies.Contains(planUnit.orderInfo.subject as Army)) {
                plan.Add(planUnit);
                assignedArmies.Add(planUnit.orderInfo.subject as Army);
            }
        }
    }

    private void FindClosestEnemiesToCell(Cell cell, Army[] result) {
        cell.FindNearestOfType<Army>((army) => army.Nation != this.Nation, result);
    }
    #endregion
}
