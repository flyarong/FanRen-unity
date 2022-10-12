using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIUtil
{
    public static void NotifyTaskUIDatasetChanged()
    {
        GameObject topGO = GameObject.Find("TaskScrollView");
        GameObject parentGO = GameObject.Find("TaskScrollViewContent");
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("TaskUICell");
        if (gameObjects.Length > 0)
        {
            foreach (GameObject item in gameObjects)
            {
                GameObject.Destroy(item);
            }
        }
        MyDBManager.GetInstance().ConnDB();
        List<RoleTask> roleTasks = MyDBManager.GetInstance().GetAllLeaderActorInProgressTasks();
        if (roleTasks.Count > 0)
        {
            topGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
            GameObject cellPrefab = Resources.Load<GameObject>("Prefab/UIPrefab/TaskUICell");
            foreach (RoleTask item in roleTasks)
            {
                GameObject cellGameObject = GameObject.Instantiate(cellPrefab);
                cellGameObject.GetComponent<Text>().text = item.remark;
                cellGameObject.transform.SetParent(parentGO.transform);
            }
        }
        else
        {
            topGO.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        }
    }

    public static void ShowTipsUI(string content)
    {
        TipsUIScript tmp = GameObject.Find("Panel_Tips").GetComponent<TipsUIScript>();
        tmp.AddTipsQueue(content);
        tmp.ShowTips();
    }

    public static void ShowDialog(string rootCanvasName, UnityAction okCB, UnityAction cancelCB, string message, bool needToggleComfirm = false)
    {
        DialogPanelScript dps = GameObject.Find(rootCanvasName).transform.Find("PanelDialog").GetComponent<DialogPanelScript>();
        dps.SetDialog(okCB, cancelCB, message, needToggleComfirm);
    }

}

public static class UIExtensions
{
    public static void SetOnClickListener(this Button button, UnityAction callBack)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(callBack);
    }
    
    public static void SetOnValueChangedListener(this Toggle toggle, UnityAction<bool> callBack)
    {
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(callBack);
    }

}
