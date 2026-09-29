using UnityEngine;
using TMPro; // Remove if using legacy Text

public class PickupPromptUI : MonoBehaviour
{
    [SerializeField] private PlayerPickup player;
    [SerializeField] private TextMeshProUGUI promptText;

    private void Update()
    {
        if (player == null || promptText == null) return;

        if (player.IsHoldingItem())
        {
            promptText.text = $"[Left Click] Throw {player.GetHeldItemName()}";
        }
        else if (player.HasItemInRange())
        {
            promptText.text = $"[E] Pick up {player.GetClosestItemName()}";
        }
        else
        {
            promptText.text = "";
        }
    }
}