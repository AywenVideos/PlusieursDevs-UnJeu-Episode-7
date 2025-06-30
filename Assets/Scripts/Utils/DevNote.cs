using UnityEngine;

// Utilise ce script pour expliquer tes systèmes aux prochains devs <3
public class DevNote : MonoBehaviour
{
    public enum MessageType
    {
        Info,
        Warning,
        Error
    }

    [SerializeField] private string message;
    [SerializeField] private MessageType messageType = MessageType.Info;
    [SerializeField] private bool readOnly = false;

    public string Message => message;
    public MessageType Type => messageType;
    public bool ReadOnly => readOnly;
}