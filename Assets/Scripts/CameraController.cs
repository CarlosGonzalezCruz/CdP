using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CameraController : MonoBehaviour {

    public float dragFactor = 1f;

    public float scrollFactor = 1f;
    
    public float minHeight = 2f;

    public float maxHeight = 10f;

    private Vector2 lastMousePosition;

    #region Unity
    private void FixedUpdate() {

        var positionChanged = false;

        positionChanged = positionChanged || this.ApplyMouseDrag();
        
        positionChanged = positionChanged || this.ApplyMouseScroll();

        if(positionChanged) {
            this.ApplyBoundaries();
        } 
    }
    #endregion

    #region Apply navigation
    private bool ApplyMouseDrag() {
        var positionChanged = false;

        if(this.IsMouseOutsideScreen()) {
            return positionChanged;
        }

        var currentMousePosition = this.NormalizeMousePosition(Input.mousePosition, -1, 1);

        if(Input.GetButton("Drag")) {
            var offset = this.lastMousePosition - currentMousePosition;
            var translation = new Vector3(offset.x, 0, offset.y);
            this.transform.Translate(translation * this.dragFactor * this.transform.position.y, Space.World);
            positionChanged = true;
        }
            
        this.lastMousePosition = currentMousePosition;
 
        return positionChanged;
    }

    private bool ApplyMouseScroll() {
        var positionChanged = false;

        if(this.IsMouseOutsideScreen()) {
            return positionChanged;
        }

        if(Input.GetAxis("Scroll") > 0 && this.transform.position.y > this.minHeight || Input.GetAxis("Scroll") < 0 && this.transform.position.y < this.maxHeight) {
            this.transform.Translate(0, 0, Input.GetAxis("Scroll") * this.scrollFactor, Space.Self);
            positionChanged = true;
        }

        return positionChanged;
    }

    private void ApplyBoundaries() {
        var bounds = BoardManager.Instance.GetWorldBounds();
        this.transform.position = new Vector3(
            Mathf.Clamp(this.transform.position.x, bounds.min.x, bounds.max.x),
            Mathf.Clamp(this.transform.position.y, this.minHeight, this.maxHeight),
            Mathf.Clamp(this.transform.position.z, bounds.min.z - this.transform.position.y * 0.5f, bounds.max.z - this.transform.position.y)
        );
    }
    #endregion

    #region Auxiliar methods
    private Vector2 NormalizeMousePosition(Vector2 mousePosition, float min = 0, float max = 1) {
        var ret = mousePosition;
        ret.Scale(new Vector2(1f / Screen.width, 1f / Screen.height));
        ret *= (max - min);
        ret += min * Vector2.one;
        return ret;
    }

    private bool IsMouseOutsideScreen() {
        var ret = Input.mousePosition.x < 0 || Input.mousePosition.y < 0;
        #if UNITY_EDITOR
            ret = ret || Input.mousePosition.x > Handles.GetMainGameViewSize().x || Input.mousePosition.y > Handles.GetMainGameViewSize().y;
        #else
            ret = ret || Input.mousePosition.x > Screen.width || Input.mousePosition.y > Screen.height;
        #endif
        return ret;
    }
    #endregion
}
