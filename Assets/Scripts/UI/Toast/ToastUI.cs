using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ToastUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Animator animator;
    [SerializeField] private Image icon;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clips;

    public ToastUI Initialize(string text, float destroyTime = 5f)
    {
        this.text.text = text;

        Invoke(nameof(Hide), destroyTime);
        return this;
    }

    public ToastUI Initialize(string text, Sprite icon, Sprite background, float destroyTime = 5f)
    {
        this.text.text = text;
        this.backgroundImage.sprite = background;

        if (icon != null)
            this.icon.sprite = icon;

        Invoke(nameof(Hide), destroyTime);
        return this;
    }
    
    public ToastUI Initialize(string text, string description, Sprite icon, Sprite background, float destroyTime = 5f)
    {
        this.text.text = text;
        this.description.text = description;
        this.backgroundImage.sprite = background;

        if(icon != null)
            this.icon.sprite = icon;

        Invoke(nameof(Hide), destroyTime);
        return this;
    }

    private void Start()
    {
        audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }

    public void Hide()
    {
        animator.SetBool("IsShown", false);
    }

    public void DestroyGo()
    {
        Destroy(gameObject);
    }
}