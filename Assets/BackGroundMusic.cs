using UnityEngine;

public class BackGroundMusic : MonoBehaviour
{
    public AudioManager am;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.am.play("BGM");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
