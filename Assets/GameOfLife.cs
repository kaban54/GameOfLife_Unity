using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;

public class GameOfLife : MonoBehaviour
{
    public static GameOfLife gameRef;
    public Vector2Int boardSize;
    public GameObject cellPrefab;
    public float iterTime;
    public float minIterTime;
    public float maxIterTime;
    private SpriteRenderer[,] spriteBoard;
    private GameObject[,] objBoard;
    private bool[,] board;
    private bool paused;
    private float timeAcc;
    public GameObject gridElement;
    public GameObject bg;
    private GameObject[] verticalGrid;
    private GameObject[] horizontalGrid;
    public bool gridEnabled;
    public float randCoef;
    // Start is called before the first frame update
    void Start() {
        gameRef = this;
        spriteBoard = new SpriteRenderer[boardSize.x, boardSize.y];
        objBoard = new GameObject[boardSize.x, boardSize.y];
        verticalGrid = new GameObject[boardSize.x + 1];
        horizontalGrid = new GameObject[boardSize.y + 1];
        board = new bool[boardSize.x, boardSize.y];
        paused = true;

        for (int i = 0; i < boardSize.x; i++) {
            for (int j = 0; j < boardSize.y; j++) {
                var position = new Vector3(i - boardSize.x / 2f + 0.5f, j - boardSize.y / 2f + 0.5f, 0);
                objBoard[i, j] = Instantiate(cellPrefab, position, Quaternion.identity);
                spriteBoard[i, j] = objBoard[i, j].GetComponent<SpriteRenderer>();
                board[i, j] = false;
                spriteBoard[i, j].enabled = false;
                var cell = objBoard[i, j].GetComponent<Cell>();
                cell.pos = new Vector2Int(i, j);
            }
        }
        bg.GetComponent<Transform>().localScale = new Vector3(boardSize.x, boardSize.y, 1);
        Instantiate(bg, new Vector3(0, 0, 0), Quaternion.identity);
        gridElement.GetComponent<Transform>().localScale = new Vector3(0.05f, boardSize.y, 1);
        for (int i = 0; i <= boardSize.x; i++) {
            verticalGrid[i] = Instantiate(gridElement, new Vector3(i - boardSize.x / 2, 0, 0), Quaternion.identity);
        }
        gridElement.GetComponent<Transform>().localScale = new Vector3(boardSize.x, 0.05f, 1);
        for (int i = 0; i <= boardSize.x; i++) {
            horizontalGrid[i] = Instantiate(gridElement, new Vector3(0, i - boardSize.x / 2, 0), Quaternion.identity);
        }
    }

    private bool IsOnBoard(int x, int y) {
        return !(x < 0 || y < 0 || x >= boardSize.x || y >= boardSize.y);
    }

    private int CountNeighbors (int x, int y) {
        var ret = 0;
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                if (i == 0 && j == 0) continue;
                if (IsOnBoard(x + i, y + j)) {
                    if (board[x + i, y + j]) ret++;
                }
            }
        }

        return ret;
    }

    private void IterateGame() {
        timeAcc += Time.deltaTime;

        while(timeAcc > iterTime) {
            timeAcc -= iterTime;
            var nextboard = (bool[,]) board.Clone();
            for (int i = 0; i < boardSize.x; i++) {
                for (int j = 0; j < boardSize.y; j++) {
                    var neigh = CountNeighbors(i, j);
                    
                    if (board[i, j]) {
                        if (neigh < 2 || neigh > 3) nextboard[i, j] = false;
                        else nextboard[i, j] = true;
                    }
                    else {
                        if (neigh == 3) nextboard[i, j] = true;
                        else nextboard[i, j] = false;
                    }
                }
            }

            board = (bool[, ]) nextboard.Clone();
        }
        for (int i = 0; i < boardSize.x; i++) {
            for (int j = 0; j < boardSize.y; j++) {
                spriteBoard[i, j].enabled = board[i, j];
            }
        }
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) paused = !paused;
        if (Input.GetKeyDown(KeyCode.Z)) iterTime = Math.Min(maxIterTime, iterTime * 2f);
        if (Input.GetKeyDown(KeyCode.X)) iterTime = Math.Max(minIterTime, iterTime / 2f);
        if (Input.GetKeyDown(KeyCode.R)) GenerateRandom();

        if (!paused) IterateGame();
    }

    public void ClickCell(int x, int y) {
        board[x, y] = !board[x, y];
        spriteBoard[x, y].enabled = board[x, y];
    }

    public void EnableGrid() {
        gridEnabled = true;
        for (int i = 0; i <= boardSize.x; i++) {
            verticalGrid[i].GetComponent<SpriteRenderer>().enabled = gridEnabled;
        }
        for (int i = 0; i <= boardSize.y; i++) {
            horizontalGrid[i].GetComponent<SpriteRenderer>().enabled = gridEnabled;
        }
    }

    public void DisableGrid() {
        gridEnabled = false;
        for (int i = 0; i <= boardSize.x; i++) {
            verticalGrid[i].GetComponent<SpriteRenderer>().enabled = gridEnabled;
        }
        for (int i = 0; i <= boardSize.y; i++) {
            horizontalGrid[i].GetComponent<SpriteRenderer>().enabled = gridEnabled;
        }
    }

    private void GenerateRandom() {
        for (int i = 0; i < boardSize.x; i++) {
            for (int j = 0; j < boardSize.y; j++) {
                board[i, j] = false;
                spriteBoard[i, j].enabled = false;
            }
        }

        var n = (int)(boardSize.x * boardSize.y * randCoef);
        var rand = new System.Random();
        while (n > 0) {
            var x = rand.Next(boardSize.x);
            var y = rand.Next(boardSize.y);
            if (!board[x, y]) {
                board[x, y] = true;
                spriteBoard[x, y].enabled = true;
                n--;
            }
        }
    }
}

