using UnityEngine;
using System.Collections.Generic;

// Indica todas las direcciones disponibles para el juego y las asocia a vectores que pueden usarse en cálculos de posición
// El ángulo se utiliza para rotaciones en efectos visuales de los ejércitos

public enum Direction {
    [DirectionInfo(0, 0, 0)]
    NONE,

    [DirectionInfo(0, 1, 180)]
    UP,

    [DirectionInfo(0, -1, 0)]
    DOWN,

    [DirectionInfo(-1, 0, 90)]
    LEFT,

    [DirectionInfo(1, 0, -90)]
    RIGHT,

    [DirectionInfo(-1, 1, 135)]
    UPLEFT,

    [DirectionInfo(1, 1, -135)]
    UPRIGHT,

    [DirectionInfo(-1, -1, 45)]
    DOWNLEFT,

    [DirectionInfo(1, -1, -45)]
    DOWNRIGHT
}

public sealed class DirectionInfoAttribute : System.Attribute {

    public readonly Vector2Int vector;

    public readonly float angle;

    public DirectionInfoAttribute(int x, int y, float angle) {
        this.vector = new Vector2Int(x, y);
        this.angle = angle;
    }
}

public static class DirectionExtensionMethods {

    #region Cache attributes
    private static Dictionary<Direction, DirectionInfoAttribute> cachedAttributes = new Dictionary<Direction, DirectionInfoAttribute>();

    private static DirectionInfoAttribute Get(Direction direction) {
        if(!cachedAttributes.ContainsKey(direction)) {
            var dirKey = System.Enum.GetName(typeof(Direction), direction);
            var dirInfo = typeof(Direction).GetField(dirKey).GetCustomAttributes(false)[0] as DirectionInfoAttribute;
            cachedAttributes[direction] = dirInfo;
        }

        return cachedAttributes[direction];
    }
    #endregion

    public static Vector2Int GetVector(this Direction direction) {
        return DirectionExtensionMethods.Get(direction).vector;
    }

    public static float GetAngle(this Direction direction) {
        return DirectionExtensionMethods.Get(direction).angle;
    }

    public static Direction GetDirection(this Vector2Int vector) {
        Direction ret = default(Direction);
        
        foreach(Direction dir in System.Enum.GetValues(typeof(Direction))) {
            var dirInfo = DirectionExtensionMethods.Get(dir);

            if(dirInfo.vector.x > 0 == vector.x > 0 && dirInfo.vector.y > 0 == vector.y > 0
            && dirInfo.vector.x < 0 == vector.x < 0 && dirInfo.vector.y < 0 == vector.y < 0) {
                ret = dir;
                break;
            }
        }

        return ret;
    }
}