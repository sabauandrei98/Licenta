using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    public GameObject debug_panel;
    private string[] game_map;
    private List<Vector3> bomb_details;
    void Start()
    {
        GameObject.FindGameObjectWithTag("DebugButton").GetComponent<Button>().onClick.AddListener(delegate { DebugMap(); });
    }

    public void DebugMap()
    {
        if (debug_panel.gameObject.activeInHierarchy)
            debug_panel.SetActive(false);
        else
        {
            debug_panel.SetActive(true);
            ShowDebugData();
        }

    }
    private void ShowDebugData()
    {
        if (game_map == null)
            return;

        string text = "";
        for (int i = 0; i < game_map.Length; i++)
        {
            for (int j = 0; j < game_map[i].Length; j++)
                text += game_map[i][j] + "  ";
            text += '\n';
        }
        for (int i = 0; i < bomb_details.Count; i++)
            text += bomb_details[i].ToString() + " ";

        debug_panel.transform.GetChild(0).GetComponent<Text>().text = text;
    }

    public void Notify(ref string[] map, ref List<Vector3> bomb_details)
    {
        this.game_map = map;
        this.bomb_details = bomb_details;
        if (debug_panel.gameObject.activeInHierarchy)
            ShowDebugData();
    }

}
