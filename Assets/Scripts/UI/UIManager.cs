using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private TMP_Text npcText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text balanceText;

    [Header("Interaction UI")]
    [SerializeField] private TMP_Text crosshairText;
    [SerializeField] private TMP_Text worldHintText;

    public void SetNPCText(string text)
    {
        if (npcText != null)
            npcText.text = text;
    }

    public void SetStatusText(string text)
    {
        if (statusText != null)
            statusText.text = text;
    }

    public void SetScoreText(string text)
    {
        if (scoreText != null)
            scoreText.text = text;
    }

    public void ClearScore()
    {
        if (scoreText != null)
            scoreText.text = string.Empty;
    }

    public void SetCrosshairVisible(bool visible)
    {
        if (crosshairText != null)
            crosshairText.gameObject.SetActive(visible);
    }

    public void ShowWorldHint(string text, Vector3 worldPosition, Camera camera)
    {
        if (worldHintText == null || camera == null)
            return;

        Vector3 screenPosition = camera.WorldToScreenPoint(worldPosition);

        if (screenPosition.z < 0)
        {
            HideWorldHint();
            return;
        }

        worldHintText.gameObject.SetActive(true);
        worldHintText.text = text;
        worldHintText.rectTransform.position = screenPosition;
    }

    public void HideWorldHint()
    {
        if (worldHintText != null)
        {
            worldHintText.text = string.Empty;
            worldHintText.gameObject.SetActive(false);
        }
    }
    public void ShowFocusHint(string text)
    {
        if (worldHintText == null)
            return;

        worldHintText.gameObject.SetActive(true);
        worldHintText.text = text;

        worldHintText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        worldHintText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        worldHintText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        worldHintText.rectTransform.anchoredPosition = new Vector2(0f, -120f);
    }

    public void HideFocusHint()
    {
        HideWorldHint();
    }
    public void SetBalanceText(float balance)
    {
        if (balanceText == null)
            return;

        balanceText.text = $"Баланс: {balance:F0} р.";
    }
}