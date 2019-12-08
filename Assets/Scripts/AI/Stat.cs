using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat {

    private System.Func<Architect, int> readCallback;

    private Dictionary<Architect, int> cachedValues;

    public Stat(System.Func<Architect, int> read) {
        this.readCallback = read;
        this.cachedValues = new Dictionary<Architect, int>();
        Stats.RegisterStat(this);
        GameManager.Simulation.onNextTurn += this.OnNextTurn;
    }

    public int Read(Architect architect) {
        int ret;
        if(this.cachedValues.ContainsKey(architect)) {
            ret = this.cachedValues[architect];
        } else {
            ret = this.readCallback.Invoke(architect);
            this.cachedValues[architect] = ret;
        }
        return ret;
    }

    private void OnNextTurn(int turn) {
        this.cachedValues.Clear();
    }
}

public static class Stats {

    public static readonly Stat SIZE = new Stat(
        read: (Architect architect) => {
            return architect.Nation.Size;
        }
    );

    public static readonly Stat FULL_ARMY_AMOUNT = new Stat(
        read: (Architect architect) => {
            return architect.GetArmyAmount();
        }
    );

    public static readonly Stat SPADE_AMOUNT = new Stat(
        read: (Architect architect) => {
            return architect.GetArmyAmount(Suit.SPADE);
        }
    );

    public static readonly Stat HEART_AMOUNT = new Stat(
        read: (Architect architect) => {
            return architect.GetArmyAmount(Suit.HEART);
        }
    );

    public static readonly Stat CLUB_AMOUNT = new Stat(
        read: (Architect architect) => {
            return architect.GetArmyAmount(Suit.CLUB);
        }
    );

    public static readonly Stat DIAMOND_AMOUNT = new Stat(
        read: (Architect architect) => {
            return architect.GetArmyAmount(Suit.DIAMOND);
        }
    );

    public static Stat AmountOf(Suit suit) {
        Stat ret = null;
        switch(suit) {
            case Suit.SPADE:
                ret = Stats.SPADE_AMOUNT;
                break;
            case Suit.HEART:
                ret = Stats.HEART_AMOUNT;
                break;
            case Suit.CLUB:
                ret = Stats.CLUB_AMOUNT;
                break;
            case Suit.DIAMOND:
                ret = Stats.DIAMOND_AMOUNT;
                break;
        }
        return ret;
    }

    #region Allow iteration through stats
    private static List<Stat> stats;
    public static void RegisterStat(Stat stat) {
        if(Stats.stats == null) {
            Stats.stats = new List<Stat>();
        }
        Stats.stats.Add(stat);
    }

    public static List<Stat> GetAll() {
        return Stats.stats;
    }
    #endregion
}
