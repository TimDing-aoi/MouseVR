using UnityEngine;

public class Particles : MonoBehaviour
{
    public float lifeSpan;
    public float dist;
    public float density;
    public float p_height;
    //readonly private float baseH = 0.0185f;
    readonly private float baseH = 0.05f;
    private uint seed;
    // Start is called before the first frame update
    void Start()
    {
        seed = (uint)UnityEngine.Random.Range(1, 10000);
        PlayerPrefs.SetInt("Optic Flow Seed", (int)seed);
        lifeSpan = PlayerPrefs.GetFloat("Life Span");
        dist = PlayerPrefs.GetFloat("Draw Distance");
        density = PlayerPrefs.GetFloat("Density");
        p_height = 0.13f;//PlayerPrefs.GetFloat("Triangle Height"); // <-- this is really player height I'm just bad at naming variables
        //dist = 5.0f;
        //lifeSpan = 1;
        ParticleSystem particleSystem = GetComponent<ParticleSystem>();

        particleSystem.Stop();

        particleSystem.randomSeed = seed;

        particleSystem.Play();
        
        var main = particleSystem.main;
        var emission = particleSystem.emission;
        var shape = particleSystem.shape;

        main.startLifetime = lifeSpan;
        main.startSize = baseH;
        main.maxParticles = Mathf.RoundToInt(Mathf.Pow(dist, 2.0f) * Mathf.PI * density / 1.0f);//Mathf.Pow(t_height, 2.0f));

        emission.rateOverTime = Mathf.CeilToInt(main.maxParticles / 10000.0f) / lifeSpan * 1000.0f / (1.0f);// Mathf.Pow(t_height, 2.0f);

        shape.randomPositionAmount = dist;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
