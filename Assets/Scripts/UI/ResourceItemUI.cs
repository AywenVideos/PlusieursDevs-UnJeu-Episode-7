using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI fallbackText;
    [SerializeField] private TextMeshProUGUI average;
    [SerializeField] private GameObject fillArea;
    [SerializeField] private ResourceType type;
    [Header("Le slider est utilisé quand la variable Type est à limited")]
    [SerializeField] private Slider slider;

    private void Start()
    {
        fillArea.SetActive(type == ResourceType.Limited);
    }

    public void UpdateAverage(int amount)
    {
        average.text = $"{amount}/s";
    }

    public void SetResource(int amount)
    {
        text.text = amount.ToString();
        fallbackText.text = text.text;
    }

    public void SetResource(int amount, int maxAmount)
    {
        text.text = amount.ToString();
        fallbackText.text = text.text;
        slider.value = (float)amount / maxAmount;
    }
}

public enum ResourceType
{
    Unlimited,
    Limited
}
