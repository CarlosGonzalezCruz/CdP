using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Se encarga de mostrar una ficha de ejército correctamente, con su respectivo color, marca de suit y contador de tropas

public class PieceRenderer : MonoBehaviour {

    public GameObject troopCounterPrefab;

    private new Renderer renderer;

    private Suit suit;

    private Color color;

    private Transform suitSymbols;

    private Canvas canvas;

    private TroopCounter counter;

    #region Unity
    void Awake() {
        var piece = this.transform.Find("Piece");
        this.renderer = piece.GetComponent<MeshRenderer>();
        this.suitSymbols = this.transform.Find("Suit");
        this.counter = this.CreateCounter();
        this.ApplySymbol();
    }

    #endregion

    #region Accessors
    public Color Color {
        get {
            return this.color;
        }
        set {
            if (this.color != value)
            {
                this.color = value;
                this.ApplyColor(value);
            }
        }
    }

    public Suit Suit {
        get {
            return this.suit;
        }
        set {
            if(this.suit != value) {
                this.suit = value;
                this.ApplySymbol();
            }           
        }
    }

    public Vector3 Corner {
        get {
            var ret = new Vector3(this.renderer.bounds.max.x, this.renderer.bounds.min.y, this.renderer.bounds.min.z);
            return ret;
        }
    }

    public int Number {
        get {
            return this.counter.Number;
        }
        set {
            this.counter.Number = value;
        }
    }
    #endregion

    public void Dispose() {
        GameObject.Destroy(this.counter.gameObject);
    }

    private void ApplyColor(Color color) {

        // Usamos un MaterialPropertyBlock para poder modificar cómo se funciona el renderer de este objeto en particular
        // sin modificar el material que usa (y por tantos sin afectar a otros objetos)

        var block = new MaterialPropertyBlock();
        if(this.renderer == null) {
            return;
        }
        this.renderer.GetPropertyBlock(block);
        block.SetColor("_Color", color);
        this.renderer.SetPropertyBlock(block);
    }

    private void ApplySymbol() {
        var suitChild = this.transform.Find("Suit");
        var suitName = suit.GetName();

        // Ocultamos todas las marcas de suit excepto la que corresponda al suit que realmente tiene el ejército

        foreach (Transform child in suitChild) {
            child.gameObject.SetActive(child.name == suitName);
        }

        suitChild.gameObject.SetActive(true);
    }

    private TroopCounter CreateCounter() {
        var counter = GameObject.Instantiate(troopCounterPrefab);
        counter.transform.SetParent(GameManager.Instance.canvas.transform);
        var component = counter.GetComponent<TroopCounter>();
        if(component == null) {
            throw new System.Exception("El prefab referenciado no tiene un componente TroopCounter");
        }
        component.piece = this;
        return component;
    }
}
