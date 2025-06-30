using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class QuestPanelUI : PanelUI
{
    public static QuestPanelUI Instance { get; private set; }

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform content;
    [SerializeField] private QuestItemUI itemPrefab;
    
    public override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public override void Start()
    {
        base.Start();
        
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestChanged += RefreshQuestList;
            RefreshQuestList(null);
        }
        else
        {
            Debug.LogError("QuestManager.Instance is null! Cannot subscribe to OnQuestChanged.");
        }
    }

    public override void Show()
    {
        base.Show();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestChanged -= RefreshQuestList;
        }
    }
    
    private void RefreshQuestList(QuestManager.Quest activeQuest)
    {
        Clear();
        
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("QuestManager.Instance is null when refreshing quest list. No quests to display.");
            return;
        }

        // Get all quests, filter for incomplete
        List<QuestManager.Quest> quests = QuestManager.Instance.GetAllQuests().OrderBy(q => q.isCompleted).ToList();
        
        foreach (QuestManager.Quest quest in quests)
        {
            QuestItemUI questItemUI = Instantiate(itemPrefab, content);
            questItemUI.Initialize(quest);
        }
    }

    private void Clear()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);
    }
}