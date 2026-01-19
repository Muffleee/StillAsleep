using UnityEngine;

/// <summary>
/// Class handling toggle box windows.
/// </summary>
public class SimpleWindowController : MonoBehaviour
{
    [SerializeField] private GameObject windowPanel; // assign the UI window (disabled by default)

    // Called by the Button OnClick()
    public void ToggleWindow() {
        AudioManager.Instance.PlayButtonClick();
        if (this.windowPanel != null) this.windowPanel.SetActive(!this.windowPanel.activeSelf);
    }
}
