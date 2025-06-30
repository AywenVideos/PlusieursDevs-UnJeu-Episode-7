using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogSystemUI : MonoBehaviour
{
    public static DialogSystemUI Instance { get; private set; }
    
    [SerializeField] private Image icon;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI dialogue;
    
    [Header("Animation")]
    [SerializeField] private List<RectTransform> panelsToAnim;
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float scaleFrom = 0.3f;
    
    private DialogueSO currentDialogueSO;
    private int currentDialogueSOIndex;
    private bool isWriting;
    private bool wantFastWriting;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        HidePanel();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && currentDialogueSO != null)
        {
            DisplayNextDialogue();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && currentDialogueSO != null)
        {
            EnDialogue();
        }
    }
    
    public void BeginDialogue(DialogueSO dialogueSO)
    {
        if (currentDialogueSO != null) return;

        currentDialogueSO = dialogueSO;
        currentDialogueSOIndex = 0;
        ShowPanel();
        DisplayNextDialogue();
    }

    private void DisplayNextDialogue()
    {
        if (currentDialogueSOIndex >= currentDialogueSO.allDialogues.Length)
        {
            EnDialogue();
            return;
        }

        if (isWriting)
        {
            wantFastWriting = true;
        }
        else
        {
            DialogueNode currentNode = currentDialogueSO.allDialogues[currentDialogueSOIndex];

            title.text = currentNode._personName;
            icon.sprite = currentNode._icon;
            ShowPanel();
            currentDialogueSOIndex++;
            StartCoroutine(WriteLine(currentNode._dialogue, dialogue));
        }
    }

    private IEnumerator WriteLine(string text, TMP_Text target)
    {
        isWriting = true;
        
        int index = currentDialogueSOIndex;
        target.text = "";

        int chunkSize;
        for (int i = 0; i < text.Length; i += chunkSize)
        {
            if (currentDialogueSOIndex != index) break;
            chunkSize = wantFastWriting ? 3 : 1;
            
            int length = Math.Min(chunkSize, text.Length - i);
            string stringToAdd = text.Substring(i, length);

            target.text += stringToAdd;
            
            yield return new WaitForSeconds(wantFastWriting ? 0.001f : 0.04f);
            if (stringToAdd.Contains('\n')) yield return new WaitForSeconds(wantFastWriting ? 0.02f : 0.8f);
        }

        isWriting = false;
        wantFastWriting = false;
    }
    
    private void EnDialogue()
    {
        currentDialogueSO = null;
        currentDialogueSOIndex = 0;
        HidePanel();
    }
    
    private void ShowPanel()
    {
        foreach (RectTransform panel in panelsToAnim)
        {
            panel.DOKill();
            panel.localScale = Vector3.one * scaleFrom;

            Sequence s = DOTween.Sequence();
            s.Append(panel.DOScale(1f, duration).SetEase(Ease.OutBack));
            s.Join(canvasGroup.DOFade(1f, duration));
            s.OnComplete(() => {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            });
        }
    }
    
    private void HidePanel()
    {
        foreach (RectTransform panel in panelsToAnim)
        {
            panel.DOKill();

            Sequence s = DOTween.Sequence();
            s.Append(panel.DOScale(scaleFrom, duration).SetEase(Ease.InBack));
            s.Join(canvasGroup.DOFade(0f, duration));
            s.OnComplete(() =>
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            });
        }
    }
}
