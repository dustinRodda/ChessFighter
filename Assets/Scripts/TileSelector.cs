using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSelector : MonoBehaviour
{
    public GameObject tileHighlightPrefab;
    public Vector2Int tileStartingPoint;

    private GameObject tileHighlight;
    private Vector2Int previousLocation;
    private bool canMove = true;
    private const float ogMoveTime = 0.1f;
    private float timeUntilNextMove;
    private Player myPlayer;

    // Use this for initialization
    void Start ()
    {
        Vector2Int gridpoint = Geometry.GridPoint(0, 0);
        Vector3 point = Geometry.PointFromGrid(gridpoint);
        tileHighlight = Instantiate(tileHighlightPrefab, point, Quaternion.identity, gameObject.transform);
        tileHighlight.SetActive(false);
        previousLocation = tileStartingPoint;
        timeUntilNextMove = ogMoveTime;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (GameManager.instance.isGameOver)
        {
            return;
        }

        if (myPlayer == GameManager.instance.currentPlayer || GameManager.instance.GameState == GameManager.GameStates.CHASE)
        {
            tileHighlight.SetActive(true);
            
            if (canMove)
            {
                Vector2Int newLocation = GetNewLocation(myPlayer.playerNumber);
                tileHighlight.transform.position = Geometry.PointFromGrid(newLocation);
                previousLocation = newLocation;

                canMove = false;
            }
            else
            {
                timeUntilNextMove -= Time.deltaTime;
                if (timeUntilNextMove <= 0)
                {
                    canMove = true;
                    timeUntilNextMove = ogMoveTime;
                }
            }
            if (Input.GetButtonDown("A" + myPlayer.playerNumber))
            {
                SelectPieceAt(previousLocation);
            }
        }
	}

    public void SetPlayer(Player player)
    {
        myPlayer = player;
    }

    private void SelectPieceAt(Vector2Int newLocation)
    {
        GameObject selectedPiece = GameManager.instance.PieceAtGrid(newLocation);
        if (GameManager.instance.DoesPieceBelongToCurrentPlayer(selectedPiece, myPlayer))
        {
            GameManager.instance.SelectPiece(selectedPiece, myPlayer.playerNumber);
            ExitState(selectedPiece);
        }
    }

    private Vector2Int GetNewLocation(int playerNumber)
    {
        int xInput = Mathf.RoundToInt(Input.GetAxis("LeftXAxis" + playerNumber));
        int newX = Mathf.RoundToInt(Mathf.Repeat(previousLocation.x + xInput, 8));

        int yInput = Mathf.RoundToInt(Input.GetAxis("LeftYAxis" + playerNumber));
        int newY = Mathf.RoundToInt(Mathf.Repeat(previousLocation.y + yInput, 8));

        Vector2Int newLocation = new Vector2Int(newX, newY);
        return newLocation;
    }

    public void EnterState()
    {
        enabled = true;
        tileHighlight.SetActive(true);
        tileHighlight.transform.position = Geometry.PointFromGrid(tileStartingPoint);
    }

    private void ExitState(GameObject movingPiece)
    {
        enabled = false;
        tileHighlight.SetActive(false);
        MoveSelector move = GetComponent<MoveSelector>();
        move.EnterState(movingPiece);
    }
}