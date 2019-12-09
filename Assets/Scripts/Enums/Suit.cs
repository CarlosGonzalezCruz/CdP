using System.Collections.Generic;

// Indica los suits de los que pueden formar parte los ejércitos. En los atributos del enumerado se especifica contra
// qué suit es débil el suit en cuestión

public enum Suit {
    [SuitInfo("Spade", HEART)]
    SPADE,
    
    [SuitInfo("Heart", CLUB)]
    HEART,
    
    [SuitInfo("Club", DIAMOND)]
    CLUB,
    
    [SuitInfo("Diamond", SPADE)]
    DIAMOND
}

public sealed class SuitInfoAttribute : System.Attribute {
    public readonly string name;
    public readonly Suit weakness;

    public SuitInfoAttribute(string name, Suit weakness) {
        this.name = name;
        this.weakness = weakness;
    }
} 

public static class SuitExtensionMethods {

    #region Cache Attributes
    private static Dictionary<Suit, SuitInfoAttribute> cachedAttributes = new Dictionary<Suit, SuitInfoAttribute>();

    private static SuitInfoAttribute Get(Suit suit) {
        if(!cachedAttributes.ContainsKey(suit)) {
            var suitKey = System.Enum.GetName(typeof(Suit), suit);
            var suitInfo = typeof(Suit).GetField(suitKey).GetCustomAttributes(false)[0] as SuitInfoAttribute;
            cachedAttributes[suit] = suitInfo;
        }

        return cachedAttributes[suit];
    }
    #endregion

    public static string GetName(this Suit suit) {
        return SuitExtensionMethods.Get(suit).name;
    }

    public static Suit GetWeakness(this Suit suit) {
        return SuitExtensionMethods.Get(suit).weakness;
    }

    public static bool IsWeakAgainst(this Suit suit, Suit other) {
        return SuitExtensionMethods.Get(suit).weakness == other;
    }
}