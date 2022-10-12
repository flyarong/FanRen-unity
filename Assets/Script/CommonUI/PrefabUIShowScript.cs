using UnityEngine;

public class PrefabUIShowScript : MonoBehaviour
{

    public GameObject prefabUI;

    public void Show()
    {
        prefabUI.SetActive(true);
    }

    public void Hide()
    {
        prefabUI.SetActive(false);
    }

}
