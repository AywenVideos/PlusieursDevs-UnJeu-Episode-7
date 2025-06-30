using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LaboratoryItemUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text goldCost;
    [SerializeField] private TMP_Text milkCost;
    [SerializeField] private TMP_Text emeraldCost;
    [SerializeField] private Image image;
    [SerializeField] private GameObject unlocked;
    
    private BuildingSO buildingSO;
    private BuildingSO laboratoryBuildingSO;
    
    public void Initialize(BuildingSO buildingSO, BuildingSO laboratoryBuildingSO)
    {
        this.buildingSO = buildingSO;
        this.laboratoryBuildingSO = laboratoryBuildingSO;
        UnlockNode unlockNode = buildingSO.UnlockNode.FirstOrDefault(u => u.building == laboratoryBuildingSO);
        button.interactable = !buildingSO.Unlocked;
        backgroundImage.color = buildingSO.Unlocked ? Color.lightGray : Color.white;
        title.text = buildingSO.tileName;
        goldCost.text = unlockNode?.goldCost.ToString() ?? "0";
        milkCost.text = unlockNode?.milkCost.ToString() ?? "0";
        emeraldCost.text = unlockNode?.emeraldCost.ToString() ?? "0";
        image.sprite = buildingSO.icon;
        unlocked.SetActive(buildingSO.Unlocked);
    }

    public void OnClickUnlock()
    {
        UnlockNode unlockNode = buildingSO.UnlockNode.FirstOrDefault(u => u.building == laboratoryBuildingSO);
        BuildManager.Instance.UnlockBuilding(unlockNode, buildingSO);
        Initialize(buildingSO, laboratoryBuildingSO);
    }
}