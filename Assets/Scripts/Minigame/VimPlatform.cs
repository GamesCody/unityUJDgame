using UnityEngine;

public class VimPlatform : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3f;              // prędkość ruchu
        public float lifeTime = 5f;
    public Vector2 direction = Vector2.left; // kierunek ruchu
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
    void Update()
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime);
    }
}
