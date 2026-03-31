using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager: MonoBehaviour
{
    [SerializeField] private Canvas tutorialLayover;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private Button resetButton;
    private TutorialGrid tutGrid;
    private Queue<Action> tutorials = new Queue<Action>();
    private String currentMessage = null;
    private bool screenUp = false;
    private bool disablePlacing = true;
    private bool disableMoving = false;
    private long currentPlayerSteps = 0;
    private GridObj startGridObj = null;
    private GridObj enemyGridObj = null;
    private int tutorialPhase = 0;
    private IMapCondition currentCond;
    private bool endPhase = false;
    private GameObject panel;
    private bool isInitialized = false;

    private void Awake()
    {   
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {   
        yield return new WaitUntil(() => GameManager.INSTANCE != null);
        yield return new WaitUntil(() => SimpleWindowController.INSTANCE != null);
        InitTutorials();
    }

    private void InitTutorials()
    {
        this.isInitialized = true;
        tutorials.Enqueue(EnemyTutorial);
        tutorials.Enqueue(PlaceableTutorial);
        tutorials.Enqueue(GenerateTutorial);
        tutorials.Enqueue(JumpAndTrapTutorial);
        tutorials.Enqueue(IceRotatingSpikeTutorial);
        tutorials.Enqueue(ItemTutorial);
        tutorials.Enqueue(PhaseAndRoundsTutorial);
        tutorials.Enqueue(FogCondTutorial);
        tutorials.Enqueue(CountdownCondTutorial);
        tutorials.Enqueue(OpponentCondTutorial);
        tutorials.Enqueue(LastTutorial);
        tutorialLayover.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);
    }

    private void PlayerMove(Vector2Int lastPlayerPos, Vector2Int playerPos, WallPos direction, long steps)
    {
        EnemyMovement.INSTANCE.MoveEnemy();
        GameManager.INSTANCE.GetCurrentGrid().RotateTiles();
        if (currentPlayerSteps == 0) currentPlayerSteps = steps;
        if(steps == currentPlayerSteps + 1)
        {
            NextStep();
        }
        if(this.tutorialPhase == 4 && (lastPlayerPos == new Vector2Int(0,2) && playerPos == new Vector2Int(0,3) || lastPlayerPos == new Vector2Int(0, 3) && playerPos == new Vector2Int(0, 2)))
        {
            NextStep();
        }
        GridObj currTile = GameManager.INSTANCE.GetCurrentGrid().GetGridObj(playerPos);
        if (this.tutorialPhase == 5 && currTile.GetGridType() == GridType.ICE)
        {
            NextStep();
        }
    }
    public void NextStep()
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
        if(tutorialPhase == 7 && Input.GetKeyDown(KeyCode.Mouse0))
        {
            NextStep();
        }
        if(tutorialPhase == 11 && Input.GetKeyDown(KeyCode.Mouse0))
        {
            Time.timeScale = 1f;

            SceneManager.LoadScene("Menu");
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
        if (!isInitialized)
        {
            StartCoroutine(WaitAndStart(grid));
            return;
        }

        if (this.resetButton != null) this.resetButton.onClick.AddListener(this.ResetGame);
        PlayerMovement.INSTANCE.onPlayerMoved.AddListener(PlayerMove);
        if (tutGrid == null) tutGrid = new TutorialGrid();
        panel = tutorialLayover.transform.GetChild(0).gameObject;
        RectTransform rt = panel.GetComponent<RectTransform>();
        Vector2 min = rt.offsetMin;
        Vector2 max = rt.offsetMax;

        min.y = 800;
        max.y = 0;

        rt.offsetMin = min;
        rt.offsetMax = max;
        tutorialLayover.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(true);
        tutGrid.FillInitialGridLayout(grid.GetGridArray());
        startGridObj = grid.GetGridObj(0, 0);
        enemyGridObj = grid.GetGridObj(3, 3);
        EnemyMovement.INSTANCE.InstantiateEnemy(new Vector2Int(3, 3));
        grid.IncreaseGrid(WallPos.LEFT, 1000);
        grid.InstantiateMissing();
        SimpleWindowController.INSTANCE.ToggleWindow();
        NextStep();
    }
    public void OnWin()
    {
        if (tutorialPhase <= 7)
        {
            screenUp = true;
            tutorialText.text = "Congratulations, you won! Just click the left mouse button to carry on with the tutorial!";
        } else
        {
            tutorialText.text = "Congratulations, you won! Click to continue with the next condition!";
            NextStep();
        }
    }
    public void OnLose()
    {
        if (tutorialPhase <= 7)
        {
            screenUp = true;
            tutorialText.text = "You lost! But don't worry, just click the left mouse button and you can carry on with the tutorial!";
        } else
        {
            tutorialText.text = "You lost! But don't worry, just click to start this condition again!";
            screenUp = true;
        }
    }
    public void ResetGame()
    {
        if (tutorialPhase <= 7)
        {
            PlayerMovement.INSTANCE.ResetPlayerState();
            PlayerMovement.INSTANCE.ResetFigure(startGridObj.GetGridPos());
            EnemyMovement.INSTANCE.ResetFigure(enemyGridObj.GetGridPos());
        } else
        {
            int condition = 0;
            switch (tutorialPhase)
            {
                case 8: condition = 0; break;
                case 9: condition = 1; break;
                case 10: condition = 2; break;
                default: condition = 0; break;
            }
            if (currentCond != null) currentCond.Deactivate();
            GameManager.INSTANCE.NewPhase();
            currentCond = GameManager.INSTANCE.GetMapCondition(condition);
            currentCond.Initiate(0);
            EnemyMovement.INSTANCE.ResetFigure(new Vector2Int(3, 3));
            PlayerMovement.INSTANCE.ResetPlayerState();
            GameManager.INSTANCE.ResetPhaseRound();
        }

    }
    public void EnemyTutorial()
    {
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
        disablePlacing = false;
        SimpleWindowController.INSTANCE.ToggleWindow();
        tutorialText.text = "You can place tiles in your inventory on a green tile at the border of the dungeon by using drag and drop or " +
            "by selecting one and clicking on the tile where you want to place it. " +
            "Placing a tile costs you one energy.";
        currentMessage = tutorialText.text;
    }

    private void GenerateTutorial()
    {
        disablePlacing = true;
        SimpleWindowController.INSTANCE.ToggleWindow();
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
        Grid grid = GameManager.INSTANCE.GetCurrentGrid();
        tutGrid.IncreaseSecondTime(grid.GetGridArray());
        grid.CollapseWorld();
        grid.IncreaseGrid(WallPos.RIGHT, 1000);
        grid.InstantiateMissing();
        tutorialText.text = "There are even more tiles! Walking on an ice tile leads to sliding over it, the rotating tiles rotate anti-clockwise with every step you take. Be careful with the spikes tile. If you are on it while the spikes are out you'll lose!";
        currentMessage = tutorialText.text;
    }

    private void ItemTutorial()
    {
        Grid grid = GameManager.INSTANCE.GetCurrentGrid();
        GridObj[,] gridArr = grid.GetGridArray();
        tutGrid.IncreaseThirdTime(grid.GetGridArray());
        Vector3 worldPos = gridArr[6, 3].GetWorldPos(grid.GetWorldOffsetX(), grid.GetWorldOffsetY());
        worldPos.y += 0.5f;
        GameObject selectedPrefab = GameManager.INSTANCE.GetItemPrefab(1);
        Instantiate(selectedPrefab, worldPos, Quaternion.identity);
        grid.CollapseWorld();
        grid.IncreaseGrid(WallPos.RIGHT, 1000);
        grid.InstantiateMissing();
        tutorialText.text = "You can pick up Items with E and use them with F. Scroll through your inventory slots using the mouse wheel. Using them costs energy! \n The pickaxe destroys the wall in the direction you are looking.\n The scanner reveals all hidden traps in a certain radius. \n The box can be placed where your pointer is and holds the enemy in place. \n The clock can reverse the time so you ghost will be placed back some steps.";
        currentMessage = tutorialText.text;
    }

    private void PhaseAndRoundsTutorial()
    {
        tutorialText.text = "Your goal is to win as many phases as possible to get a high score. One phase consists of three rounds where you have to catch your ghost. But not every phase is the same, here are three possible difficulties during the phases! Click to continue.";
        currentMessage = tutorialText.text;
    }

    private void FogCondTutorial()
    {
        RectTransform rt = panel.GetComponent<RectTransform>();
        Vector2 min = rt.offsetMin;
        Vector2 max = rt.offsetMax;

        min.y = 175;
        max.y = -750;

        rt.offsetMin = min;
        rt.offsetMax = max;
        SimpleWindowController.INSTANCE.ToggleWindow();
        PlayerMovement.INSTANCE.onPlayerMoved.RemoveListener(PlayerMove);
        GameManager.INSTANCE.SetTutorialCurrently(false);
        endPhase = true;
        ResetGame();
        disablePlacing = false;
        tutorialText.text = "Catch your ghost! This is the first of three conditions that can occur.";
        currentMessage = tutorialText.text;
    }
    private void CountdownCondTutorial()
    {
        ResetGame();
        disablePlacing = false;
        tutorialText.text = "Catch your ghost! This is the second of three conditions that can occur. Make a step before the timer runs out!";
        currentMessage = tutorialText.text;
    }
    private void OpponentCondTutorial()
    {
        ResetGame();
        disablePlacing = false;
        tutorialText.text = "Catch your ghost! This is the last condition that can occur. Avoid the spike ball!";
        currentMessage = tutorialText.text;
    }
    private void LastTutorial()
    {
        RectTransform rt = panel.GetComponent<RectTransform>();
        Vector2 min = rt.offsetMin;
        Vector2 max = rt.offsetMax;

        min.y = 800;
        max.y = 0;

        rt.offsetMin = min;
        rt.offsetMax = max;
        disableMoving = true;
        tutorialText.text = "Congratulations, you finished the tutorial! Click to get back to the main menu!";
        currentMessage = tutorialText.text;
    }
    IEnumerator WaitAndStart(Grid grid)
    {
        yield return new WaitUntil(() => isInitialized);
        StartTutorial(grid);
    }
    public bool IsPlacingDisabled() { return disablePlacing; }
    public bool IsMovingDisabled() { return (screenUp || disableMoving);  }
    public bool IsInEndphase() { return endPhase; }
}
