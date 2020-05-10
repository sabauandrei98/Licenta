using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveLight : MonoBehaviour
{
    private float timer;
    private float intensity;

    private float first_intensity = 0.5f;
    private float second_intensity = 1;

    void Start()
    {
        timer = Random.Range(0.5f, 2f);
        this.GetComponent<Light>().intensity = first_intensity;
    }
    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            timer = Random.Range(0.5f, 2f);

            if (this.GetComponent<Light>().intensity == first_intensity)
                this.GetComponent<Light>().intensity = second_intensity;
            else
                this.GetComponent<Light>().intensity = first_intensity;
        }
    }
}
