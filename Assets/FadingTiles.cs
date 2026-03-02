using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadingTiles : MonoBehaviour
{
    public static FadingTiles INSTANCE;

    private Queue<GridObj> fadedTiles = new Queue<GridObj>();
    private const int maxTrailLength = 5;

    private bool isActive = false;

    private void Awake()
    {
        INSTANCE = this;
    }
    
    public void Activate()
    {
        if (isActive) return;

        isActive = true;

        if (PlayerMovement.INSTANCE != null)
            PlayerMovement.INSTANCE.onPlayerMoved.AddListener(OnPlayerMoved);
    }

    public void Deactivate()
    {
        if (!isActive) return;

        isActive = false;

        if (PlayerMovement.INSTANCE != null)
            PlayerMovement.INSTANCE.onPlayerMoved.RemoveListener(OnPlayerMoved);

        while (fadedTiles.Count > 0)
            RestoreTile(fadedTiles.Dequeue());
    }

    private void OnPlayerMoved(Vector2Int lastPos, Vector2Int newPos, WallPos dir, long step)
    {
        if (!isActive) return;

        Grid grid = GameManager.INSTANCE.GetCurrentGrid();
        GridObj tile = grid.GetGridObj(newPos);

        if (tile == null) return;

        if (tile.IsActive())
        {
            StartCoroutine(FadeOutTile(tile));
            fadedTiles.Enqueue(tile);
        }

        if (fadedTiles.Count > maxTrailLength)
        {
            RestoreTile(fadedTiles.Dequeue());
        }
    }

    private IEnumerator FadeOutTile(GridObj tile)
    {
        tile.SetActiveState(false);

        GameObject floor = tile.GetFloorObj();
        if (floor == null) yield break;

        Renderer renderer = floor.GetComponent<Renderer>();
        if (renderer == null) yield break;

        Material mat = renderer.material;
        Color startColor = mat.color;
        float duration = 0.5f;
        float time = 0f;

        while (time < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, time / duration);
            mat.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            time += Time.deltaTime;
            yield return null;
        }

        mat.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        floor.SetActive(false);
    }

    private void RestoreTile(GridObj tile)
    {
        if (tile == null) return;

        tile.SetActiveState(true);

        GameObject floor = tile.GetFloorObj();
        if (floor == null) return;

        floor.SetActive(true);

        Renderer renderer = floor.GetComponent<Renderer>();
        if (renderer == null) return;

        Color c = renderer.material.color;
        renderer.material.color = new Color(c.r, c.g, c.b, 1f);
    }
}