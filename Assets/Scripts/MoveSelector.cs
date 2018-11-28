using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSelector : MonoBehaviour
{
    public GameObject moveLocationPrefab;
    public GameObject tileHighlightPrefab;
    public GameObject attackLocationPrefab;
    public TileSelector myTileSelector;
    public TileSelector otherTileSelector;

    private GameObject tileHighlight;
    private GameObject movingPiece;
    private List<Vector2Int> moveLocations;
    private List<GameObject> locationHighlights;
    private bool canMove = true;
    private const float ogMoveTime = 0.1f;
    private float timeUntilNextMove;
    private int previousIndex;
    private Player myPlayer;
    private Vector2Int previousLocation;

    // Use this for initialization
    void Start ()
    {
        enabled = false;
        tileHighlight = Instantiate(tileHighlightPrefab, Geometry.PointFromGrid(new Vector2Int(0, 0)), Quaternion.identity, gameObject.transform);
        tileHighlight.SetActive(false);
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
            int playerNumber = myPlayer.playerNumber;

            if (canMove && moveLocations.Count > 0)
            {
                tileHighlight.SetActive(true);
                Vector2Int newLocation = GetNewLocation(playerNumber);
                previousLocation = newLocation;

                tileHighlight.transform.position = Geometry.PointFromGrid(newLocation);
                GameObject moveHighlight = locationHighlights.Find(q => q.transform.position == Geometry.PointFromGrid(newLocation));
                if (moveHighlight.activeSelf)
                {
                    GameObject deactivatedHighlight = locationHighlights.Find(q => !q.activeSelf);
                    if (deactivatedHighlight)
                        deactivatedHighlight.SetActive(true);
                    moveHighlight.SetActive(false);
                }
           
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

            if (Input.GetButtonDown("A" + playerNumber) && moveLocations.Count > 0)
            {
                MovePieceTo(previousLocation);
            }

            if (Input.GetButtonDown("B" + playerNumber))
            {
                ReturnToSelect();
            }
        }
    }

    public void SetPlayer(Player player)
    {
        myPlayer = player;
    }

    private Vector2Int GetNewLocation(int playerNumber)
    {
        int xInput = Mathf.RoundToInt(Input.GetAxis("LeftXAxis" + playerNumber));
        int newIndex = Mathf.RoundToInt(Mathf.Repeat(previousIndex + xInput, moveLocations.Count));
        previousIndex = newIndex;

        return moveLocations[newIndex];
    }

    private void MovePieceTo(Vector2Int newLocation)
    {
        GameObject pieceToCapture = GameManager.instance.PieceAtGrid(newLocation);
        if (pieceToCapture == null)
        {
            GameManager.instance.Move(movingPiece, newLocation);
            switch (GameManager.instance.GameState)
            {
                case GameManager.GameStates.CHESS:
                    ExitState();
                    break;
                case GameManager.GameStates.CHASE:
                    ReturnToSelect();
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (GameManager.instance.GameState)
            {
                case GameManager.GameStates.CHESS:
                    GameManager.instance.InitiateChase(pieceToCapture);
                    ReturnToSelect();
                    break;
                case GameManager.GameStates.CHASE:
                    GameManager.instance.InitiateAttackerWins(pieceToCapture, movingPiece);
                    ExitState();
                    break;
                default:
                    break;
            }
        }
    }

    public void EnterState(GameObject piece)
    {
        canMove = false;
        movingPiece = piece;
        previousIndex = 0;
        timeUntilNextMove = ogMoveTime;
        enabled = true;
        moveLocations = GameManager.instance.MovesForPiece(movingPiece);
        locationHighlights = new List<GameObject>();

        foreach (Vector2Int loc in moveLocations)
        {
            GameObject highlight;
            if (GameManager.instance.PieceAtGrid(loc))
            {
                highlight = Instantiate(attackLocationPrefab, Geometry.PointFromGrid(loc),
                    Quaternion.identity, gameObject.transform);
            }
            else
            {
                highlight = Instantiate(moveLocationPrefab, Geometry.PointFromGrid(loc),
                    Quaternion.identity, gameObject.transform);
            }
            locationHighlights.Add(highlight);
        }
    }

    private void ReturnToSelect(int tileCheck = 0)
    {
        enabled = false;
        tileHighlight.SetActive(false);
        GameManager.instance.DeselectPiece(movingPiece);
        movingPiece = null;
        if(tileCheck != -1)
        {
            myTileSelector.EnterState();
        }
        else
        {
            otherTileSelector.EnterState();
        }
        foreach (GameObject highlight in locationHighlights)
        {
            Destroy(highlight);
        }
    }

    public void ExitState()
    {
        if (GameManager.instance.GameState == GameManager.GameStates.CHESS)
        {
            GameManager.instance.NextPlayer();

            ReturnToSelect(-1);
        }
    }
}
