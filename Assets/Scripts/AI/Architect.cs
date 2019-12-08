using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Nation))]
public class Architect : MonoBehaviour {

    private Nation nation;

    #region Unity
    private void Awake() {
        this.nation = this.GetComponent<Nation>();
    }
    #endregion

    #region Accessors
    public Nation Nation {
        get {
            return this.nation;
        }
    }
    #endregion

    public int GetArmyAmount() {
        return this.Nation.GetArmyAmount();
    }

    public int GetArmyAmount(Suit suit) {
        return this.Nation.GetArmyAmount(suit);
    }

    public Dictionary<Stat, int> GetState() {
        var ret = new Dictionary<Stat, int>();
        foreach(Stat stat in Stats.GetAll()) {
            ret.Add(stat, stat.Read(this));
        }
        return ret;
    }
}
