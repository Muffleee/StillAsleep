using UnityEngine;

/// <summary>
/// Class handling toggle box windows.
/// </summary>
public class SimpleWindowController : MonoBehaviour
{
    [SerializeField] private GameObject windowPanel; // assign the UI window (disabled by default)
    public static SimpleWindowController INSTANCE;
    private void Start()
    {
        INSTANCE = this;
    }
    public void ToggleWindow() {
        if (this.windowPanel != null) this.windowPanel.SetActive(!this.windowPanel.activeSelf);
    }
}
