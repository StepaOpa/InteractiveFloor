using UnityEngine;

public class IcebreakerCollider : MonoBehaviour
{
    [SerializeField] private float dissapearTime = 1f;
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("IceShard"))
        {
            other.gameObject.GetComponent<IceShard>().Dissapear(1f);
        }
    }
}
