using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ArchitectState = System.Collections.Generic.Dictionary<Stat, int>;

public struct ActionInfo {
    public Architect subjectArchitect;
    public Architect targetArchitect;
    public Actionable subject;
    public Actionable target;

    public ActionInfo(Architect subjectArchitect = null, Architect targetArchitect = null, Actionable subject = null, Actionable target = null) {
        this.subjectArchitect = subjectArchitect;
        this.targetArchitect = targetArchitect;
        this.subject = subject;
        this.target = target;
    }
}

public class Action {

    public delegate void GetAbstractConsequences(ActionInfo info, WorldState state);
    public delegate Order GetOrder(ActionInfo info, WorldState state);

    private System.Type subjectType;
    private System.Type targetType;
    private GetAbstractConsequences consequences;
    private GetOrder order;

    public Action(GetAbstractConsequences consequences, GetOrder order, System.Type requiredSubjectType = null, System.Type requiredTargetType = null) {
        this.consequences = consequences;
        this.order = order;
        this.subjectType = requiredSubjectType;
        this.targetType = requiredTargetType;
    }

    public void ApplyConsequences(ActionInfo info, WorldState state) {
        if((this.subjectType == null || info.subject.GetType() != this.subjectType)
           && (this.targetType == null || info.target.GetType() != this.targetType)) {
            // Los tipos no encajan y la consecuencia no tiene sentido
            return;
        }
        this.consequences(info, state);
    }

    public Order GetOrderForArmy(ActionInfo info, WorldState state) {
        if(info.subject != null) {
            // No hay ejército al que dar una orden
            throw new System.Exception("No hay un ejército al que dar una orden.");
        }
        if ((this.subjectType == null || info.subject.GetType() != this.subjectType)
           && (this.targetType == null || info.target.GetType() != this.targetType)) {
            // Los tipos no encajan y la orden no tiene sentido
            throw new System.Exception("No se puede obtener la orden de la acción porque los tipos de los Actionable no encajan.");
        }
        return this.order(info, state);
    }
}

public static class Actions {

    public static readonly Action CLAIM = new Action(

        requiredSubjectType: typeof(Army),
        requiredTargetType: typeof(Cell),

        consequences: (ActionInfo info, WorldState state) => {
            var targetCell = info.target as Cell;

            if(targetCell != null) {
                if(targetCell.Nation != null && targetCell.Nation != info.subjectArchitect.Nation) {
                    state[targetCell.Nation.Architect][Stats.SIZE] -= 1;
                }
                if(targetCell.Nation != info.subjectArchitect.Nation) {
                    state[info.subjectArchitect][Stats.SIZE] += 1;
                }
                state.CellClaims[targetCell] = info.subjectArchitect.Nation;
            
            } else {
                state[info.subjectArchitect][Stats.SIZE] += 1;
            }
            
            if(info.subject != null) {
                state.ArmiesPosition[(Army) info.subject] = targetCell;
            }
        },
        
        order: (ActionInfo info, WorldState state) => {
            Order ret = null;
            if(info.subject.CurrentCell == info.target) {
                ret = Orders.CLAIM;
            } else {
                ret = Orders.MOVE_TOWARDS;
            }
            return ret;
        }
    );

    public static readonly Action ATTACK_SPADE_ARMY = new Action(
        
        requiredSubjectType: typeof(Army),
        requiredTargetType: typeof(Army),

        consequences: (ActionInfo info, WorldState state) => {
            Actions.ApplyConsequencesOfAttacking(info, state, Suit.SPADE);
            if(info.subject != null && info.target != null) {
                state.ArmiesPosition[(Army) info.subject] = info.target.CurrentCell;
            }
        },

        order: (ActionInfo info, WorldState state) => {
            Order ret = null;
            if(info.target == null) {
                return ret;
            }

            if(info.subject.IsAdjacentTo(info.target)) {
                ret = Orders.ATTACK;
            } else {
                ret = Orders.MOVE_TOWARDS;
            }
            return ret;
        }
    );

    public static readonly Action ATTACK_HEART_ARMY = new Action(
        
        requiredSubjectType: typeof(Army),
        requiredTargetType: typeof(Army),

        consequences: (ActionInfo info, WorldState state) => {
            Actions.ApplyConsequencesOfAttacking(info, state, Suit.HEART);
            if(info.subject != null && info.target != null) {
                state.ArmiesPosition[(Army) info.subject] = info.target.CurrentCell;
            }
        },

        order: (ActionInfo info, WorldState state) => {
            Order ret = null;
            if(info.target == null) {
                return ret;
            }

            if(info.subject.IsAdjacentTo(info.target)) {
                ret = Orders.ATTACK;
            } else {
                ret = Orders.MOVE_TOWARDS;
            }
            return ret;
        }
    );

    public static readonly Action ATTACK_CLUB_ARMY = new Action(

        requiredSubjectType: typeof(Army),
        requiredTargetType: typeof(Army),

        consequences: (ActionInfo info, WorldState state) => {
            Actions.ApplyConsequencesOfAttacking(info, state, Suit.CLUB);
            if(info.subject != null && info.target != null) {
                state.ArmiesPosition[(Army) info.subject] = info.target.CurrentCell;
            }
        },

        order: (ActionInfo info, WorldState state) => {
            Order ret = null;
            if(info.target == null) {
                return ret;
            }

            if(info.subject.IsAdjacentTo(info.target)) {
                ret = Orders.ATTACK;
            } else {
                ret = Orders.MOVE_TOWARDS;
            }
            return ret;
        }
    );

    public static readonly Action ATTACK_DIAMOND_ARMY = new Action(

        requiredSubjectType: typeof(Army),
        requiredTargetType: typeof(Army),

        consequences: (ActionInfo info, WorldState state) => {
            Actions.ApplyConsequencesOfAttacking(info, state, Suit.DIAMOND);
            if(info.subject != null && info.target != null) {
                state.ArmiesPosition[(Army) info.subject] = info.target.CurrentCell;
            }
        },

        order: (ActionInfo info, WorldState state) => {
            Order ret = null;
            if(info.target == null) {
                return ret;
            }

            if(info.subject.IsAdjacentTo(info.target)) {
                ret = Orders.ATTACK;
            } else {
                ret = Orders.MOVE_TOWARDS;
            }
            return ret;
        }
    );


    private static void ApplyConsequencesOfAttacking(ActionInfo info, WorldState state, Suit suit) {
        var subjectArmy = info.subject as Army;
        var targetArmy = info.target as Army;

        state[info.subjectArchitect][Stats.FULL_ARMY_AMOUNT] -= 1;

        if(info.subject != null) {
            
            state[info.subjectArchitect][Stats.AmountOf(subjectArmy.Suit)] -= 1;
            
            if(targetArmy.Suit.IsWeakAgainst(subjectArmy.Suit)) {
                state[info.targetArchitect][Stats.AmountOf(suit)] -= 2;
                state[info.targetArchitect][Stats.FULL_ARMY_AMOUNT] -= 2;
            } else {
                state[info.targetArchitect][Stats.AmountOf(suit)] -= 1;
                state[info.targetArchitect][Stats.FULL_ARMY_AMOUNT] -= 1;
            }
        
        } else {
            state[info.targetArchitect][Stats.AmountOf(suit)] -= 2;
            state[info.targetArchitect][Stats.FULL_ARMY_AMOUNT] -= 2;
        }
    }

    #region Allow iteration through actions
    private static List<Action> actions;
    public static void RegisterStat(Action action) {
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
