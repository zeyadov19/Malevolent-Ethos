using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemySpawner : MonoBehaviour
{
    [Tooltip("The enemy prefab to spawn.")]
    [SerializeField] private GameObject FlyingSlimes;
    [SerializeField] private Transform[] spawnPoints1;
    [SerializeField] private GameObject ExplodingSlimea;
    [SerializeField] private Transform[] spawnPoints2;


    public void SpawnFlyingSlimes()
    {
        if (FlyingSlimes == null || spawnPoints1 == null || spawnPoints1.Length == 0)
            return;

        StartCoroutine(SpawnFlyingSlimesWithInterval());
    }

    private System.Collections.IEnumerator SpawnFlyingSlimesWithInterval()
    {
        foreach (var point in spawnPoints1)
        {
            if (point != null)
                Instantiate(FlyingSlimes, point.position, point.rotation);

            yield return new WaitForSeconds(2f);
        }
    }

    public void SpawnExplodingSlimea()
    {
        if (ExplodingSlimea == null || spawnPoints2 == null || spawnPoints2.Length == 0)
            return;

        StartCoroutine(SpawnExplodingSlimeaWithInterval());
    }

    private System.Collections.IEnumerator SpawnExplodingSlimeaWithInterval()
    {
        foreach (var point in spawnPoints2)
        {
            if (point != null)
                Instantiate(ExplodingSlimea, point.position, point.rotation);

            yield return new WaitForSeconds(5f);
        }
    }
}
