using UnityEngine;

using UnityEngine;

public class music : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    public void Play()
    {
        audioSource.PlayOneShot(clip);
    }
}