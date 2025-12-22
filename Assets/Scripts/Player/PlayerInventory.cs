using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Mobile JumpPad Einstellungen")]
    [SerializeField] private int mobileJumpPadCost = 10;
    [SerializeField] private int mobileJumpPadCooldownSteps = 10;

    private int mobileJumpPadCooldownRemainingSteps = 0;

    public int MobileJumpPadCost
    {
        get { return this.mobileJumpPadCost; }
    }

    public int MobileJumpPadCooldownSteps
    {
        get { return this.mobileJumpPadCooldownSteps; }
    }

    public bool CanPlaceMobileJumpPad()
    {
        return this.mobileJumpPadCooldownRemainingSteps <= 0;
    }

    public int GetMobileJumpPadCooldownRemainingSteps()
    {
        return this.mobileJumpPadCooldownRemainingSteps;
    }

    public void TriggerMobileJumpPadCooldown()
    {
        this.mobileJumpPadCooldownRemainingSteps = this.mobileJumpPadCooldownSteps;
    }

    // Wird bei jedem Schritt (Move) einmal aufgerufen
    public void OnPlayerStep()
    {
        if (this.mobileJumpPadCooldownRemainingSteps > 0)
        {
            this.mobileJumpPadCooldownRemainingSteps--;
        }
    }
}
