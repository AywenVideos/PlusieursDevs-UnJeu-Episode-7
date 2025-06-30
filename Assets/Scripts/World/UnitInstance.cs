using UnityEngine;
using UnityEngine.Events;

public class UnitInstance : MonoBehaviour
{

    #region Properties

    public UnitSO UnitSO => unitSo;
    public int X { get; private set; }
    public int Y { get; private set; }

    #endregion

    #region Variables

    [SerializeField] private UnitSO unitSo;

    #endregion

    public UnitInstance Initialize(int x, int y)
    {
        X = x;
        Y = y;

        return this;
    }

    public virtual string GetDefaultData()
    {
        return "";
    }
}