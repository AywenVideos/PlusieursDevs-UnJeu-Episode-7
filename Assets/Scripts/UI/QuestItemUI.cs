using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestItemUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text reward;
    [SerializeField] private GameObject completed;
    
    public void Initialize(QuestManager.Quest quest)
    {
        backgroundImage.color = quest.isCompleted ? Color.lightGray : Color.white;
        title.text = quest.title;
        description.text = quest.description;
        reward.text = $"Récompense : {quest.emeraldReward} émeraudes";
        completed.SetActive(quest.isCompleted);
    }
} 