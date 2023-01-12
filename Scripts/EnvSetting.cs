using UnityEngine;

[System.Serializable]
public struct EnvSound
{
    public string name;
    public AudioClip sound;
    public float distance;
    public GameObject[] gameObjects;
}

[RequireComponent(typeof(AudioSource))]
public class EnvSetting : MonoBehaviour
{
  
    [Header("DefaultSettings")]
    [SerializeField] GameObject defaultGameobject;
    [SerializeField] AudioClip defaultClip;
    [SerializeField] float defaultDistance = 30.0f;
    [SerializeField, Range(0.1f, 1.0f)] float defaultVolume = 0.1f;

    [Space(20)]
    [Header("AdditionalSounds")]
    [SerializeField] EnvSound[] envSounds;

    AudioSource audioSource;
    GameObject Player;

    //////////////////////////////////////////////////////////////////////////////////////////////	
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.clip = defaultClip;
        audioSource.volume = defaultVolume;
        audioSource.loop = true;

        if (defaultClip!=null) audioSource.Play();

        Player = GameObject.Find("Player");
    }

    void Start()
    {
        for (int i = 0; i < envSounds.Length; i++)
        {
            GameObject[] temp = envSounds[i].gameObjects;

            float distemp = envSounds[i].distance;

            for (int j = 0; j < temp.Length; j++)
            {
                temp[j].AddComponent<AudioSource>();

                audioSource = temp[j].GetComponent<AudioSource>();

                audioSource.clip = envSounds[i].sound;
                audioSource.loop = true;
                audioSource.playOnAwake = false;
                audioSource.volume = 0;
                audioSource.spatialBlend = 1.0f;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.maxDistance = distemp;
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////	
    void Update()
    {
        for (int i = 0; i < envSounds.Length; i++)
        {
            GameObject[] temp = envSounds[i].gameObjects;

            float distemp = envSounds[i].distance;
            for (int j = 0; j < temp.Length; j++)
            {
                audioSource = temp[j].GetComponent<AudioSource>();

                float distance = (Player.transform.position - temp[j].transform.position).magnitude;

                if (distance < distemp)
                {
                    if (!audioSource.isPlaying) audioSource.Play();

                    if (PlayerSynthesis.isInside)
                    {
                        audioSource.pitch = Mathf.Lerp(audioSource.pitch, 0.8f, Time.deltaTime);
                        audioSource.volume = (distemp - 1.5f*distance) / distemp;
                    }
                    else
                    {
                        audioSource.pitch = Mathf.Lerp(audioSource.pitch, 1.2f, Time.deltaTime);
                        audioSource.volume = (distemp - distance) / distemp;
                    }
                }

                else audioSource.Pause();
            }

            audioSource = GetComponent<AudioSource>();

            if (PlayerSynthesis.isInside) audioSource.volume = Mathf.Lerp(audioSource.volume, defaultVolume / 2, Time.deltaTime);
            else audioSource.volume = Mathf.Lerp(audioSource.volume, defaultVolume, Time.deltaTime);

            float dis = (defaultGameobject.transform.position - Player.transform.position).magnitude;

            if (dis < defaultDistance)
            {
                audioSource.volume = 1.5f * (defaultDistance - dis) / (10 * defaultDistance) + defaultVolume;
                audioSource.pitch = 3.5f * (defaultDistance - dis) / (10 * defaultDistance) + 1.0f;
            }
        }
        }
    }
