using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialManager: MonoBehaviour
{
    [SerializeField] private GameObject tutorialLayover;
    [SerializeField] private TMP_Text tutorialText;
    private TutorialGrid tutGrid;
    private Queue<Action> tutorials = new Queue<Action>();
    private String currentMessage = null;
    private bool screenUp = false;
    bool disableMovement = false;
    bool disablePlacing = false;
    long currentPlayerSteps = 0;
    private GridObj startGridObj = null;
    private GridObj enemyGridObj = null;
    int tutorialPhase = 0;

    private void Awake()
    {
        tutorials.Enqueue(EnemyTutorial);
        tutorials.Enqueue(PlaceableTutorial);
        tutorials.Enqueue(GenerateTutorial);
        tutorials.Enqueue(JumpAndTrapTutorial);
        tutorials.Enqueue(IceRotatingSpikeTutorial);
    }
    private void Start()
    {
        PlayerMovement.INSTANCE.onPlayerMoved.AddListener(PlayerMove);
    }
    private void PlayerMove(Vector2Int lastPlayerPos, Vector2Int playerPos, WallPos direction, long steps)
    {
        EnemyMovement.INSTANCE.MoveEnemy();
        if (currentPlayerSteps == 0) currentPlayerSteps = steps;
        if(steps == currentPlayerSteps + 2)
        {
            Debug.Log("nextStep");
            NextStep();
        }
        if((lastPlayerPos == new Vector2Int(0,2) && playerPos == new Vector2Int(0,3) || lastPlayerPos == new Vector2Int(0, 3)) && playerPos == new Vector2Int(0, 2))
        {
            NextStep();
        }
    }
    private void NextStep()
    {
        if (tutorials.Count > 0)
        {
            tutorialPhase++;
            Action step = tutorials.Dequeue();
            step.Invoke();
            
        }
    }
    private void Update()
    {
        if(screenUp && Input.GetKeyDown(KeyCode.Mouse0))
        {
            screenUp = false;
            tutorialText.text = currentMessage;
            ResetGame();
        }
    }
    public void SpawnCrystalOnObject(GridObj tile, int worldOffsetX, int worldOffsetY) 
    {
        if (tutorialPhase != 4) return;
        Vector3 worldPos = tile.GetWorldPos(worldOffsetX, worldOffsetY);
        EnergyCrystal.PrepareSpawn(worldPos, 100);
        Instantiate(GameManager.INSTANCE.GetPrefabLibrary().prefabEnergyCrystal, worldPos, Quaternion.identity);
        
    }
    public void StartTutorial(Grid grid)
    {
        
        if (tutGrid == null) tutGrid = new TutorialGrid();
        tutorialLayover.SetActive(true);
        tutGrid.FillInitialGridLayout(grid.GetGridArray());
        startGridObj = grid.GetGridObj(0, 0);
        enemyGridObj = grid.GetGridObj(3, 3);
        EnemyMovement.INSTANCE.InstantiateEnemy(new Vector2Int(3, 3));
        grid.IncreaseGrid(WallPos.LEFT, 1000);
        grid.InstantiateMissing();
        
        NextStep();
    }
    public void OnWin()
    {
        screenUp = true;
        tutorialText.text = "Congratulations, you won! Just click the left mouse button to carry on with the tutorial!";
    }
    public void OnLose()
    {
        screenUp = true;
        tutorialText.text = "You lost! But don't worry, just click the left mouse button and you can carry on with the tutorial!";
    }
    private void ResetGame()
    {
        PlayerMovement.INSTANCE.ResetFigure(startGridObj.GetGridPos());
        EnemyMovement.INSTANCE.ResetFigure(enemyGridObj.GetGridPos());
    }
    public void EnemyTutorial()
    {
        Debug.Log("Enemy Tutoriaaal");
        tutorialText.text = "Your goal is to catch your own ghost. Move using WASD and zoom in and out with STRG/CTRL + mouse wheel. You ghost is sometimes able to destroy walls that are blocking him.";
        currentMessage = tutorialText.text;
    }
    public void PlacedTile()
    {
        currentPlayerSteps = 0;
        NextStep();
    }

    public void PlaceableTutorial()
    {
        Debug.Log("placable tutorial yayy");
        tutorialText.text = "You can place tiles in your inventory on a green tile at the border of the dungeon by using drag and drop or " +
            "by selecting one and clicking on the tile where you want to place it. " +
            "Placing a tile costs you one energy.";
        currentMessage = tutorialText.text;
    }

    private void GenerateTutorial()
    {
        tutorialText.text = "After some steps, the green replacable tiles at the border will be randomly filled.";
        currentMessage = tutorialText.text;
    }

    private void JumpAndTrapTutorial()
    {
        Grid grid = GameManager.INSTANCE.GetCurrentGrid();
        tutGrid.IncreaseFirstTime(grid.GetGridArray());
        grid.CollapseWorld();
        grid.IncreaseGrid(WallPos.FRONT, 1000);
        grid.InstantiateMissing();
        tutorialText.text = "You can collect a crystal. Those are worth 5 energy. \n There are more types of tiles. You can see a jumppad, where you can spend one energy to jump over an adjacent wall. " +
            "The trap will cost you 3 energy. Be careful! If you lose more energy than you currently have, you lose! There might also be hidden traps around.";
        currentMessage = tutorialText.text;
    }

    private void IceRotatingSpikeTutorial()
    {
        tutorialText.text = "There are even more tiles! Walking on an ice tile leads to sliding over it, the rotating tiles rotate clockwise(???) with every step you take. Be careful with the spikes tile. If you are on it while the spikes are out you'll lose!";
        currentMessage = tutorialText.text;
    }
    public bool IsPlacingDisabled() { return (this.tutorialPhase <= 1); }
    public bool IsMovingDisabled() { return screenUp;  }
}
