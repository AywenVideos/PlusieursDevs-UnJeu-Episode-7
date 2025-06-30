using UnityEngine;

[CreateAssetMenu(menuName =("Dialogue/New Dialogue"))]
public class DialogueSO : ScriptableObject
{
    public DialogueNode[] allDialogues;
}

[System.Serializable]
public class DialogueNode
{
    public string _personName;
    [TextArea] public string _dialogue;
    public Sprite _icon;
}
