using UnityEngine;

public class SimpleWindowController : MonoBehaviour
{
    [SerializeField] private GameObject windowPanel; // assign the UI window (disabled by default)

    // Called by the Button OnClick()
    public void ToggleWindow() {
    if (windowPanel != null) windowPanel.SetActive(!windowPanel.activeSelf);
}
}
