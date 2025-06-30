using System.Collections;
using UnityEngine;

public class CanonScript : MonoBehaviour
{
    public GameObject target;

    [SerializeField]
    private GameObject bullet;
    [SerializeField]
    private Transform firePosition;
    public Transform rotatingPart;
    void Start()
    {
        target = PickATarget();
        StartCoroutine("Fire");
    }
    IEnumerator Fire()
    {
        while (true)
        {
            // fire each 5 seconds
            yield return new WaitForSeconds(0.5f);
            Instantiate(bullet, firePosition);
        }
    }

    void Update()
    {
        Vector3 dir = target.transform.position - rotatingPart.position;
        rotatingPart.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0, -90, 0);
    }

    GameObject PickATarget()
    {
        GameObject[] troupes = GameObject.FindGameObjectsWithTag("Troupes");
        GameObject closerTroupe = troupes[0];
        foreach (GameObject troupe in troupes)
        {
            if(Vector3.Distance(troupe.transform.position, transform.position) < Vector3.Distance(closerTroupe.transform.position, transform.position))
            {
                closerTroupe = troupe;
            }
        }
        return closerTroupe;
    }
}
