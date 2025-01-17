using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeInitScript : MonoBehaviour
{

    public GameObject sanShu;

    // Start is called before the first frame update
    void Start()
    {
        MyDBManager.GetInstance().ConnDB();
        //收集干柴任务进行中，且处于可提交未提交状态，且和三叔一起去青牛镇任务还没触发，则显示三叔
        if ((MyDBManager.GetInstance().GetRoleItemInBag(1).itemCount >= 5 
            && MyDBManager.GetInstance().GetRoleTask(1).taskState == (int)FRTaskState.InProgress
            && MyDBManager.GetInstance().GetRoleTask(3).taskState == (int)FRTaskState.Untrigger) 
            || MyDBManager.GetInstance().GetRoleTask(3).taskState == (int)FRTaskState.InProgress)
        {
            sanShu.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
