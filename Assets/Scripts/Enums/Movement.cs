using System.Linq;
using System.Collections.Generic;
using UnityEngine;

// Indica el conjunto de direcciones que podrán utilizar los actuables

public enum Movement {
    [MovementInfo(Direction.UP, Direction.DOWN, Direction.LEFT, Direction.RIGHT)]
    ORTHOGONAL,

    //// [MovementInfo(Direction.UPLEFT, Direction.UPRIGHT, Direction.DOWNLEFT, Direction.DOWNRIGHT)]
    //// DIAGONAL,

    [MovementInfo(Direction.UP, Direction.DOWN, Direction.LEFT, Direction.RIGHT, Direction.UPLEFT, Direction.UPRIGHT, Direction.DOWNLEFT, Direction.DOWNRIGHT)]
    FULL
}

public sealed class MovementInfoAttribute : System.Attribute {

    public readonly Direction[] allowedDirections;

    public MovementInfoAttribute(params Direction[] directions) {
        this.allowedDirections = directions;
    }
}

public static class MovementExtensionMethods {

    #region Cache Attributes
    private static Dictionary<Movement, MovementInfoAttribute> cachedAttributes = new Dictionary<Movement, MovementInfoAttribute>();

    private static MovementInfoAttribute Get(Movement movement) {
        if(!cachedAttributes.ContainsKey(movement)) {
            var movKey = System.Enum.GetName(typeof(Movement), movement);
            var movInfo = typeof(Movement).GetField(movKey).GetCustomAttributes(false)[0] as MovementInfoAttribute;
            cachedAttributes[movement] = movInfo;
        }

        return cachedAttributes[movement];
    }
    #endregion

    public static Direction[] GetAllowedDirections(this Movement movement) {
        return MovementExtensionMethods.Get(movement).allowedDirections;
    }

    public static Direction GetDefaultDirection(this Movement movement) {
        return MovementExtensionMethods.Get(movement).allowedDirections[0];
    }

    public static bool Allows(this Movement movement, Direction direction) {
        return MovementExtensionMethods.Get(movement).allowedDirections.Contains(direction);
    }

    public static System.Func<Vector2Int, Vector2Int, int> GetHeuristics(this Movement movement) {
        System.Func<Vector2Int, Vector2Int, int> ret = null;
        switch(movement) {
            case Movement.ORTHOGONAL:
                ret = MovementExtensionMethods.OrthogonalHeuristics;
                break;
            //// case Movement.DIAGONAL:
            ////    ret = MovementExtensionMethods.DiagonalHeuristics;
            ////    break;
            case Movement.FULL:
                ret = MovementExtensionMethods.FullHeuristics;
                break;
            default:
                throw new System.Exception("El tipo de movimiento indicado no tiene una función heurística asociada.");
        }
        return ret;
    }

    #region Heuristics
    private static int OrthogonalHeuristics(Vector2Int a, Vector2Int b) {
        
        // La heurística usada en ortogonal es la distancia Manhattan

        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private static int DiagonalHeuristics(Vector2Int a, Vector2Int b) {
        
        // La heurística usada en diagonal se basa en la distancia Chebyshev, con la limitación de que los elementos
        // que estén en diagonales incompatibles son mutuamente inalcanzables y no pueden interactuar

        var xDistance = Mathf.Abs(a.x - b.x);
        var yDistance = Mathf.Abs(a.y - b.y);
        if(xDistance % 2 == yDistance % 2) {
            return Mathf.Max(xDistance, yDistance);
        } else {
            return int.MaxValue;
        }
    }

    private static int FullHeuristics(Vector2Int a, Vector2Int b) {
        
        // La heurística usada cuando todas las direcciones están disponibles es la distancia Chebyshev

        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }
    #endregion
}