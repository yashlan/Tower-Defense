using UnityEngine;

public class AutoDestroyObject : MonoBehaviour
{
    void Start() => Destroy(gameObject, 0.5f);
}
