using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// bon j'ai peut être un peu abusé en faisant un système hyper compliqué de toast / notifications ... 
// svp utilisez le : Toast.Message, Toast.Error, Toast.Warning, Toast.Info
public class ToastUIManager : MonoBehaviour
{
    public static ToastUIManager Instance { get; private set; }
    [SerializeField] private Transform content;
    [SerializeField] private ToastUI toastTitlePrefab;
    [SerializeField] private ToastUI toastTitleDescriptionPrefab;
    [SerializeField] private ToastUI toastTitleIconPrefab;
    [SerializeField] private ToastUI toastTitleDescriptionIconPrefab;

    [Header("Sprites Settings")]
    [SerializeField] private Sprite dangerBackground;
    [SerializeField] private Sprite warningBackground;
    [SerializeField] private Sprite normalBackground;
    [SerializeField] private Sprite infoBackground;

    [SerializeField] private Sprite dangerIcon;
    [SerializeField] private Sprite warningIcon;
    [SerializeField] private Sprite normalIcon;
    [SerializeField] private Sprite infoIcon;

    private void Awake()
    {
        Instance = this;
    }

    public void CreateToast(string title, Toast.Type type, float time = 5f, string description = null, Sprite icon = null)
    {
        ToastUI toast = null;
        Sprite background = normalBackground;
        Sprite newIcon = icon;
        
        switch(type)
        {
            case Toast.Type.Danger:
                if(icon == null)
                    newIcon = dangerIcon;

                background = dangerBackground;
                break;
            case Toast.Type.Normal:
                if (icon == null)
                    newIcon = normalIcon;

                background = normalBackground;
                break;
            case Toast.Type.Info:
                if (icon == null)
                    newIcon = infoIcon;

                background = infoBackground;
                break;
            case Toast.Type.Warning:
                if (icon == null)
                    newIcon = warningIcon;

                background = warningBackground;
                break;
        }

        if(description != null && newIcon != null)
        {
            toast = Instantiate(toastTitleDescriptionIconPrefab, content).Initialize(title, description, newIcon, background, time);
        }else if(description != null)
        {
            toast = Instantiate(toastTitleDescriptionIconPrefab, content).Initialize(title, description, newIcon, background, time);
        }else if(icon != null)
        {
            toast = Instantiate(toastTitleDescriptionIconPrefab, content).Initialize(title, newIcon, background, time);
        }else
        {
            toast = Instantiate(toastTitleDescriptionIconPrefab, content).Initialize(title, null, background, time);
        }
    }
}

public static class Toast
{
    public enum Type
    {
        Normal,
        Info,
        Warning,
        Danger
    }

    public static void Notify(string text, Toast.Type type, Sprite sprite, string description = null, float time = 5f)
    {
        ToastUIManager.Instance.CreateToast(text, type, time, description, sprite);
    }

    public static void Info(string text, float time = 5f)
    {
        ToastUIManager.Instance.CreateToast(text, Type.Info, time);
    }

    public static void Info(string text, string description, float time = 5f)
    {
        ToastUIManager.Instance.CreateToast(text, Type.Info, time, description);
    }

    public static void Message(string text, float time = 5f)
    {
        ToastUIManager.Instance.CreateToast(text, Type.Normal, time);
    }

    public static void Message(string text, string description, float time = 5f)
    {
        ToastUIManager.Instance.CreateToast(text, Type.Normal, time, description);
    }

    public static void Warning(string text, float time = 5f)
    {
        ToastUIManager.Instance.CreateToast(text, Type.Warning, time);
    }

    public static void Warning(string text, string description, float time = 5f)
    {
        ToastUIManager.Instance.CreateToast(text, Type.Warning, time, description);
    }

    public static void Error(string text, float time = 5f)
    {
        ToastUIManager.Instance.CreateToast(text, Type.Danger, time);
    }

    public static void Error(string text, string description, float time = 5f)
    {
        ToastUIManager.Instance.CreateToast(text, Type.Danger, time, description);
    }
}