using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BoardManager : MonoBehaviour {
    
    #region Singleton
    public static BoardManager Instance {
        get; private set;
    }

    private void InitSingleton() {
        if(BoardManager.Instance != null) {
            throw new UnityException("Hay más de una instancia de BoardManager.");
        } else {
            BoardManager.Instance = this;
        }
    }
    #endregion

    public Cell cellPrefab;
    public int width;
    public int height;

    private Dictionary<Vector2Int, Cell> cells;

    #region Unity
    void Awake() {
        this.InitSingleton();
        this.RegisterCells();
    }
    #endregion

    public void RegisterCells() {
        this.cells = new Dictionary<Vector2Int, Cell>();
        foreach(Transform child in this.transform) {
            var cell = child.GetComponent<Cell>();
            if(cell != null) {
                this.cells.Add(cell.Coordinates, cell);
            }
        }
    }

    public Cell GetCell(Vector2Int coordinates) {
        var boundCoordinates = new Vector2Int(this.Modulo(coordinates.x, this.width), this.Modulo(coordinates.y, this.height));
        return this.cells[boundCoordinates];
    }

    public Bounds GetWorldBounds() {
        return new Bounds(this.transform.position, new Vector3(width, 1, height));
    }

    private int Modulo(int a, int b) {
        if(a >= 0) {
            return a % b;
        } else {
            var i = 0;
            while(i > a) {
                i -= b;
            }
            return a - i;
        }
    }

    #region Manipulate through Unity Editor
    [ContextMenu("Create Board Cells")]
    public void CreateBoardCells() {

        this.RemoveChildren();

        for(var x = 0; x < this.width; x++) {
            for(var y = 0; y < this.height; y++) {
                var newCell = GameObject.Instantiate(cellPrefab, new Vector3(x - (this.width - 1) * 0.5f, this.transform.position.y, y - (this.height - 1) * 0.5f), Quaternion.identity);
                newCell.transform.parent = this.transform;
                newCell.InitPosition(new Vector2Int(x, y));
            }
        }
    }

    private void RemoveChildren() {
        #if UNITY_EDITOR
        if(EditorApplication.isPlaying) {
            foreach(Transform child in this.transform) {
                GameObject.Destroy(child.gameObject);
            }
        } else {
        #endif
            var children = new GameObject[this.transform.childCount];
            for(var i = 0; i < this.transform.childCount; i++) {
                children[i] = this.transform.GetChild(i).gameObject;
            }
            foreach(GameObject child in children) {
                DestroyImmediate(child);
            }
        #if UNITY_EDITOR
        }
        #endif
    }
    #endregion
}
