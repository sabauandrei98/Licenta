using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLoader : MonoBehaviour
{
    public static GameObject LoadPlayer(int x, int y, int map_size)
    {
        GameObject go = null;

        //0 - farmer
        if (x == 0 && y == 0)
            go = Resources.Load<GameObject>("KenneyGraveyard/digger");

        //1 - zombie 
        if (x == 0 && y == map_size - 1)
            go = Resources.Load<GameObject>("KenneyGraveyard/zombie");

        //2 - skeleton
        if (x == map_size - 1 && y == 0)
            go = Resources.Load<GameObject>("KenneyGraveyard/skeleton");

        //3 - human
        if (x == map_size - 1 && y == map_size - 1)
            go = Resources.Load<GameObject>("KenneyGraveyard/vampire");

        return go;
    }

    public static GameObject LoadPumpkin()
    {
        return Resources.Load<GameObject>("Prefabs/Pumpkin");
    }

    public static GameObject LoadBomb()
    {
        return Resources.Load<GameObject>("KenneyGraveyard/urn");
    }

    public static GameObject LoadObstacle(int index)
    {
        const int obstacles_size = 21;

        //get a random object
        if (index == -1)
            index = Random.Range(0, obstacles_size);

        if (index == 0)
            return Resources.Load<GameObject>("KenneyGraveyard/altarStone");

        if (index == 1)
            return Resources.Load<GameObject>("KenneyGraveyard/altarWood");

        if (index == 2)
            return Resources.Load<GameObject>("KenneyGraveyard/borderPillar");

        if (index == 3)
            return Resources.Load<GameObject>("KenneyGraveyard/coffin");

        if (index == 4)
            return Resources.Load<GameObject>("KenneyGraveyard/coffinOld");

        if (index == 5)
            return Resources.Load<GameObject>("KenneyGraveyard/columnLarge");

        if (index == 6)
            return Resources.Load<GameObject>("KenneyGraveyard/cross");

        if (index == 7)
            return Resources.Load<GameObject>("KenneyGraveyard/crossColumn");

        if (index == 8)
            return Resources.Load<GameObject>("KenneyGraveyard/crossWood");

        if (index == 9)
            return Resources.Load<GameObject>("KenneyGraveyard/crypt");

        if (index == 10)
            return Resources.Load<GameObject>("KenneyGraveyard/gravestoneBevel");

        if (index == 11)
            return Resources.Load<GameObject>("KenneyGraveyard/gravestoneBroken");

        if (index == 12)
            return Resources.Load<GameObject>("KenneyGraveyard/gravestoneCross");

        if (index == 13)
            return Resources.Load<GameObject>("KenneyGraveyard/gravestoneDebris");

        if (index == 14)
            return Resources.Load<GameObject>("KenneyGraveyard/gravestoneDecorative");

        if (index == 15)
            return Resources.Load<GameObject>("KenneyGraveyard/gravestoneRoof");

        if (index == 16)
            return Resources.Load<GameObject>("KenneyGraveyard/gravestoneRound");

        if (index == 17)
            return Resources.Load<GameObject>("KenneyGraveyard/pillarLarge");

        if (index == 18)
            return Resources.Load<GameObject>("KenneyGraveyard/pillarObelisk");

        if (index == 19)
            return Resources.Load<GameObject>("KenneyGraveyard/pillarSquare");

        if (index == 20)
            return Resources.Load<GameObject>("KenneyGraveyard/rocks");

        return null;
    }
}
