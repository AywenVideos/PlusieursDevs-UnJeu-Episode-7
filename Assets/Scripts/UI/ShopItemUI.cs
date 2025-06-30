using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject goldPanel;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private GameObject milkPanel;
    [SerializeField] private TextMeshProUGUI milkText;
    [SerializeField] private GameObject emeraldPanel;
    [SerializeField] private TextMeshProUGUI emeraldText;

    private TileSO tileSO;

    public void Initialize(TileSO tileSO)
    {
        this.tileSO = tileSO;
        
        text.text = tileSO.tileName;
        icon.sprite = tileSO.icon;
        
        goldPanel.SetActive(tileSO.goldCost > 0);
        goldText.text = tileSO.goldCost.ToString();

        milkPanel.SetActive(tileSO.milkCost > 0);
        milkText.text = tileSO.milkCost.ToString();
        
        emeraldPanel.SetActive(tileSO.emeraldCost > 0);
        emeraldText.text = tileSO.emeraldCost.ToString();
    }

    public void Select()
    {
        PlayerData playerinfo = GameManager.Instance.PlayerData;

        if (playerinfo.gold < tileSO.goldCost)
        {
            Toast.Error("Pas assez d'or !", "Vous n'avez pas assez d'or pour construire ce bâtiment.");
            return;
        }

        if (playerinfo.milk < tileSO.milkCost)
        {
            Toast.Error("Pas assez de lait !", "Vous n'avez pas assez de lait pour construire ce bâtiment.");
            return;
        }
        if (playerinfo.emerald < tileSO.emeraldCost)
        {
            Toast.Error("Pas assez d'émeraudes !", "Vous n'avez pas assez d'émeraudes pour construire ce bâtiment.");
            return;
        }
        BuildManager.Instance.HoldTile(tileSO);
    }
}