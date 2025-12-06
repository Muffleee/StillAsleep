using UnityEngine;
using TMPro;

/// <summary>
/// Class handling user interface widget for the player's current energy.
/// </summary>
public class EnergyUI : MonoBehaviour
{
    [SerializeField] private PlayerResources player;
    [SerializeField] private TMP_Text energyText;

    /// <summary>
    /// Updates displayed energy levels each frame.
    /// </summary>
    void Update()
    {
        this.energyText.text = "Energy: " + this.player.CurrentEnergy;
    }
}
