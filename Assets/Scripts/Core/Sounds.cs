using UnityEngine;

public class MusicShuffler : MonoBehaviour
{
    public AudioClip[] songs;
    public AudioSource walking;
    private int nextIndex = 0;
    public PlayerMovement Jimmy;

    void Update()
    {
        if (walking == null || Jimmy == null || songs == null || songs.Length == 0) return;

        if (!walking.isPlaying && Jimmy.isMoving)
        {
            AudioClip clip = songs[nextIndex % songs.Length];
            nextIndex++;
            if (clip != null)
            {
                walking.clip = clip;
                walking.Play();
            }
        }
    }
}
