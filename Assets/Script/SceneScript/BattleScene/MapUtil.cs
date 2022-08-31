using UnityEngine;

public class MapUtil
{
    public static (int, int) GetPositionFromGridItemGO(GameObject mapGridItem)
    {
        string[] position = mapGridItem.name.Split(",");
        return (int.Parse(position[0]), int.Parse(position[1]));
    }
}
