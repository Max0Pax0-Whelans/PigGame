using UnityEngine;
using TMPro; // Remove if using legacy Text

public class ItStatusUI : MonoBehaviour
{
    [SerializeField] private PlayerPickup player;
    [SerializeField] private TextMeshProUGUI itText;

    private void Update()
    {
        if (player == null || itText == null) return;

        // Show "YOU ARE IT!" only while holding the item
        itText.text = player.IsHoldingItem() ? "YOU ARE IT!" : "";
    }
}