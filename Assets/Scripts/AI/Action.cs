using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Struct que usaremos para enviar datos concretos a las acciones
public struct ActionInfo {
    // Arquitectos participantes
    public Architect subjectArchitect;
    public Architect targetArchitect;
    // Actuables participantes
    public Actionable subject;
    public Actionable target;

    public ActionInfo(Architect subjectArchitect = null, Architect targetArchitect = null, Actionable subject = null, Actionable target = null) {
        this.subjectArchitect = subjectArchitect;
        this.targetArchitect = targetArchitect;
        this.subject = subject;
        this.target = target;
    }
}

// Acciones que producen cambios de estado en el mundo

public class Action {

    public delegate void GetAbstractConsequences(ActionInfo info, WorldState state);
    public delegate Order GetOrder(ActionInfo info, WorldState state);

    public string name;

    private System.Type subjectType;
    private System.Type targetType;
    private GetAbstractConsequences consequences;
    private GetOrder order;

    public Action(GetAbstractConsequences consequences, GetOrder order, System.Type requiredSubjectType = null, System.Type requiredTargetType = null, string name = "Action") {
        this.name = name;
        this.consequences = consequences;
        this.order = order;
        this.subjectType = requiredSubjectType;
        this.targetType = requiredTargetType;
        Actions.RegisterAction(this);
    }

    public void ApplyConsequences(ActionInfo info, WorldState state) {
        if((this.subjectType == null || info.subject != null && info.subject.GetType() != this.subjectType)
           && (this.targetType == null || info.target != null && info.target.GetType() != this.targetType)) {
            // Los tipos no encajan y la consecuencia no tiene sentido
            return;
        }
        this.consequences(info, state);
    }

    public Order GetOrderForActionable(ActionInfo info, WorldState state) {
        if ((this.subjectType == null || info.subject.GetType() != this.subjectType)
           && (this.targetType == null || info.target.GetType() != this.targetType)) {
            // Los tipos no encajan y la orden no tiene sentido
            throw new System.Exception("No se puede obtener la orden de la acción porque los tipos de los Actionable no encajan.");
        }
        return this.order(info, state);
    }

    public override string ToString() {
        return this.name;
    }

    #region Accessors
    public System.Type RequiredSubjectType {
        get {
            return this.subjectType;
        }
    }

    public System.Type RequiredTargetType {
        get {
            return this.targetType;
        }
    }
    #endregion
}

public static class Actions {

    // Acción de conquistar una casilla
    public static readonly Action CLAIM = new Action(

        name: "Claim",

        requiredSubjectType: typeof(Army),
        requiredTargetType: typeof(Cell),

        consequences: (ActionInfo info, WorldState state) => {
            var targetCell = info.target as Cell;

            // Si la casilla se ha especificado, revisamos más concretamente el impacto que tendrá conquistar esta casilla
            if(targetCell != null) {
                if(targetCell.Nation != null && targetCell.Nation != info.subjectArchitect.Nation) {
                    state[targetCell.Nation.Architect][Stats.SIZE] -= 1;
                }
                if(targetCell.Nation != info.subjectArchitect.Nation) {
                    state[info.subjectArchitect][Stats.SIZE] += 1;
                }
                state.CellClaims[targetCell] = info.subjectArchitect.Nation;
            
            // Si no, estimación optimista
            } else {
                state[info.subjectArchitect][Stats.SIZE] += 1;
            }
            
            // El ejército que la ha conquistado estaría ahora en ella
            if(info.subject != null) {
                state.ArmiesPosition[(Army) info.subject] = targetCell;
            }
        },
        
        order: (ActionInfo info, WorldState state) => {
            Order ret = null;

            // Si no estamos en la casilla, ir hacia ella
            if(info.target != null && info.subject.CurrentCell != info.target) {
                ret = Orders.MOVE_TOWARDS;

            // Si lo estamos, conquistarla
            } else {
                ret = Orders.CLAIM;
            }
            return ret;
        }
    );


    // Atacar ejército de tipo Pica
    public static readonly Action ATTACK_SPADE_ARMY = new Action(

        name: "Attack Spade army",
        
        requiredSubjectType: typeof(Army),
        requiredTargetType: typeof(Army),

        consequences: (ActionInfo info, WorldState state) => {
            Actions.ApplyConsequencesOfAttacking(info, state, Suit.SPADE);
            
            // El ejército estaría ahora aquí
            if(info.subject != null && info.target != null) {
                state.ArmiesPosition[(Army) info.subject] = info.target.CurrentCell;
            }
        },

        order: (ActionInfo info, WorldState state) => {

            // Si no sabemos el objetivo, por defecto la orden es atacar
            Order ret = Orders.ATTACK;
            if(info.target == null) {
                return ret;
            }

            // Si lo sabemos, iremos hacia él si no estamos a su lado
            if(info.subject.IsAdjacentTo(info.target)) {
                ret = Orders.ATTACK;
            } else {
                ret = Orders.MOVE_TOWARDS;
            }
            return ret;
        }
    );

