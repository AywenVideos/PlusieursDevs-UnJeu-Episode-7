using System.Collections;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float speed = -5f;            
    public float lifetime = 5f;      

    private void Start()
    {
        StartCoroutine(DieAfterXSeconds());
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private IEnumerator DieAfterXSeconds()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
