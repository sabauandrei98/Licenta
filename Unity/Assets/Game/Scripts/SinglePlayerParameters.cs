using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SinglePlayerParameters : MonoBehaviour
{

    private InputField bots_input_field;
    private InputField obstacles_input_field;
    private InputField pumpkins_input_field;

    void Awake()
    {
        bots_input_field = GameObject.FindWithTag("Bots_InputField").GetComponent<InputField>();
        obstacles_input_field = GameObject.FindWithTag("Obstacles_InputField").GetComponent<InputField>();
        pumpkins_input_field = GameObject.FindWithTag("Pumpkins_InputField").GetComponent<InputField>();
    }

    //add here some text changed events

    private Vector3 SinglePlayerSettings()
    {
        int AI_number = 1;
        int obstacles_number = 40;
        int pumpkins_number = 40;
        try
        {
            int aux = int.Parse(bots_input_field.text);
            if (1 <= aux && aux <= 3)
                AI_number = aux;
        }
        catch { }

        try
        {
            int aux = int.Parse(obstacles_input_field.text);
            if (1 <= aux && aux <= 80)
                obstacles_number = aux;

            obstacles_number = (obstacles_number / 4) * 4;
            if (obstacles_number < 4)
                obstacles_number = 4;
        }
        catch { }

        try
        {
            int aux = int.Parse(pumpkins_input_field.text);
            if (1 <= aux && aux <= 80)
                pumpkins_number = aux;

            pumpkins_number = (pumpkins_number / 4) * 4;
            if (pumpkins_number < 4)
                pumpkins_number = 4;
        }
        catch { }

        return new Vector3(AI_number, obstacles_number, pumpkins_number);
    }
}
