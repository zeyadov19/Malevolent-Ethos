using UnityEngine;

public class Arrow : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 5f;
    

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerStats>()?.TakeDamage(damage); // or pass custom knockback
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("GroundLayer"))
        {
            Destroy(gameObject);
        }
    }
}
