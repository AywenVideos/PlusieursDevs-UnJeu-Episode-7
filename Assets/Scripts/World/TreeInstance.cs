using DG.Tweening;
using UnityEngine;

public class TreeInstance : TileInstance
{
    [SerializeField] private Transform modelTransform;

    public override void OnSelect()
    {
        base.OnSelect();

        modelTransform.localScale = Vector3.one * 1.2f;
        modelTransform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);
    }
}