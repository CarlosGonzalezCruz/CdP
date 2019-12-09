using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OrderInfo {
    public Actionable subject;
    public Actionable target;

    public OrderInfo(Actionable subject = null, Actionable target = null) {
        this.subject = subject;
        this.target = target;
    }
}

public class Order {

    public delegate void OrderExecution(OrderInfo info);

    public string name;

    private System.Type subjectType;

    private System.Type targetType;

    private OrderExecution execution;

    public Order(OrderExecution execution, System.Type requiredSubjectType = null, System.Type requiredTargetType = null, string name = "Order") {
        this.name = name;
        this.execution = execution;
        this.subjectType = requiredSubjectType;
        this.targetType = requiredTargetType;
    }

    public void Execute(OrderInfo info) {
        if((this.subjectType == null || info.subject.GetType() != this.subjectType)
        && (this.targetType == null || info.target.GetType() != this.targetType)) {
            // Los tipos no encajan y la consecuencia no se puede ejecutar
            return;
        }
        this.execution(info);
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

public static class Orders {
    
    public static readonly Order MOVE_TOWARDS = new Order(

        name: "Move towards",

        requiredSubjectType: typeof(Army),

        execution: (info) => {
            var cell = info.subject.CurrentCell;
            var minimumDistance = int.MaxValue;
            Direction chosenDirection = GameManager.Instance.allowedMovement.GetDefaultDirection();
            foreach(var direction in GameManager.Instance.allowedMovement.GetAllowedDirections()) {
                var adjacentCell = cell.GetNeighbour(direction);
                if(adjacentCell == null) {
                    continue;
                }
                if(adjacentCell != null && adjacentCell.Army != null && adjacentCell.Army.Nation != ((Army) info.subject).Nation) {
                    continue;
                }
                
                var distance = adjacentCell.RequestDistanceFrom(info.target);
                if(distance < minimumDistance) {
                    minimumDistance = distance;
                    chosenDirection = direction;
                }
            }
            if(cell.RequestDistanceFrom(info.target) > minimumDistance) {
                var possibleAlly = cell.GetNeighbour(chosenDirection)?.Army;
                if(possibleAlly != null && possibleAlly.Nation == ((Army) info.subject).Nation) {
                    ((Army) info.subject).Join(chosenDirection);
                } else if(possibleAlly != null) {
                    ((Army) info.subject).Attack(chosenDirection);
                } else {
                    ((Army) info.subject).Move(chosenDirection);
                }
            }
        }
    );

    public static readonly Order CLAIM = new Order(

        name: "Claim",

        requiredSubjectType: typeof(Army),

        execution: (info) => {
            ((Army) info.subject).Claim();
        }
    );

    public static readonly Order ATTACK = new Order(

        name: "Attack",

        requiredSubjectType: typeof(Army),
        requiredTargetType: typeof(Army),

        execution: (info) => {
            if(!info.subject.IsAdjacentTo(info.target)) {
                return;
            }

            var direction = (info.target.Coordinates - info.subject.Coordinates).GetDirection();
            ((Army) info.subject).Attack(direction);
        }
    );

    public static readonly Order BUILD_SPADE = new Order(

        name: "Build Spade",

        requiredSubjectType: typeof(Cell),

        execution: (info) => {
            var cell = (Cell) info.subject;
            if(cell.Nation.Size > cell.Nation.GetArmyAmount()) {
                cell.ScheduleArmy(Suit.SPADE);
            }
        }
    );

    public static readonly Order BUILD_HEART = new Order(

        name: "Build Heart",

        requiredSubjectType: typeof(Cell),

        execution: (info) => {
            var cell = (Cell) info.subject;
            if(cell.Nation.Size < cell.Nation.GetArmyAmount()) {
                cell.ScheduleArmy(Suit.HEART);
            }
        }
    );

    public static readonly Order BUILD_CLUB = new Order(

        name: "Build Club",

        requiredSubjectType: typeof(Cell),

        execution: (info) => {
            var cell = (Cell) info.subject;
            if(cell.Nation.Size < cell.Nation.GetArmyAmount()) {
                cell.ScheduleArmy(Suit.CLUB);
            }
        }
    );

    public static readonly Order BUILD_DIAMOND = new Order(

        name: "Build Diamond",

        requiredSubjectType: typeof(Cell),

        execution: (info) => {
            var cell = (Cell) info.subject;
            if(cell.Nation.Size < cell.Nation.GetArmyAmount()) {
                cell.ScheduleArmy(Suit.DIAMOND);
            }
        }
    );
}
