using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private int pumpkins_collected = 0;
    private bool killed = false;
    private string player_race;
    private string player_name;
    private float player_speed = 2.0f;
    private Vector3 null_vector = new Vector3(-1, -1, -1);
    private Vector3 target_position;

    void Awake()
    {
        target_position = null_vector;
    }

    private void UpdateStats()
    {
        string name = player_name;
        if (name == "singleplayer")
            name = "You";
        GameObject.FindGameObjectWithTag(player_race + "Stats").GetComponent<Text>().text = "<color=#46CF4E>Online</color>" + "\n" +
                                                                                            "<color=#669EB0>Token: </color>" + name + "\n" +
                                                                                             "<color=#669EB0>Points: </color>" + pumpkins_collected.ToString();

    }

    private void Update()
    {
        if (target_position != null_vector)
        {
            float step = player_speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target_position, step);

            if (Vector3.Distance(transform.position, target_position) < 0.01f)
            {
                transform.position = target_position;
                target_position = null_vector;
            }
        }
    }

    public void SetRace(string player_race)
    {
        this.player_race = player_race;
        UpdateStats();
    }

    public void IncreasePoints()
    {
        pumpkins_collected++;
        UpdateStats();
    }
 
    public void SetName(string player_name)
    {
        this.player_name = player_name;
    }

    public string GetName()
    {
        return player_name;
    }

    public void MovePlayerToPosition(int x, int y)
    {
        gameObject.transform.LookAt(new Vector3(x, 0, y));
        target_position = new Vector3(x, 0.1f, y);
    }

    public bool IsDead()
    {
        return killed;
    }
    public void Kill()
    {
        killed = true;
        this.gameObject.SetActive(false);
    }
}