    // Atacar ejército de tipo Corazón
    public static readonly Action ATTACK_HEART_ARMY = new Action(
        
        name: "Attack Heart army",

        requiredSubjectType: typeof(Army),
        requiredTargetType: typeof(Army),

        consequences: (ActionInfo info, WorldState state) => {
            Actions.ApplyConsequencesOfAttacking(info, state, Suit.HEART);
            
            // El ejército estaría ahora aquí
            if(info.subject != null && info.target != null) {
                state.ArmiesPosition[(Army) info.subject] = info.target.CurrentCell;
            }
        },

        order: (ActionInfo info, WorldState state) => {
            // Si no sabemos el objetivo, por defecto la orden es atacar
            Order ret = Orders.ATTACK;
            if(info.target == null) {
                return ret;
            }

            // Si lo sabemos, iremos hacia él si no estamos a su lado
            if(info.subject.IsAdjacentTo(info.target)) {
                ret = Orders.ATTACK;
            } else {
                ret = Orders.MOVE_TOWARDS;
            }
            return ret;
        }
    );

    // Atacar ejército de tipo Trébol
    public static readonly Action ATTACK_CLUB_ARMY = new Action(

        name: "Attack club army",

        requiredSubjectType: typeof(Army),
        requiredTargetType: typeof(Army),

        consequences: (ActionInfo info, WorldState state) => {
            Actions.ApplyConsequencesOfAttacking(info, state, Suit.CLUB);
            
            // El ejército estaría ahora aquí
            if(info.subject != null && info.target != null) {
                state.ArmiesPosition[(Army) info.subject] = info.target.CurrentCell;
            }
        },

        order: (ActionInfo info, WorldState state) => {
            // Si no sabemos el objetivo, por defecto la orden es atacar
            Order ret = Orders.ATTACK;
            if(info.target == null) {
                return ret;
            }

            // Si lo sabemos, iremos hacia él si no estamos a su lado
            if(info.subject.IsAdjacentTo(info.target)) {
                ret = Orders.ATTACK;
            } else {
                ret = Orders.MOVE_TOWARDS;
            }
            return ret;
        }
    );

    // Atacar ejército de tipo Diamante
    public static readonly Action ATTACK_DIAMOND_ARMY = new Action(

        name: "Attack diamond army",

        requiredSubjectType: typeof(Army),
        requiredTargetType: typeof(Army),

        consequences: (ActionInfo info, WorldState state) => {
            Actions.ApplyConsequencesOfAttacking(info, state, Suit.DIAMOND);
            
            // El ejército estaría ahora aquí
            if(info.subject != null && info.target != null) {
                state.ArmiesPosition[(Army) info.subject] = info.target.CurrentCell;
            }
        },

        order: (ActionInfo info, WorldState state) => {
            // Si no sabemos el objetivo, por defecto la orden es atacar
            Order ret = Orders.ATTACK;
            if(info.target == null) {
                return ret;
            }

            // Si lo sabemos, iremos hacia él si no estamos a su lado
            if(info.subject.IsAdjacentTo(info.target)) {
                ret = Orders.ATTACK;
            } else {
                ret = Orders.MOVE_TOWARDS;
            }
            return ret;
        }
    );

    // Crear ejército de tipo Pica
    public static readonly Action CREATE_SPADE_ARMY = new Action(

        name: "Create Spade army",

        requiredSubjectType: typeof(Cell),

        consequences: (ActionInfo info, WorldState state) => {
            state[info.subjectArchitect][Stats.FULL_ARMY_AMOUNT] += 1;
            state[info.subjectArchitect][Stats.SPADE_AMOUNT] += 1;
        },

        order: (ActionInfo info, WorldState state) => {
            return Orders.BUILD_SPADE;
        }
    );

    // Crear ejército de tipo Corazón
    public static readonly Action CREATE_HEART_ARMY = new Action(

        name: "Create Heart army",

        requiredSubjectType: typeof(Cell),

        consequences: (ActionInfo info, WorldState state) => {
            state[info.subjectArchitect][Stats.FULL_ARMY_AMOUNT] += 1;
            state[info.subjectArchitect][Stats.SPADE_AMOUNT] += 1;
        },

        order: (ActionInfo info, WorldState state) => {
            return Orders.BUILD_HEART;
        }
    );

