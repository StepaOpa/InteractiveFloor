using UnityEngine;

public class InteractionPoint : MonoBehaviour
{
    [SerializeField] public float cameraDistance = 10f;
    [SerializeField] public Transform cameraPosition = null;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClick()
    {
        Debug.Log("Нажата точка интереса: " + gameObject.name);
    }
}
