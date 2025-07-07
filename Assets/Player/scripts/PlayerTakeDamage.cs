using UnityEngine;

public class PlayerTakeDamage : MonoBehaviour
{
    public int contactDamage = 100;
    
    private void OnTriggerEnter2D(Collider2D other)
    {

        var ps = other.gameObject.GetComponent<PlayerStats>();
        if (ps != null)
            ps.TakeDamage(contactDamage);
    }
}
