using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPCListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private Button assignButton;

    private NPC npc;
    private NPCAssignmentUI parentUI;

    public void Initialize(NPC npc, NPCAssignmentUI parentUI)
    {
        this.npc = npc;
        this.parentUI = parentUI;

        if (npcNameText != null)
            npcNameText.text = npc.GetDisplayName();

        if (assignButton != null)
            assignButton.onClick.AddListener(OnAssignButtonClicked);
    }

    private void OnAssignButtonClicked()
    {
        if (npc != null && parentUI != null)
        {
            parentUI.AssignNPCToBuilding(npc);
        }
    }

    private void OnDestroy()
    {
        if (assignButton != null)
            assignButton.onClick.RemoveListener(OnAssignButtonClicked);
    }
} 