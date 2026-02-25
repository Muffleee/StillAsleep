using System.Collections;
using UnityEngine;

public class TileFadeController : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float respawnDelay = 3f;

    private Grid grid;

   private IEnumerator Start()
    {
        // Warten bis GameManager existiert
        while (GameManager.INSTANCE == null)
            yield return null;

        // Warten bis Grid existiert
        while (GameManager.INSTANCE.GetCurrentGrid() == null)
            yield return null;

        grid = GameManager.INSTANCE.GetCurrentGrid();

        PlayerMovement.INSTANCE.onPlayerMoved.AddListener(OnPlayerMoved);
    }

    private void OnPlayerMoved(Vector2Int lastPos, Vector2Int newPos, WallPos dir, long step)
    {
        Debug.Log("Player moved from: " + lastPos);
        GridObj tile = grid.GetGridObj(lastPos);
        if (tile != null && tile.IsActive())
        {
            StartCoroutine(FadeTileRoutine(tile));
        }
    }

    private IEnumerator FadeTileRoutine(GridObj tile)
    {

        GameObject floor = tile.GetFloorObj();

        if (floor == null) yield break;

        Renderer renderer = floor.GetComponentInChildren<Renderer>();

        if (renderer == null) yield break;

        Material mat = renderer.material;

        Color originalColor = mat.color;

        // -------- Fade Out --------
            
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {

            elapsed += Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;

        }

        mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        tile.SetActiveState(false);   // 🔴 Spieler darf hier nicht mehr laufen


        yield return new WaitForSeconds(respawnDelay);

        tile.SetActiveState(true);    // 🟢 wieder begehbar

        
        elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            mat.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        mat.color = originalColor;

    }
}