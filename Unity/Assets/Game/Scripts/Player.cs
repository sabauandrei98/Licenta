using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    
    private string player_name;
    public int pumpkins_collected = 0;
    private GameObject bomb_obj;
    public bool killed = false;

    void Start()
    {
        bomb_obj = Resources.Load<GameObject>("KenneyGraveyard/urn");
    }
    public void Kill()
    {
        killed = true;
    }

 
    public void SetName(string player_name)
    {
        this.player_name = player_name;
    }

    public void MovePlayerToPosition(int x, int y)
    {
        //some animation
        gameObject.transform.position = new Vector3(x, 0.1f, y);
    }

    public void PlaceBomb(int x, int y)
    {
        //some particle effects
    }
}
