using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetonateObject : MonoBehaviour
{
    private float remove_effect_object_time = 4.0f;

    public void Detonate(string effect)
    {
        if (effect == "pumpkin")
        {
            GameObject go = Instantiate(ResourceLoader.LoadPumpkinExplosionParticles(), this.gameObject.transform.position, Quaternion.identity) as GameObject;
            Destroy(go, remove_effect_object_time);
        }

        if (effect == "bomb")
        {
            GameObject go = Instantiate(ResourceLoader.LoadBombExplosionParticles(), this.gameObject.transform.position, Quaternion.identity) as GameObject;
            Destroy(go, remove_effect_object_time);
        }
        Destroy(this.gameObject);
    }
}
