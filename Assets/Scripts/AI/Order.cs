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

    private System.Type subjectType;

    private System.Type targetType;

    private OrderExecution execution;

    public Order(OrderExecution execution, System.Type requiredSubjectType = null, System.Type requiredTargetType = null) {
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
}

public static class Orders {
    
    public static readonly Order MOVE_TOWARDS = new Order(
        requiredSubjectType: typeof(Army),

        execution: (info) => {
            var cell = info.subject.CurrentCell;
            var minimumDistance = int.MaxValue;
            Direction chosenDirection = GameManager.Instance.allowedMovement.GetDefaultDirection();
            foreach(var direction in GameManager.Instance.allowedMovement.GetAllowedDirections()) {
                var adjacentCell = cell.GetNeighbour(direction);
                if(adjacentCell.Army != null) {
                    continue;
                }
                
                var distance = adjacentCell.RequestDistanceFrom(info.target);
                if(distance < minimumDistance) {
                    minimumDistance = distance;
                    chosenDirection = direction;
                }
            }
            if(cell.RequestDistanceFrom(info.target) > minimumDistance) {
                ((Army) info.subject).Move(chosenDirection);
            }
        }
    );

    public static readonly Order CLAIM = new Order(
        requiredSubjectType: typeof(Army),

        execution: (info) => {
            ((Army) info.subject).Claim();
        }
    );

    public static readonly Order ATTACK = new Order(
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
}
