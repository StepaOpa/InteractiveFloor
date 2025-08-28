using UnityEngine;
using System.Collections;

public class IceShard : MonoBehaviour
{

    private void Start()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().useGravity = false;
    }

    public void ActivateRigidbody()
    {
        GetComponent<Rigidbody>().isKinematic = false;
    }

    public void Dissapear(float dissapearTime)
    {
        StartCoroutine(DissapearCoroutine(dissapearTime));
    }

    private IEnumerator DissapearCoroutine(float dissapearTime)
    {
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsedTime < dissapearTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dissapearTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }
        Destroy(gameObject);
    }
}
