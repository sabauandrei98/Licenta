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

        bots_input_field.onValueChanged.AddListener(delegate { Bots_InputField_ValueChanged(); });
        obstacles_input_field.onValueChanged.AddListener(delegate { Obstacles_InputField_ValueChanged(); });
        pumpkins_input_field.onValueChanged.AddListener(delegate { Pumpkins_InputField_ValueChanged(); });
    }

    private void Bots_InputField_ValueChanged()
    {
        int AI_number = 1;
        try
        {
            int aux = int.Parse(bots_input_field.text);
            if (1 <= aux && aux <= 3)
                AI_number = aux;
        }
        catch { }
        finally { PlayerPrefs.SetInt("BotsNumber", AI_number); }
    }


    private void Obstacles_InputField_ValueChanged()
    {
        int obstacles_number = 40;
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
        finally { PlayerPrefs.SetInt("ObstaclesNumber", obstacles_number); }
    }


    private void Pumpkins_InputField_ValueChanged()
    {
        int pumpkins_number = 40;
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
        finally { PlayerPrefs.SetInt("PumpkinsNumber", pumpkins_number); }
    }

}
