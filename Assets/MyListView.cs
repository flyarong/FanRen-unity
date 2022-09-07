using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyListView : MonoBehaviour
{

    ScrollRect mScrollRect;
    GridLayoutGroup mGridLayoutGroup;

    void Start()
    {
        //GetComponent<RectTransform>().SetAsLastSibling();

        mScrollRect = GetComponent<ScrollRect>();
        mGridLayoutGroup = GetComponentInChildren<GridLayoutGroup>();

        Debug.Log("mGridLayoutGroup " + mGridLayoutGroup.flexibleHeight);
        Debug.Log("mGridLayoutGroup " + mGridLayoutGroup.preferredHeight);
    }


    public void OnCloseButtonClick()
    {
        this.gameObject.SetActive(false);
    }

}
