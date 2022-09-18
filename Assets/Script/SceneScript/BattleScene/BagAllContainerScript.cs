using UnityEngine;

public class BagAllContainerScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickCloseButton()
    {
        DoCloseBagContainer();
    }

    public void DoCloseBagContainer()
    {
        this.gameObject.SetActive(false);
    }

}
