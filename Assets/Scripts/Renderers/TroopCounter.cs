using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Muestra en la UI el número de tropas del ejército asociado a este contador

[RequireComponent(typeof(CanvasGroup))]
public class TroopCounter : MonoBehaviour {

    public PieceRenderer piece;

    public float maxRenderDistance;

    private CanvasGroup canvasGroup;

    private Text numberText;

    #region Unity
    private void Awake() {
        this.canvasGroup = this.GetComponent<CanvasGroup>();
        this.numberText = this.GetComponentInChildren<Text>();
        this.Hidden = true;
    }
    
    private void Update() {
        var distance = Vector3.Distance(Camera.main.transform.position, this.piece.transform.position);
        this.Hidden = distance > maxRenderDistance;

        this.transform.position = Camera.main.WorldToScreenPoint(this.piece.Corner);
    }
    #endregion

    #region Accessors
    public bool Hidden {
        get {
            return this.canvasGroup.alpha == 0;
        }
        set {
            this.canvasGroup.alpha = !value ? 1 : 0;
            this.canvasGroup.interactable = !value;
            this.canvasGroup.blocksRaycasts = !value;
        }
    }

    public int Number {
        get {
            return int.Parse(this.numberText.text);
        }
        set {
            this.numberText.text = value.ToString();
        }
    }
    #endregion
}