    // Crear ejército de tipo Trébol
    public static readonly Action CREATE_CLUB_ARMY = new Action(

        name: "Create Club army",

        requiredSubjectType: typeof(Cell),

        consequences: (ActionInfo info, WorldState state) => {
            state[info.subjectArchitect][Stats.FULL_ARMY_AMOUNT] += 1;
            state[info.subjectArchitect][Stats.SPADE_AMOUNT] += 1;
        },

        order: (ActionInfo info, WorldState state) => {
            return Orders.BUILD_CLUB;
        }
    );

    // Crear ejército de tipo Diamante
    public static readonly Action CREATE_DIAMOND_ARMY = new Action(

        name: "Create Diamond army",

        requiredSubjectType: typeof(Cell),

        consequences: (ActionInfo info, WorldState state) => {
            state[info.subjectArchitect][Stats.FULL_ARMY_AMOUNT] += 1;
            state[info.subjectArchitect][Stats.SPADE_AMOUNT] += 1;
        },

        order: (ActionInfo info, WorldState state) => {
            return Orders.BUILD_DIAMOND;
        }
    );

    // La siguiente función abstrae todas las posibilidades que hay que tener en cuenta en todas las acciones de atacar
    private static void ApplyConsequencesOfAttacking(ActionInfo info, WorldState state, Suit suit) {
        var subjectArmy = info.subject as Army;
        var targetArmy = info.target as Army;

        // Como mínimo, el bando que ataca va a perder una tropa
        state[info.subjectArchitect][Stats.FULL_ARMY_AMOUNT] -= 1;

        if(subjectArmy != null) {

            // Si sabemos el tipo, también podemos tener en cuenta que perderíamos una tropa de ese tipo
            state[info.subjectArchitect][Stats.AmountOf(subjectArmy.Suit)] -= 1;

            // Si el rival es débil perderá dos unidades suyas
            if(targetArmy.Suit.IsWeakAgainst(subjectArmy.Suit)) {
                state[info.targetArchitect][Stats.AmountOf(suit)] -= 2;
                state[info.targetArchitect][Stats.FULL_ARMY_AMOUNT] -= 2;

            // Si el atacante es débil perderá otra tropa adicional (en efecto perdiendo dos unidades también)
            } else if(subjectArmy.Suit.IsWeakAgainst(targetArmy.Suit)) {
                state[info.subjectArchitect][Stats.AmountOf(suit)] -= 1;
                state[info.subjectArchitect][Stats.FULL_ARMY_AMOUNT] -= 1;
                state[info.targetArchitect][Stats.AmountOf(suit)] -= 1;
                state[info.targetArchitect][Stats.FULL_ARMY_AMOUNT] -= 1;

            // En otro caso, el enemigo simplemente pierde una tropa también
            } else {
                state[info.targetArchitect][Stats.AmountOf(suit)] -= 1;
                state[info.targetArchitect][Stats.FULL_ARMY_AMOUNT] -= 1;
            }
        
        // Estimación optimista por desconocimiento: El ejército enemigo es débil al ataque
        } else {
            state[info.targetArchitect][Stats.AmountOf(suit)] -= 2;
            state[info.targetArchitect][Stats.FULL_ARMY_AMOUNT] -= 2;
        }

        // La cantidad de tropas no puede bajar de cero
        state[info.targetArchitect][Stats.AmountOf(suit)] = Mathf.Max(0, state[info.targetArchitect][Stats.AmountOf(suit)]);
        state[info.subjectArchitect][Stats.FULL_ARMY_AMOUNT] = Mathf.Max(0, state[info.subjectArchitect][Stats.FULL_ARMY_AMOUNT]);
        state[info.targetArchitect][Stats.AmountOf(suit)] = Mathf.Max(0, state[info.targetArchitect][Stats.AmountOf(suit)]);
        state[info.targetArchitect][Stats.FULL_ARMY_AMOUNT] = Mathf.Max(0, state[info.targetArchitect][Stats.FULL_ARMY_AMOUNT]);
    }

    #region Permitir iterar acciones
    private static List<Action> actions;
    public static void RegisterAction(Action action) {
        if(Actions.actions == null) {
            Actions.actions = new List<Action>();
        }
        Actions.actions.Add(action);
    }

    public static List<Action> GetAll() {
        return Actions.actions;
    }
    #endregion
}
