using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TileInstance : MonoBehaviour
{
    #region Properties

    public TileSO TileSO => tileSO;
    public int X { get; private set; }
    public int Y { get; private set; }
    #endregion

    #region Variables

    [SerializeField] private TileSO tileSO;
    public Levels[] levels;
    #endregion

    #region Events
    public UnityEvent OnSelectEvent;
    public UnityEvent OnUpgradeEvent;
    public UnityEvent OnDestroyEvent;
    #endregion

    public TileInstance Initialize(int x, int y)
    {
        X = x;
        Y = y;

        return this;
    }

    public virtual string GetDefaultData()
    {
        return "";
    }

    public virtual void OnSelect()
    {
        UserInterface.Instance.ToggleDestroyButton(true);
        OnSelectEvent?.Invoke();
    }

    public virtual void OnDestroy()
    {
        //Gain 75% of basic cost
        if (SceneManager.GetActiveScene().name == "01_Game")
        {
            GameManager.Instance.PlayerData.gold += Mathf.RoundToInt(tileSO.goldCost * 0.75f);
            GameManager.Instance.PlayerData.milk += Mathf.RoundToInt(tileSO.milkCost * 0.75f);
            GameManager.Instance.PlayerData.emerald += Mathf.RoundToInt(tileSO.emeraldCost * 0.75f);
        }
        OnUnSelect();
        UserInterface.Instance.SetSelectedTileInstance(null);
        OnDestroyEvent?.Invoke();
    }

    public virtual void OnUpgrade(int level)
    {
        foreach (Levels lev in levels)
        {
            lev._levelVisual.SetActive(false);
        }

        if (level < levels.Length)
        {
            levels[level]._levelVisual.SetActive(true);
        }

        OnUpgradeEvent?.Invoke();
    }

    public virtual void OnUnSelect()
    {
        UserInterface.Instance.ToggleDestroyButton(false);
    }

    private void OnDrawGizmos()
    {
        if (tileSO == null)
            return;

        for(int x = 0; x < tileSO.width; x++)
        {
            for(int y = 0; y < tileSO.height; y++)
            {
                Gizmos.color = Color.red;
                Vector3 offset = new Vector3(x * 2f, 0f, y * 2f);
                Gizmos.DrawWireCube(transform.position + offset, new Vector3(2, 0, 2));
            }
        }
    }
}


[System.Serializable]
public class Levels
{
    public string _levelName = "Next House Level Name";
    public string _levelDescription = "Next House Level Description";
    public GameObject _levelVisual;
}
