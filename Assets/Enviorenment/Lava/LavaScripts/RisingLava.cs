using UnityEngine;
using System.Collections;

public class RisingLava : MonoBehaviour
{
    public float riseSpeed = 1f;
    public float moveDuration = 3f;
    public float pauseDuration = 2f;

    private bool isMoving = false;
    private bool hasStarted = false;

    public void StartLava()
    {
        AudioManager.instance.PlayAt("Lava", gameObject);
        if (!hasStarted)
        {
            hasStarted = true;
            StartCoroutine(RiseWithSinglePause());
        }
    }

    IEnumerator RiseWithSinglePause()
    {
        // Start rising
        isMoving = true;

        // Rise for moveDuration
        yield return new WaitForSeconds(moveDuration);

        // Pause once
        isMoving = false;
        yield return new WaitForSeconds(pauseDuration);

        // Resume rising (and never pause again)
        isMoving = true;
    }

    void Update()
    {
        if (isMoving)
        {
            transform.Translate(Vector2.up * riseSpeed * Time.deltaTime);
        }
    }
}
