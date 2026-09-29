using UnityEngine;
using TMPro; // Remove if using legacy Text

public class AIItStatusUI : MonoBehaviour
{
    [SerializeField] private PlayerPickup aiPlayer;   // The AI's PlayerPickup component
    [SerializeField] private TextMeshProUGUI itText;

    private void Update()
    {
        if (aiPlayer == null || itText == null) return;

        // Show "AI is It!" only while the AI is holding the ball
        itText.text = aiPlayer.IsHoldingItem() ? "AI is It!" : "";
    }
}