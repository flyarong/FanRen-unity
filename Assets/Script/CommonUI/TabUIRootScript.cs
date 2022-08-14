using UnityEngine;

public class TabUIRootScript : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject tabUIPanelGameObj;

    private bool isTabUIShowing = false;

    //如果正在显示，那么人物的移动、镜头的移动要拦截
    public bool IsTabUIShowing()
    {
        return this.isTabUIShowing;
    }

    //public GameObject tabUICamera;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            OpenTabUI();
        }
    }

    public void OpenTabUI()
    {
        Debug.Log("tab click");
        this.tabUIPanelGameObj.SetActive(!this.tabUIPanelGameObj.activeInHierarchy);
        //this.tabUICamera.SetActive(this.tabUIPanelGameObj.activeInHierarchy);
        this.isTabUIShowing = this.tabUIPanelGameObj.activeInHierarchy;
    }

}
