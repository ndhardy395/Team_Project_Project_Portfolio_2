using UnityEngine;

public class Propelrturn : MonoBehaviour
{
    public float spinSpeed = 1200f;


    void Update()
    {
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime, Space.Self);
    }
}
