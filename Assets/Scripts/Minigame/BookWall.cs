using UnityEngine;

public class BookWall : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3f;
    public float lifeTime = 5f;

    [Header("Child Parts")]
    public Transform[] parts;

    void Start()
    {
        Destroy(gameObject, lifeTime);

        if (parts == null || parts.Length == 0)
        {
            parts = GetComponentsInChildren<Transform>();
        }
    }

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    public void ApplyCombination(bool[] disabled)
    {
        for (int i = 1; i < parts.Length; i++)
        {
            parts[i].gameObject.SetActive(!disabled[i]);
        }
    }
}
