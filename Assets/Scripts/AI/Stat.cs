using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Los stats permiten a los arquitectos consultar aspectos sobre el estado del mundo
public class Stat {

    // Función a ejecutar cuando queramos leer un stat
    private System.Func<Architect, int> readCallback;

    // Los valores no cambian dentro de un mismo turno, así que se pueden cachear en caso de que
    // se consulte varias veces el mismo stat sobre el mismo arquitecto
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

    // Número de casillas conquistadas
    public static readonly Stat SIZE = new Stat(
        read: (Architect architect) => {
            return architect.Nation.Size;
        }
    );

    // Número de tropas total entre todos los ejércitos
    public static readonly Stat FULL_ARMY_AMOUNT = new Stat(
        read: (Architect architect) => {
            return architect.GetArmyAmount();
        }
    );


    // Número de tropas entre los ejércitos de tipo Pica
    public static readonly Stat SPADE_AMOUNT = new Stat(
        read: (Architect architect) => {
            return architect.GetArmyAmount(Suit.SPADE);
        }
    );

    // Número de tropas entre los ejércitos de tipo Corazón
    public static readonly Stat HEART_AMOUNT = new Stat(
        read: (Architect architect) => {
            return architect.GetArmyAmount(Suit.HEART);
        }
    );

    // Número de tropas entre los ejércitos de tipo Trébol
    public static readonly Stat CLUB_AMOUNT = new Stat(
        read: (Architect architect) => {
            return architect.GetArmyAmount(Suit.CLUB);
        }
    );

    // Número de tropas entre los ejércitos de tipo Diamante
    public static readonly Stat DIAMOND_AMOUNT = new Stat(
        read: (Architect architect) => {
            return architect.GetArmyAmount(Suit.DIAMOND);
        }
    );

    // Devuelve el stat asociado a un suit concreto
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

    #region Permitir iterar stats
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
