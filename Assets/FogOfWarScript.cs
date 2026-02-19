using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarScript : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private int revealRadius = 3;
    [SerializeField] private float cubeHeight = 2f;
    [SerializeField] private float cubeScale  = 1.01f;
    [SerializeField] private Material fogMaterial;
    [SerializeField] private Texture2D fogTexture;
    [SerializeField] private float scrollSpeedX = 0.04f;
    [SerializeField] private float scrollSpeedZ = 0.02f;
    private static readonly int OffsetID = Shader.PropertyToID("_BaseMap_ST");
    private static readonly int BaseMap  = Shader.PropertyToID("_BaseMap");
    public static FogOfWarScript INSTANCE { get; private set; }
    private void Awake() { INSTANCE = this;}
    private void Start()
    {
        fogMaterial.SetTexture(BaseMap, fogTexture);
        playerMovement.onPlayerMoved.AddListener((f, to, d, s) => RevealAround(to));
    }
    private void Update()
    {
        fogMaterial.SetVector(OffsetID, new Vector4(1f, 1f, (Time.time * scrollSpeedX) % 1f, (Time.time * scrollSpeedZ) % 1f));
    }
    public void RefreshFog()
    {
        var grid = gameManager.GetCurrentGrid();
        if (grid == null) return;
        var arr = grid.GetGridArray();
        for (int x = 0; x < arr.GetLength(0); x++)
            for (int y = 0; y < arr.GetLength(1); y++)
            {
                var obj = arr[x, y];
                if (obj == null || !obj.IsInstantiated() || obj.IsRevealed || obj.IsFogged) continue;
                CreateFogTile(obj, grid);
            }
        RevealAround(playerMovement.GetCurrentGridPos());
    }

    public void RevealAround(Vector2Int center)
    {
        var grid = gameManager.GetCurrentGrid();
        if (grid == null) return;
        var arr = grid.GetGridArray();
        for (int a = -revealRadius; a <= revealRadius; a++)
            for (int b = -revealRadius; b <= revealRadius; b++)
            {
                int x = center.x + a, y = center.y + b;
                if (x >= 0 && y >= 0 && x < arr.GetLength(0) && y < arr.GetLength(1))
                    arr[x, y]?.MarkRevealed();
            }
    }

    public void RevealAll()
    {
        Grid grid = gameManager.GetCurrentGrid();
        if (grid == null) return;
        GridObj[,] arr = grid.GetGridArray();
        for (int x = 0; x < arr.GetLength(0); x++)
            for (int y = 0; y < arr.GetLength(1); y++)
                arr[x, y]?.MarkRevealed();
    }

    public void ResetFog()
    {
        Grid grid = gameManager.GetCurrentGrid();
        if (grid == null) return;
        GridObj[,] arr = grid.GetGridArray();
        for (int x = 0; x < arr.GetLength(0); x++)
            for (int y = 0; y < arr.GetLength(1); y++)
                arr[x, y]?.ResetFogState();
    }

    public int  GetRevealRadius() { return revealRadius; }
    public void SetRevealRadius(int radius) { revealRadius = Mathf.Max(0, radius); }

    private void CreateFogTile(GridObj obj, Grid grid)
    {
        Vector3 worldPos = obj.GetWorldPos(grid.GetWorldOffsetX(), grid.GetWorldOffsetY());
        float size = GridObj.PLACEMENT_FACTOR * cubeScale;

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "FogCube";
        Destroy(cube.GetComponent<Collider>());
        cube.transform.position = new Vector3(worldPos.x, cubeHeight * 0.5f, worldPos.z);
        cube.transform.localScale = new Vector3(size, cubeHeight, size);

        if (obj.GetparentObj() != null)
            cube.transform.SetParent(obj.GetparentObj().transform, true);

        cube.GetComponent<MeshRenderer>().material = fogMaterial;
        cube.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        cube.GetComponent<MeshRenderer>().receiveShadows = false;

        obj.SetFogQuad(cube);
    }
}