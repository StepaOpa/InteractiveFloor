using UnityEngine;

public class IcebreakerTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("IceShard"))
        {
            other.gameObject.GetComponent<IceShard>().ActivateRigidbody();
        }
    }
}
