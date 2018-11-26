/*
 * Copyright (c) 2018 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * Notwithstanding the foregoing, you may not use, copy, modify, merge, publish, 
 * distribute, sublicense, create a derivative work, and/or sell copies of the 
 * Software in any work that is designed, intended, or marketed for pedagogical or 
 * instructional purposes related to programming, coding, application development, 
 * or information technology.  Permission for such use, copying, modification,
 * merger, publication, distribution, sublicensing, creation of derivative works, 
 * or sale is expressly withheld.
 *    
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Board board;

    public GameObject whiteKing;
    public GameObject whiteQueen;
    public GameObject whiteBishop;
    public GameObject whiteKnight;
    public GameObject whiteRook;
    public GameObject whitePawn;

    public GameObject blackKing;
    public GameObject blackQueen;
    public GameObject blackBishop;
    public GameObject blackKnight;
    public GameObject blackRook;
    public GameObject blackPawn;

    public Player currentPlayer;
    public Player otherPlayer;
    public bool isGameOver;//

    public TileSelector player1TileSelector;//
    public MoveSelector player1MoveSelector;//
    public TileSelector player2TileSelector;//
    public MoveSelector player2MoveSelector;//

    public GameObject attacker;//
    public GameObject defender;//

    private GameObject[,] pieces;
    private GameObject[,] survivalPieces;//
    private GameObject[,] chessPieces;//
    private Player white;
    private Player black;
    private GameObject p1CurrentlySelected;//
    private GameObject p2CurrentlySelected;//
    private GameStates gameState;//
    private Vector2Int survivalOrigin;//
    private Vector2Int attackerOrigin;
    private int survivalTimer = 10;

    public GameStates GameState {
        get {
            return gameState;
        }

        private set {
            gameState = value;
        }
    }//

    void Awake()
    {
        instance = this;
    }

    void Start ()
    {
        pieces = new GameObject[8, 8];
        survivalPieces = new GameObject[8, 8];//
        chessPieces = new GameObject[8, 8];//

        white = new Player("white", true);
        black = new Player("black", false);

        currentPlayer = white;
        otherPlayer = black;

        isGameOver = false;//
        player1TileSelector.SetPlayer(currentPlayer);//
        player1MoveSelector.SetPlayer(currentPlayer);//
        player2TileSelector.SetPlayer(otherPlayer);//
        player2MoveSelector.SetPlayer(otherPlayer);//

        InitialSetup();
    }

    private void InitialSetup()
    {
        GameState = GameStates.CHESS;//

        AddPiece(whiteRook, white, 0, 0);
        AddPiece(whiteKnight, white, 1, 0);
        AddPiece(whiteBishop, white, 2, 0);
        AddPiece(whiteQueen, white, 3, 0);
        AddPiece(whiteKing, white, 4, 0);
        AddPiece(whiteBishop, white, 5, 0);
        AddPiece(whiteKnight, white, 6, 0);
        AddPiece(whiteRook, white, 7, 0);

        for (int i = 0; i < 8; i++)
        {
            AddPiece(whitePawn, white, i, 1);
        }

        AddPiece(blackRook, black, 0, 7);
        AddPiece(blackKnight, black, 1, 7);
        AddPiece(blackBishop, black, 2, 7);
        AddPiece(blackQueen, black, 3, 7);
        AddPiece(blackKing, black, 4, 7);
        AddPiece(blackBishop, black, 5, 7);
        AddPiece(blackKnight, black, 6, 7);
        AddPiece(blackRook, black, 7, 7);

        for (int i = 0; i < 8; i++)
        {
            AddPiece(blackPawn, black, i, 6);
        }

        SetPieces();//
    }

    private void ToggleActivePieces()//
    {
        foreach(GameObject piece in pieces)
        {
            if(piece)
                piece.SetActive(!piece.activeSelf);
        }
    }

    public void AddPiece(GameObject prefab, Player player, int col, int row)
    {
        GameObject pieceObject = board.AddPiece(prefab, col, row);
        player.pieces.Add(pieceObject);
        chessPieces[col, row] = pieceObject;//
        Piece newPiece = pieceObject.GetComponent<Piece>();
        newPiece.owner = player;
    }

    public void SelectPieceAtGrid(Vector2Int gridPoint)
    {
        GameObject selectedPiece = pieces[gridPoint.x, gridPoint.y];
        if (selectedPiece)
        {
            board.SelectPiece(selectedPiece);
        }
    }

    public void SelectPiece(GameObject piece, int playerNumber)
    {
        if (playerNumber == 1)
        {
            if(gameState == GameStates.CHESS)
            {
                DeselectPiece(p2CurrentlySelected);
            }
            DeselectPiece(p1CurrentlySelected);
            
            p1CurrentlySelected = piece;//
        }
        else
        {
            if (gameState == GameStates.CHESS)
            {
                DeselectPiece(p1CurrentlySelected);
            }
            DeselectPiece(p2CurrentlySelected);

            p2CurrentlySelected = piece;//
        }

        board.SelectPiece(piece);
    }

    public void DeselectPiece(GameObject piece)
    {
        if (piece)
        {
            board.DeselectPiece(piece);
        }
    }

    public GameObject PieceAtGrid(Vector2Int gridPoint)
    {
        if (gridPoint.x > 7 || gridPoint.y > 7 || gridPoint.x < 0 || gridPoint.y < 0)
        {
            return null;
        }
        return pieces[gridPoint.x, gridPoint.y];
    }

    public Vector2Int GridForPiece(GameObject piece)
    {
        for (int i = 0; i < 8; i++) 
        {
            for (int j = 0; j < 8; j++)
            {
                if (pieces[i, j] == piece)
                {
                    return new Vector2Int(i, j);
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    public bool FriendlyPieceAt(Vector2Int gridPoint)
    {
        GameObject piece = PieceAtGrid(gridPoint);

        if (piece == null) {
            return false;
        }

        if (otherPlayer.pieces.Contains(piece))
        {
            return false;
        }

        return true;
    }

    public bool DoesPieceBelongToCurrentPlayer(GameObject piece, Player player)
    {
        return player.pieces.Contains(piece);
    }

    public void Move(GameObject piece, Vector2Int gridPoint)
    {
        Vector2Int startGridPoint = GridForPiece(piece);
        pieces[startGridPoint.x, startGridPoint.y] = null;
        pieces[gridPoint.x, gridPoint.y] = piece;
        if(GameState == GameStates.CHESS)
            chessPieces = pieces;
        board.MovePiece(piece, gridPoint);
    }

    public List<Vector2Int> MovesForPiece(GameObject pieceObject)
    {
        Piece piece = pieceObject.GetComponent<Piece>();
        Vector2Int gridPoint = GridForPiece(pieceObject);
        List<Vector2Int> locations = piece.MoveLocations(gridPoint);

        locations.RemoveAll(tile => tile.x < 0 || tile.x > 7
            || tile.y < 0 || tile.y > 7);

        if(gameState == GameStates.CHESS)//
            locations.RemoveAll(tile => FriendlyPieceAt(tile));

        return locations;
    }

    public void NextPlayer()
    {
        Player tempPlayer = currentPlayer;
        currentPlayer = otherPlayer;
        otherPlayer = tempPlayer;
    }

    public void CapturePieceAt(Vector2Int gridPoint)
    {
        GameObject pieceToCapture = PieceAtGrid(gridPoint);
        if(pieceToCapture.GetComponent<Piece>().type == PieceType.King)
        {
            isGameOver = true;
            Debug.Log(currentPlayer.name + " wins!");
        }

        currentPlayer.capturedPieces.Add(pieceToCapture);
        chessPieces[gridPoint.x, gridPoint.y] = null;
        
        Destroy(pieceToCapture);
    }

    public void InitiateSurvival(GameObject pieceToCapture)//
    {
        GameState = GameStates.SURVIVAL;
        survivalOrigin = Geometry.GridFromPoint(pieceToCapture.transform.position);
        chessPieces = pieces;
        survivalPieces = new GameObject[8,8];

        //Turn all chess pieces off
        ToggleActivePieces();

        defender = pieceToCapture;

        if(currentPlayer.playerNumber == 1)
        {
            attacker = p1CurrentlySelected;
        }
        else
        {
            attacker = p2CurrentlySelected;
        }

        SetSurvivalLocation(attacker, currentPlayer.playerNumber);
        SetSurvivalLocation(defender, otherPlayer.playerNumber);

        SetSurvivalPieces(attacker, defender);

        SetPieces();

        //Turn on only survival piecesS
        ToggleActivePieces();

        player1TileSelector.EnterState();
        player2TileSelector.EnterState();
        attackerOrigin = Geometry.GridFromPoint(attacker.transform.position);
        StartCoroutine(SurvivalCountDown());
    }

    private void SetSurvivalLocation(GameObject piece, int playerNumber)//
    {
        Vector2Int newPos = new Vector2Int(Random.Range(0, 7), (playerNumber - 1) * 7);
        piece.transform.position = Geometry.PointFromGrid(newPos);
    }

    public void InitiateAttackerWins(GameObject defender, GameObject attacker)//
    {
        GameState = GameStates.CHESS;

        CapturePieceAt(Geometry.GridFromPoint(defender.transform.position));

        ToggleActivePieces();

        SetPieces();

        ToggleActivePieces();

        Move(attacker, survivalOrigin);
    }

    public void InitiateDefenderWins()//
    {
        GameState = GameStates.CHESS;

        ToggleActivePieces();

        SetPieces();

        ToggleActivePieces();

        Move(attacker, attackerOrigin);
        Move(defender, survivalOrigin);
    }

    private void SetPieces()//
    {
        switch (gameState)
        {
            case GameStates.CHESS:
                pieces = chessPieces;
                break;
            case GameStates.SURVIVAL:
                pieces =  survivalPieces;
                break;
        }
    }

    private void SetSurvivalPieces(GameObject attacker, GameObject defender)//
    {
        Vector2Int pieceGridPoint = Geometry.GridFromPoint(attacker.transform.position);
        survivalPieces[pieceGridPoint.x, pieceGridPoint.y] = attacker;

        pieceGridPoint = Geometry.GridFromPoint(defender.transform.position);
        survivalPieces[pieceGridPoint.x, pieceGridPoint.y] = defender;
    }

    private IEnumerator SurvivalCountDown()
    {
        yield return new WaitForSeconds(survivalTimer);
        if(GameState == GameStates.SURVIVAL)
        {
            InitiateDefenderWins();
        }
    }

    public enum GameStates//
    {
        CHESS,
        SURVIVAL
    }
}
