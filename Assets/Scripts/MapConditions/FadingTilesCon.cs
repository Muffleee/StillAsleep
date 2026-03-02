using UnityEngine;

public class FadingTilesCon : MonoBehaviour, IMapCondition
{
    public int Difficulty()
    {
        return 1; // anpassen falls nötig
    }

    public void Initiate(int level)
    {
        if (FadingTiles.INSTANCE != null){
            Debug.Log("FadingTiles INIT");
            FadingTiles.INSTANCE.Activate();}
    }

    public void Deactivate()
    {
        if (FadingTiles.INSTANCE != null){
            FadingTiles.INSTANCE.Deactivate();}
    }
}