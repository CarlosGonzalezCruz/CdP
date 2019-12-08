using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class CellRenderer : MonoBehaviour {

    private Color color;

    private new Renderer renderer;

    #region Unity
    void Start() {
        this.renderer = GetComponent<Renderer>();
    }
    #endregion

    #region Accessors
    public float RenderedHeight {
        get {
            return this.transform.localScale.y;
        }
        set {
            var currentScale = this.transform.localScale;
            this.transform.localScale = new Vector3(currentScale.x, value, currentScale.z);
        }
    }

    public Color Color {
        get {
            return this.color;
        }
        set {
            if(this.color != value) {
                this.color = value;
                this.ApplyColor(value);
            }  
        }
    }
    #endregion

    private void ApplyColor(Color color) {
        var block = new MaterialPropertyBlock();
        this.renderer.GetPropertyBlock(block);
        block.SetColor("_Color", color);
        this.renderer.SetPropertyBlock(block);
    }
}
