using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    private int[,] board;
    private bool paused;
    private float timeAcc;
    public GameObject gridElement;
    public GameObject bg;
    private GameObject[] verticalGrid;
    private GameObject[] horizontalGrid;
    public bool gridEnabled;
    public int mode;
    private int curTeam;
    private Color[] colors;
    public bool clickable;
    public Canvas score;
    public int[] scoreCount;
    // Start is called before the first frame update
    void Start() {
        gameRef = this;
        spriteBoard = new SpriteRenderer[boardSize.x, boardSize.y];
        objBoard = new GameObject[boardSize.x, boardSize.y];
        verticalGrid = new GameObject[boardSize.x + 1];
        horizontalGrid = new GameObject[boardSize.y + 1];
        board = new int[boardSize.x, boardSize.y];
        paused = true;
        curTeam = 1;
        colors = new Color[3];
        colors[0] = new Color(255, 255, 255);
        colors[1] = new Color(255, 0, 0);
        colors[2] = new Color(0, 0, 255);
        clickable = true;
        mode = 0;
        scoreCount = new int[3];
        scoreCount[1] = 0;
        scoreCount[2] = 0;

        for (int i = 0; i < boardSize.x; i++) {
            for (int j = 0; j < boardSize.y; j++) {
                var position = new Vector3(i - boardSize.x / 2f + 0.5f, j - boardSize.y / 2f + 0.5f, 0);
                objBoard[i, j] = Instantiate(cellPrefab, position, Quaternion.identity);
                spriteBoard[i, j] = objBoard[i, j].GetComponent<SpriteRenderer>();
                board[i, j] = 0;
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
        gridEnabled = true;
        DisableGrid();
        GenerateRandom();
    }

    private bool IsOnBoard(int x, int y) {
        return !(x < 0 || y < 0 || x >= boardSize.x || y >= boardSize.y);
    }

    private Vector2Int CountNeighbors (int x, int y) {
        var ret = new Vector2Int(0, 0);
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                if (i == 0 && j == 0) continue;
                if (IsOnBoard(x + i, y + j)) {
                    if      (board[x + i, y + j] == 1) ret.x++;
                    else if (board[x + i, y + j] == 2) ret.y++;
                }
            }
        }

        return ret;
    }

    private void IterateGame() {
        timeAcc += Time.deltaTime;
        while(timeAcc > iterTime) {
            var count1 = 0;
            var count2 = 0;
            timeAcc -= iterTime;
            var nextboard = (int[,]) board.Clone();
            for (int i = 0; i < boardSize.x; i++) {
                for (int j = 0; j < boardSize.y; j++) {
                    if (mode == 0) {
                        var neigh = CountNeighbors(i, j).x;
                        
                        if (board[i, j] == 1) {
                            if (neigh < 2 || neigh > 3) nextboard[i, j] = 0;
                            else nextboard[i, j] = 1;
                        }
                        else {
                            if (neigh == 3) nextboard[i, j] = 1;
                            else nextboard[i, j] = 0;
                        }
                    }
                    else {
                        var neighVec = CountNeighbors(i, j);
                        var neigh = neighVec.x + neighVec.y;
                        
                        if (board[i, j] != 0) {
                            if (neigh < 2 || neigh > 3) nextboard[i, j] = 0;
                            else nextboard[i, j] = board[i, j];
                        }
                        else {
                            if (neigh == 3) {
                                if (neighVec.x >= 2) nextboard[i, j] = 1;
                                else                 nextboard[i, j] = 2;
                            }
                            else nextboard[i, j] = 0;
                        }
                    }
                    if      (nextboard[i, j] == 1) count1++;
                    else if (nextboard[i, j] == 2) count2++;
                }
            }
            scoreCount[1] = count1;
            scoreCount[2] = count2;
            board = (int[, ]) nextboard.Clone();
        }


        for (int i = 0; i < boardSize.x; i++) {
            for (int j = 0; j < boardSize.y; j++) {
                spriteBoard[i, j].enabled = (board[i, j] != 0);
            }
        }
        if (mode == 1) SetColors();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) paused = !paused;
        if (Input.GetKeyDown(KeyCode.Z)) iterTime = Math.Min(maxIterTime, iterTime * 2f);
        if (Input.GetKeyDown(KeyCode.X)) iterTime = Math.Max(minIterTime, iterTime / 2f);
        if (Input.GetKeyDown(KeyCode.R)) ClearBoard();
        if (Input.GetKeyDown(KeyCode.G)) GenerateRandom();
        if (Input.GetKeyDown(KeyCode.M)) SwitchMode();
        if (Input.GetKeyDown(KeyCode.T) && mode == 1) curTeam = 3 - curTeam;

        if (!paused) IterateGame();
    }

    public void ClickCell(int x, int y) {
        if (!clickable) return;
        if (mode == 0) {
            board[x, y] = 1 - board[x, y];
        }
        else {
            if (board[x, y] == curTeam) board[x, y] = 0;
            else                        board[x, y] = curTeam;
            if (board[x, y] != 0) spriteBoard[x, y].color = colors[board[x, y]];
        }
        spriteBoard[x, y].enabled = (board[x, y] != 0);
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

    public void ClearBoard() {
        for (int i = 0; i < boardSize.x; i++) {
            for (int j = 0; j < boardSize.y; j++) {
                board[i, j] = 0;
                spriteBoard[i, j].enabled = false;
                spriteBoard[i, j].color = colors[0];
            }
        }
    }

    public void GenerateRandom() {
        ClearBoard();

        var rand = new System.Random();
        var k = (float)rand.Next(5, 40) / 100f;
        var n = (int)(boardSize.x * boardSize.y * k);
        if (mode == 1) n -= n % 2;
        while (n > 0) {
            var x = rand.Next(boardSize.x);
            var y = rand.Next(boardSize.y);
            if (board[x, y] == 0) {
                if (mode == 0) board[x, y] = 1;
                else           board[x, y] = n % 2 + 1;
                spriteBoard[x, y].enabled = true;
                n--;
            }
        }
        if (mode == 1) SetColors();
    }

    private void SetColors() {
        for (int i = 0; i < boardSize.x; i++) {
            for (int j = 0; j < boardSize.y; j++) {
                if (board[i, j] == 0) continue;
                spriteBoard[i, j].color = colors[board[i, j]];
            }
        }
    }

    public void SwitchMode() {
        mode = 1 - mode;
        ClearBoard();
        if (mode == 1) score.enabled = true;
        else score.enabled = false;
    }
}

