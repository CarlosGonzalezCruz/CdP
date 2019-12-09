using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Se encarga de elegir acciones y dar órdenes a sus actuables asignados

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

    // Número total de tropas
    public int GetArmyAmount() {
        return this.Nation.GetArmyAmount();
    }

    // Número de tropas del suit
    public int GetArmyAmount(Suit suit) {
        return this.Nation.GetArmyAmount(suit);
    }

    // Devuelve el valor que tiene un stat si se consulta sobre este arquitecto en este momento
    public Dictionary<Stat, int> GetState() {
        var ret = new Dictionary<Stat, int>();
        foreach(Stat stat in Stats.GetAll()) {
            ret.Add(stat, stat.Read(this));
        }
        return ret;
    }

    private void OnNextTurn(int turn) {
        // No hacer nada si no estás participando
        if(this.defeated || this.Nation.Cells.Count <= 0) {
            return;
        }
        // Traza y sigue un plan
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
        // Si recibes un ataque, el otro arquitecto se considera un rival
        this.rivals.Add(other);
        other.onDefeatedBy += this.OnRivalDefeated;
    }

    private void OnRivalDefeated(Architect other, Architect rival) {
        // Si el rival es destruido, no lo consideramos más
        this.rivals.Remove(rival);
        rival.onDefeatedBy -= this.OnRivalDefeated;
    }

    private void OnSelfDefeated(Architect other, Architect rival) {
        //// GameObject.Destroy(this.gameObject);
    }
    #endregion

    #region Planificación

    // Struct que relaciona una acción, una orden, y los actuables involucrados
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

    // El plan para el turno es un conjunto de unidades de plan
    private List<PlanUnit> plan;

    // Sigue las unidades de plan establecidas y elimínalas al terminar
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

    // Traza un plan para seguir más adelante
    // Este método contiene el algoritmo principal
    private void CreatePlan() {
        this.plan.Clear();
        var worldState = WorldState.Current;

        #region Ordenar acciones por consecuencias abstractas

        Dictionary<Action, float> actionConvenience = new Dictionary<Action, float>();

        Architect mostConvenientTargetArchitect = null;
        var maximumConvenience = -Mathf.Infinity;

        // Exploramos todas las acciones sin entrar en detalle, para obtener una primera aproximación acerca de
        // qué acciones resultarían en un estado del mundo más favorable

        ActionInfo abstractInfo = new ActionInfo(subjectArchitect: this);
        foreach(var action in Actions.GetAll()) {
            foreach(var targetArchitect in GameManager.Instance.architectContainer.GetComponentsInChildren<Architect>()) {
                if(targetArchitect.Defeated) {
                    continue;
                }

                // Aplicamos las consecuencias sobre una copia del estado del mundo para comparar
                var nextWorldState = worldState.Clone();
                abstractInfo.targetArchitect = targetArchitect;
                action.ApplyConsequences(abstractInfo, nextWorldState);
                var convenience = this.profile.GetConvenienceFor(nextWorldState);

                if(convenience > maximumConvenience) {
                    maximumConvenience = convenience;
                    mostConvenientTargetArchitect = targetArchitect;
                }
            }

            // Esta es la conveniencia más alta posible que esta acción puede ofrecer
            actionConvenience[action] = maximumConvenience;
        }

        List<Action> possibleActions = new List<Action>(Actions.GetAll());
        #endregion

        #region Escoger posibles Actionables sujetos
        HashSet<Actionable> possibleSubjectActionables = new HashSet<Actionable>();

        // Son posibles sujetos todos los ejércitos y todas las celdas bajo nuestro control

        possibleSubjectActionables.UnionWith(this.Nation.Armies);
        possibleSubjectActionables.UnionWith(this.Nation.Cells);
        #endregion

        #region Escoger posibles Actionables objetivos
        HashSet<Actionable> possibleTargetActionables = new HashSet<Actionable>();
        
        // Son objetivos posibles todas las casillas adyacentes al territorio conquistado, todos los ejércitos de los rivales,
        // y los 16 ejércitos enemigos más cercanos
        
        possibleTargetActionables.UnionWith(this.Nation.FindAdjacentUnclaimedCellsInWorldState(worldState));

        foreach(var rival in this.rivals) {
            possibleTargetActionables.UnionWith(rival.Nation.Armies);
        }

        var nearbyEnemies = new Army[16];
        foreach(var army in this.Nation.Armies) {
            this.FindClosestEnemiesToCell(army.CurrentCell, nearbyEnemies);
            possibleTargetActionables.UnionWith(nearbyEnemies);
        }

        // En la búsqueda de posibles objetivos, es posible que se haya colado algún valor nulo. Lo eliminamos

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

                // Si el sujeto no puede participar en la acción, pasamos al siguiente directamente
                if(action.RequiredSubjectType != null && subject.GetType() != action.RequiredSubjectType) {
                    continue;
                }

                foreach(var target in possibleTargetActionables) {

                    // Si el objetivo no puede participar en la acción, pasamos al siguiente directamente
                    if(action.RequiredTargetType != null && target.GetType() != action.RequiredTargetType) {
                        continue;
                    }

                    // ¿Qué orden tendríamos que seguir para ejecutar esta acción, dados el sujeto y el objetivo indicados?

                    var order = action.GetOrderForActionable(new ActionInfo(
                        subjectArchitect: this,
                        subject: subject,
                        targetArchitect: target is Army ? ((Army) target).Nation.Architect : ((Cell) target).Nation?.Architect,
                        target: target), worldState);

                    // Es posible que el objetivo no pueda participar en la orden
                    if(order.RequiredTargetType != null && target.GetType() != order.RequiredTargetType) {
                        continue;
                    }

                    // ¿Es este el combo orden + sujeto + objetivo más conveniente que hemos encontrado harta ahora?
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

            // Si al menos hemos dado con una orden válida, generamos unidad de plan con los datos más óptimos que hemos encontrado
            // y actualizamos la conveniencia de la acción con los nuevos datos que tenemos
            if(mostConvenientOrder != null) {
                PlanUnit planUnitForThisAction = new PlanUnit(action, mostConvenientOrder,
                new OrderInfo(subject: mostConvenientSubject, target: mostConvenientTarget));

                possiblePlanUnits[action] = planUnitForThisAction;
                actionConvenience[action] += bestConvenience;

            // Si no la hemos encontrado, esta acción es completamente inviable y su conveniencia es mínima
            } else {
                actionConvenience[action] = -Mathf.Infinity;
            }
        }

        // Ordenamos las acciones ahora que sabemos con más precisión qué conveniencia tienen
        possibleActions.Sort((a, b) => Mathf.RoundToInt(actionConvenience[b] - actionConvenience[a]));

        // Asignamos unidades de plan a cada ejército. Como las acciones están ordenadas de más a menos convenientes,
        // iremos asignando unidades en efectividad decreciente
        HashSet<Army> assignedArmies = new HashSet<Army>();
        foreach(var action in possibleActions) {
            if(!possiblePlanUnits.ContainsKey(action)) {
                continue;
            }
            var planUnit = possiblePlanUnits[action];

            // Si el ejército al que asignaríamos esta orden está ocioso, se la asignamos. Si no, es que ya tiene una más efectiva
            if(!assignedArmies.Contains(planUnit.orderInfo.subject as Army)) {
                plan.Add(planUnit);
                assignedArmies.Add(planUnit.orderInfo.subject as Army);
            }
        }
    }

    // Función auxiliar que busca los ejércitos enemigos más cercanos a la casilla indicada. Se le asigna un array donde depositar
    // los resultados para que, ya que es necesario utilizarlo repetidas veces, no sea necesario consumir recursos en asignar memoria
    private void FindClosestEnemiesToCell(Cell cell, Army[] result) {
        cell.FindNearestOfType<Army>((army) => army.Nation != this.Nation, result);
    }
    #endregion
}
