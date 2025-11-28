using UnityEngine;
using TMPro;

public class EnergyUI : MonoBehaviour
{
    [SerializeField] private PlayerResources player;
    [SerializeField] private TMP_Text energyText;

    void Update()
    {
        energyText.text = "Energy: " + player.CurrentEnergy;
    }
}
