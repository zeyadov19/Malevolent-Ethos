using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemySpawner : MonoBehaviour
{
    [Tooltip("The enemy prefab to spawn.")]
    public GameObject FlyingSlimes;
    public Transform[] spawnPoints1;
    public GameObject ExplodingSlimea;
    public Transform[] spawnPoints2;

    public void SpawnFlyingSlimes()
    {
        if (FlyingSlimes == null || spawnPoints1 == null || spawnPoints1.Length == 0)
            return;

        foreach (var point in spawnPoints1)
        {
            if (point != null)
                Instantiate(FlyingSlimes, point.position, point.rotation);
        }
    }

    public void SpawnExplodingSlimea()
    {
        if (ExplodingSlimea == null || spawnPoints2 == null || spawnPoints2.Length == 0)
            return;

        foreach (var point in spawnPoints2)
        {
            if (point != null)
                Instantiate(ExplodingSlimea, point.position, point.rotation);
        }
    }
}
