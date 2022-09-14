using UnityEngine;

public class UILevelChangeScript : MonoBehaviour
{

    public UILevel uiLevel;

    // Start is called before the first frame update
    void Start()
    {
        if(uiLevel == UILevel.Top)
        {
            transform.SetAsLastSibling();
        }
        else if (uiLevel == UILevel.Bottom)
        {
            transform.SetAsFirstSibling();
        }
    }

}

public enum UILevel
{
    Top, Bottom, None
}
