using UnityEngine;

public class BGMMap2 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       AudioManager.instance.Play("BGM");
    }

}
