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

    private void UpdateStats()
    {
        GameObject.FindGameObjectWithTag(player_race + "Stats").GetComponent<Text>().text = "<color=#46CF4E>Online</color>" + "\n" +
                                                                                            "<color=#669EB0>Token: </color>" + player_name + "\n" +
                                                                                             "<color=#669EB0>Points: </color>" + pumpkins_collected.ToString();

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
        //some animation
        gameObject.transform.position = new Vector3(x, 0.1f, y);
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
