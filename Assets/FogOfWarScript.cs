using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarScript : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private int revealRadius = 3;
    [SerializeField] private Material fogMaterial;
    [SerializeField] private float cubeHeight = 2f;
    [SerializeField] private float cubeScale  = 1.01f;
    [Header("Particles")]
    [SerializeField] private Material particleMaterial;
    [SerializeField] private Color  particleColor  = new Color(0.3f, 0.3f, 0.5f, 0.6f);
    [SerializeField] private float  particleSpeed  = 0.4f;
    [SerializeField] private float  particleSize   = 0.5f;
    [SerializeField] private int    particleRate   = 5;

    public static FogOfWarScript INSTANCE { get; private set; }
    private void Awake() { INSTANCE = this;}
    private void Start()
    {
        playerMovement.onPlayerMoved.AddListener(OnPlayerMoved);
    }

    private void OnPlayerMoved(Vector2Int from, Vector2Int to, WallPos dir, long step)
    {
        RevealAround(to);
    }

    public void RefreshFog()
    {
        Grid grid = gameManager.GetCurrentGrid();
        if (grid == null) return;

        GridObj[,] arr = grid.GetGridArray();
        int w = arr.GetLength(0);
        int h = arr.GetLength(1);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                GridObj obj = arr[x, y];
                if (obj == null || !obj.IsInstantiated()) continue;
                if (obj.IsRevealed) continue;
                if (obj.IsFogged)   continue;

                CreateFogTile(obj, grid);
            }
        }

        RevealAround(playerMovement.GetCurrentGridPos());
    }

    public void RevealAround(Vector2Int center)
    {
        Grid grid = gameManager.GetCurrentGrid();
        if (grid == null) return;

        GridObj[,] arr = grid.GetGridArray();
        int w = arr.GetLength(0);
        int h = arr.GetLength(1);

        for (int a = -revealRadius; a <= revealRadius; a++)
        {
            for (int b = -revealRadius; b <= revealRadius; b++)
            {
                int x = center.x + a;
                int y = center.y + b;
                if (x < 0 || y < 0 || x >= w || y >= h) continue;

                arr[x, y]?.MarkRevealed();
            }
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

    public int  GetRevealRadius()           => revealRadius;
    public void SetRevealRadius(int radius) => revealRadius = Mathf.Max(0, radius);

    private void CreateFogTile(GridObj obj, Grid grid)
    {
        Vector3 worldPos = obj.GetWorldPos(grid.GetWorldOffsetX(), grid.GetWorldOffsetY());
        float tileSize   = GridObj.PLACEMENT_FACTOR * cubeScale;
        float halfSize   = tileSize * 0.5f;

        GameObject root = new GameObject("FogTile");
        root.transform.position = new Vector3(worldPos.x, 0f, worldPos.z);
        if (obj.GetparentObj() != null)
            root.transform.SetParent(obj.GetparentObj().transform, true);

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "FogCube";
        Destroy(cube.GetComponent<Collider>());
        cube.transform.SetParent(root.transform, false);
        cube.transform.localPosition = new Vector3(0f, cubeHeight * 0.5f, 0f);
        cube.transform.localScale    = new Vector3(tileSize, cubeHeight, tileSize);

        if (fogMaterial != null)
        {
            MeshRenderer mr      = cube.GetComponent<MeshRenderer>();
            mr.material          = fogMaterial;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows    = false;
        }

        float midY = cubeHeight * 0.5f;

        AddSideParticles(root, new Vector3( halfSize, midY,  0f),  Vector3.right,   tileSize, cubeHeight);
        AddSideParticles(root, new Vector3(-halfSize, midY,  0f), -Vector3.right,   tileSize, cubeHeight);
        AddSideParticles(root, new Vector3( 0f,       midY,  halfSize),  Vector3.forward, tileSize, cubeHeight);
        AddSideParticles(root, new Vector3( 0f,       midY, -halfSize), -Vector3.forward, tileSize, cubeHeight);

        obj.SetFogQuad(root);
    }

    private void AddSideParticles(GameObject root, Vector3 localPos, Vector3 outDir, float tileSize, float height)
    {
        GameObject psObj = new GameObject("FogPS");
        psObj.transform.SetParent(root.transform, false);
        psObj.transform.localPosition = localPos;

        psObj.transform.localRotation = Quaternion.LookRotation(outDir, Vector3.up);

        ParticleSystem ps          = psObj.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psr = psObj.GetComponent<ParticleSystemRenderer>();

        var main             = ps.main;
        main.loop            = true;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(1.2f, 2.5f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(particleSpeed * 0.5f, particleSpeed);
        main.startSize       = new ParticleSystem.MinMaxCurve(particleSize * 0.6f, particleSize * 1.4f);
        main.startColor      = particleColor;
        main.startRotation   = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.gravityModifier = -0.02f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = particleRate * 8;

        var emission          = ps.emission;
        emission.rateOverTime = particleRate;

        var shape       = ps.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.scale     = new Vector3(tileSize * 0.9f, height * 0.9f, 0f);

        var vel     = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.Local;
        vel.z       = new ParticleSystem.MinMaxCurve(particleSpeed * 0.5f, particleSpeed * 1.5f); // outward (local Z = outDir)
        vel.x       = new ParticleSystem.MinMaxCurve(-particleSpeed * 0.3f, particleSpeed * 0.3f);
        vel.y       = new ParticleSystem.MinMaxCurve(0f, particleSpeed * 0.4f);

        var colorLife     = ps.colorOverLifetime;
        colorLife.enabled = true;
        Gradient g        = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(particleColor.r, particleColor.g, particleColor.b), 0f),
                new GradientColorKey(new Color(particleColor.r, particleColor.g, particleColor.b), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f,                  0f),
                new GradientAlphaKey(particleColor.a,     0.15f),
                new GradientAlphaKey(particleColor.a,     0.7f),
                new GradientAlphaKey(0f,                  1f)
            }
        );
        colorLife.color = g;

        var sizeLife      = ps.sizeOverLifetime;
        sizeLife.enabled  = true;
        sizeLife.size     = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

        psr.renderMode        = ParticleSystemRenderMode.Billboard;
        psr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        psr.receiveShadows    = false;

        if (particleMaterial != null)
            psr.material = particleMaterial;

        ps.Play();
    }
}