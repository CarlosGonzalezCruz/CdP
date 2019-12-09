using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Struct con el que pasaremos información concreta a las órdenes
public struct OrderInfo {
    public Actionable subject;
    public Actionable target;

    public OrderInfo(Actionable subject = null, Actionable target = null) {
        this.subject = subject;
        this.target = target;
    }
}

// Las órdenes permiten que los Actuables cambien el estado del mundo
public class Order {

    public delegate void OrderExecution(OrderInfo info);

    public string name;

    // Requisitos de tipos de participantes. Los participantes de otros tipos no se permiten
    private System.Type subjectType;
    private System.Type targetType;

    // Función a ejecutar cuando la orden se ejecute
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


    // Orden de moverse hacia otro actuable
    public static readonly Order MOVE_TOWARDS = new Order(

        name: "Move towards",

        requiredSubjectType: typeof(Army),

        execution: (info) => {
            
            // El objetivo no es ir directamente hacia el otro actuable, sino acercarse a él dando un paso
            // en la dirección que menos distancia requiera recorrer

            var cell = info.subject.CurrentCell;
            var minimumDistance = int.MaxValue;

            // Exploramos todas las direcciones
            Direction chosenDirection = GameManager.Instance.allowedMovement.GetDefaultDirection();
            foreach(var direction in GameManager.Instance.allowedMovement.GetAllowedDirections()) {

                var adjacentCell = cell.GetNeighbour(direction);
                // Si no hay casilla en esta dirección, no hay nada que explorar
                if(adjacentCell == null) {
                    continue;
                }
                // Si hay una casilla pero está ocupada por un ejército enemigo, no hay nada que hacer
                if(adjacentCell != null && adjacentCell.Army != null && adjacentCell.Army.Nation != ((Army) info.subject).Nation) {
                    continue;
                }
                
                // Buscamos la dirección con la distancia más corta
                var distance = adjacentCell.RequestDistanceFrom(info.target);
                if(distance < minimumDistance) {
                    minimumDistance = distance;
                    chosenDirection = direction;
                }
            }

            // Si la casilla más cercana al objetivo que hemos encontrado está más lejos que la casilla en la que ya estamos,
            // no nos movemos

            if(cell.RequestDistanceFrom(info.target) > minimumDistance) {
                // Si el ejército que hay en la siguiente casilla es del mismo bando, los juntamos
                var possibleAlly = cell.GetNeighbour(chosenDirection)?.Army;
                if(possibleAlly != null && possibleAlly.Nation == ((Army) info.subject).Nation) {
                    ((Army) info.subject).Join(chosenDirection);

                // Si es un enemigo, lo atacamos
                } else if(possibleAlly != null) {
                    ((Army) info.subject).Attack(chosenDirection);

                // En otro caso, realizamos el movimiento
                } else {
                    ((Army) info.subject).Move(chosenDirection);
                }
            }
        }
    );

    // Orden de reclamar una casilla
    public static readonly Order CLAIM = new Order(

        name: "Claim",

        requiredSubjectType: typeof(Army),

        execution: (info) => {
            ((Army) info.subject).Claim();
        }
    );

    // Orden de atacar un objetivo
    public static readonly Order ATTACK = new Order(

        name: "Attack",

        requiredSubjectType: typeof(Army),
        requiredTargetType: typeof(Army),

        execution: (info) => {
            // Sólo podemos atacar al objetivo si es adyacente
            if(!info.subject.IsAdjacentTo(info.target)) {
                return;
            }

            var direction = (info.target.Coordinates - info.subject.Coordinates).GetDirection();
            ((Army) info.subject).Attack(direction);
        }
    );

    // Orden para casillas de construir un ejército de tipo Pica
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


    // Orden para casillas de construir un ejército de tipo Corazón
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

    // Orden para casillas de construir un ejército de tipo Trébol
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

    // Orden para casillas de construir un ejército de tipo Diamante
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
