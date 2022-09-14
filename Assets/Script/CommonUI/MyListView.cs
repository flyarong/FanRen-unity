using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class MyListView : MonoBehaviour
{

    GridLayoutGroup gridLayoutGroup;
    RectTransform scrollContentRectTransform;

    //格子UI prefab
    public GameObject bagGridItemPrefab;

    //最后一次点击的索引
    int lastClickGridItemIndex;

    /// <summary>
    /// 上一帧的偏移量
    /// </summary>
    float preScrollOffset;

    //可见滚动区域的高度(可视区域高度)
    float containerHeight = 0f;

    //滚动内容最大高度，根据数据量计算出来
    int maxHeight;

    List<RoleItem> datas = new List<RoleItem>();

    /// <summary>
    /// 从数据库查
    /// </summary>
    int dataSize = 111;

    //列数
    int columnCount;

    //每个格子的高度
    float cellHeight;

    //格子之间的垂直间隔
    float spaceHeight;

    //滚动内容的顶部间隔(初始值)
    int originPaddingTop;

    //存放格子UI
    LinkedList<GameObject> cacheItems = new LinkedList<GameObject>();

    //当前加载的最后一个数据的索引
    int gridItemLastIndex = -1;

    //占满一屏需要多少行
    int oneScreenNeedRow;

    //占满一屏需要多少格
    int oneScreenNeedItems;

    //是否数据全部一次性加载，是则不需要无限滚动
    bool isLoadAll = false;

    //滚动GameObject
    GameObject scrollContentGameObj;

    //当前已滑动的偏移量
    float scrollOffset;

    //最大允许的偏移量
    float maxScrollOffset;

    void Start()
    {
        InitDatas();
        InitUIDatas();
        InitItemCache();
        SelectItem(0);
    }

    private void InitDatas()
    {
        MyDBManager.GetInstance().ConnDB();
        this.datas = MyDBManager.GetInstance().GetRoleItemInBag(1, false);

        //this.datas.RemoveRange(0, 13);
        //this.datas.Clear();

        this.dataSize = this.datas.Count;
    }

    private void InitUIDatas()
    {
        gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>();

        scrollContentGameObj = gridLayoutGroup.gameObject;
        scrollContentRectTransform = scrollContentGameObj.GetComponent<RectTransform>();

        columnCount = gridLayoutGroup.constraintCount;
        cellHeight = gridLayoutGroup.cellSize.y;
        spaceHeight = gridLayoutGroup.spacing.y;
        if(originPaddingTop == 0) originPaddingTop = gridLayoutGroup.padding.top;

        containerHeight = transform.rectTransform().rect.height;
        Debug.Log("containerHeight " + containerHeight);

        float containerWidth = transform.rectTransform().rect.width;
        Vector2 v2 = gridLayoutGroup.spacing;
        v2.x = (containerWidth - (columnCount * gridLayoutGroup.cellSize.x)) / (columnCount + 1);
        gridLayoutGroup.spacing = v2;
        gridLayoutGroup.padding.left = (int)v2.x;
        gridLayoutGroup.padding.right = (int)v2.x;

        int totalRows = dataSize % columnCount == 0 ? dataSize / columnCount : dataSize / columnCount + 1;
        maxHeight = totalRows * (int)(cellHeight + spaceHeight) + gridLayoutGroup.padding.bottom;
        Debug.Log("finalHeight " + maxHeight);

        maxScrollOffset = maxHeight - containerHeight;
        Debug.Log("maxScrollOffset " + maxScrollOffset);

        //占满1屏需要的行数(如果刚好整除，允许不用+1，加了也没关系，只是增加了一行的缓存而已)
        oneScreenNeedRow = (int)((containerHeight - originPaddingTop) / (cellHeight + spaceHeight)) + 1;
        Debug.Log("one screen needRow " + oneScreenNeedRow);

        oneScreenNeedItems = oneScreenNeedRow * columnCount;
        Debug.Log("one screen needItems " + oneScreenNeedItems);
    }

    public void NotifyDatasetChange()
    {

        int originDataSize = this.dataSize;

        this.dataSize = this.datas.Count;
        //this.gridItemNewestNextPointer = 0;
        InitUIDatas();

        if (this.dataSize > originDataSize) //增加了数据
        {

            //最后一行剩余的空位
            int lastRowNonUseCount;
            LinkedListNode<GameObject> lastVisibleNode = cacheItems.Last;
            for (int i = 0; i < cacheItems.Count; i++)
            {
                if (lastVisibleNode != null && lastVisibleNode.Value.activeInHierarchy)
                {
                    break;
                }
                else
                {
                    lastVisibleNode = lastVisibleNode.Previous;
                }
            }
            if (lastVisibleNode != null && lastVisibleNode.Value.activeInHierarchy) //说明有数据
            {
                Debug.Log("lastVisibleNode.Value.transform.GetChild(0).name " + lastVisibleNode.Value.transform.GetChild(0).name);
                if ((1 + int.Parse(lastVisibleNode.Value.transform.GetChild(0).name)) % columnCount == 0) //最后一行没有空位，刚好排满
                {
                    lastRowNonUseCount = 0;
                }
                else
                {
                    lastRowNonUseCount = columnCount - ((1 + int.Parse(lastVisibleNode.Value.transform.GetChild(0).name)) % columnCount);
                }
                Debug.Log("lastRowNonUseCount " + lastRowNonUseCount);
                //增加了多少数据
                int addDataCount = this.dataSize - originDataSize;
                Debug.Log("addDataCount " + addDataCount);

                RefreshAllGridItem();
                if (addDataCount > lastRowNonUseCount) //增加的数据量超过剩余空位
                {

                    if(originDataSize <= (oneScreenNeedItems + 2 * columnCount))
                    {
                        Debug.Log("旧数据在范围内");
                        if (this.dataSize <= (oneScreenNeedItems + 2 * columnCount))
                        {
                            Debug.Log("范围内增加数据，只需要行变动");
                            int addLineCount;
                            int c = addDataCount - lastRowNonUseCount;
                            if (c % columnCount == 0)
                            {
                                addLineCount = c / columnCount;
                            }
                            else
                            {
                                addLineCount = c / columnCount + 1;
                            }
                            Debug.Log("addLineCount " + addLineCount);
                            Vector2 sd = scrollContentRectTransform.sizeDelta;
                            sd.y += (addLineCount * (cellHeight + spaceHeight));
                            scrollContentRectTransform.sizeDelta = sd;
                        }
                        else if (this.dataSize > (oneScreenNeedItems + 2 * columnCount))
                        {
                            Debug.Log("范围内增加数据到范围外，可能需要行变动 和 paddingTop变动");
                            SetInitHeight();
                            //if (scrollOffset >= maxHeight - containerHeight) //如果当前滚动到底部，则可以+1行
                            //{
                            //    //+1行
                            //    ScrollTouchUp();
                            //}
                        }
                    }
                    else if (originDataSize > (oneScreenNeedItems + 2 * columnCount))
                    {
                        Debug.Log("旧数据量在范围外");
                    }

                }
                else
                {
                    Debug.Log("添加的数据量小于等于隐藏格子，无需处理");
                }
            }
            else //说明添加数据之前零数据
            {
                RefreshAllGridItem();
                SetInitHeight();
            }

            
        }
        else if (this.dataSize == originDataSize) //没有增减数据
        {
            Debug.Log("数据量没变");
            RefreshAllGridItem();
        }
        else //减少了数据
        {
            Debug.Log("originDataSize " + originDataSize);

            if (originDataSize <= (oneScreenNeedItems + 2 * columnCount)) //范围内
            {
                Debug.Log("范围内 减到 范围内");
                RefreshAllGridItem();
                SetInitHeight();
            }
            else if (originDataSize > (oneScreenNeedItems + 2 * columnCount))
            {
                if (this.dataSize <= (oneScreenNeedItems + 2 * columnCount))
                {
                    Debug.Log("范围外 减到 范围内");
                    //RefreshAllGridItem();
                    //SetInitHeight();
                    //gridLayoutGroup.padding.top = originPaddingTop;
                }
                else if (this.dataSize > (oneScreenNeedItems + 2 * columnCount))
                {
                    Debug.Log("范围外 减到 范围外");
                }
            }

            ////最后一行数据量
            //int lastRowVisibleGridItemCount = originDataSize % this.columnCount;
            //if(originDataSize % this.columnCount == 0)
            //{
            //    lastRowVisibleGridItemCount = columnCount;
            //}
            //RefreshAllGridItem();
            //int reduceDataCount = originDataSize - this.dataSize;
            //Debug.Log("reduceDataCount " + reduceDataCount + ", lastRowGridItemCount " + lastRowVisibleGridItemCount);
            //if (reduceDataCount >= lastRowVisibleGridItemCount) //需要减行
            //{
            //    Debug.Log("需要减行");
            //    int c = reduceDataCount - lastRowVisibleGridItemCount; //需要减1行
            //    int lineCountForDelete = 1;
            //    if(c < columnCount)
            //    {
            //        Debug.Log("不需要+1");
            //    }
            //    else if(c == columnCount)
            //    {
            //        lineCountForDelete++;
            //    }
            //    else if (c > columnCount)
            //    {
            //        if(c % columnCount == 0)
            //        {
            //            lineCountForDelete += c / columnCount;
            //        }
            //        else
            //        {
            //            lineCountForDelete += (int)(c / columnCount + 1);
            //        }
            //    }

                //    Debug.Log("lineCountForDelete " + lineCountForDelete);



                //    Vector2 sd = scrollContentRectTransform.sizeDelta;
                //    sd.y -= (lineCountForDelete * (cellHeight + spaceHeight));

                //    if (this.dataSize > (oneScreenNeedItems + columnCount * 2)) //滚动区域
                //    {
                //        int atLeastHeight = ((int)((oneScreenNeedRow + 2) * (cellHeight + spaceHeight))) + gridLayoutGroup.padding.bottom; //占满一屏+2行的高度
                //        if(sd.y < atLeastHeight)
                //        {
                //            Debug.LogWarning("不允许减行-1");
                //        }
                //        else
                //        {
                //            Debug.LogWarning("1111");
                //            gridLayoutGroup.padding.top -= (lineCountForDelete * (int)(cellHeight + spaceHeight));

                //            scrollContentRectTransform.sizeDelta = sd;

                //            Vector2 os = scrollContentRectTransform.anchoredPosition;
                //            os.y -= (lineCountForDelete * (cellHeight + spaceHeight));
                //            scrollContentRectTransform.anchoredPosition = os;

                //        }
                //    }
                //    else
                //    {
                //        if(sd.y < originPaddingTop)
                //        {
                //            Debug.LogWarning("不允许减行-2");
                //        }
                //        else
                //        {
                //            Debug.LogWarning("2222");
                //            gridLayoutGroup.padding.top -= (lineCountForDelete * (int)(cellHeight + spaceHeight));
                //            scrollContentRectTransform.sizeDelta = sd;
                //        }
                //    }



                //}
                //else
                //{
                //    Debug.Log("减少的数据量，最后一行足够扣减，无需减行");
                //}
        }

    }

    private void RefreshAllGridItem()
    {
        Debug.Log("RefreshAllGridItem()");
        LinkedListNode<GameObject> node = cacheItems.First;
        do
        {
            GameObject cacheGridItem = node.Value;
            int itemDataIndex = int.Parse(cacheGridItem.transform.GetChild(0).name);
            if (itemDataIndex < this.dataSize)
            {
                RoleItem roleItem = this.datas[itemDataIndex];
                SetGridItem(cacheGridItem, itemDataIndex, roleItem);
            }
            else
            {
                cacheGridItem.SetActive(false);
            }
        } while ((node = node.Next) != null);

        SelectItem(lastClickGridItemIndex);
    }

    public void OnGridItemClick(GameObject gridItem)
    {
        int clickIndex = int.Parse(gridItem.transform.GetChild(0).name);
        Debug.Log("OnGridItemClick clickIndex " + clickIndex);
        lastClickGridItemIndex = clickIndex;

        LinkedListNode<GameObject> node = cacheItems.First;
        do
        {
            GameObject gridItemGO = node.Value;
            gridItemGO.GetComponent<Image>().color = Color.white;
        }
        while ((node = node.Next) != null);
        gridItem.GetComponent<Image>().color = Color.green;

        ShowItemDesc(this.datas[clickIndex]);
    }

    private void SelectItem(int targetIndex)
    {
        int _targetIndex = targetIndex;
        if (_targetIndex >= this.datas.Count)
        {
            _targetIndex = this.dataSize - 1;
        }
        LinkedListNode<GameObject> node = cacheItems.First;
        do
        {
            GameObject gridItemGO = node.Value;
            string dataIndex = gridItemGO.transform.GetChild(0).name;
            int intIndex = int.Parse(dataIndex);
            if (_targetIndex == intIndex)
            {
                OnGridItemClick(gridItemGO);
                break;
            }
        }
        while ((node = node.Next) != null);
    }

    private void SetGridItem(GameObject cacheItem, int index, RoleItem roleItem)
    {
        cacheItem.name = "cacheItem_" + index;
        cacheItem.transform.GetChild(0).name = index.ToString();
        cacheItem.SetActive(true);
        cacheItem.GetComponentInChildren<Text>().text = roleItem.itemName + "x" + roleItem.itemCount;
        //cacheItem.GetComponentInChildren<Text>().text = "x" + index;
        cacheItem.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("Images/ItemImage/" + roleItem.imageName);
        Button bt = cacheItem.GetComponent<Button>();
        bt.onClick.RemoveAllListeners();
        bt.onClick.AddListener(() =>
        {
            OnGridItemClick(cacheItem);
        });
        if(index == lastClickGridItemIndex)
        {
            cacheItem.GetComponent<Image>().color = Color.green;
        }
        else
        {
            cacheItem.GetComponent<Image>().color = Color.white;
        }
    }

    private void InitItemCache()
    {

        Transform gridItemParent = GetComponent<ScrollRect>().content;
        //if (dataSize > (oneScreenNeedItems + columnCount * 2)) //数据量超出1屏+2行
        //{

        for (int i = 0; i < (oneScreenNeedItems + columnCount * 2); i++)
        {
            gridItemLastIndex++;
            GameObject cacheItem = Instantiate(bagGridItemPrefab, gridItemParent);
            cacheItems.AddLast(cacheItem);
            if(gridItemLastIndex < this.dataSize)
            {
                RoleItem roleItem = this.datas[gridItemLastIndex];
                SetGridItem(cacheItem, gridItemLastIndex, roleItem);
            }
            else
            {
                cacheItem.name = "cacheItem_" + gridItemLastIndex;
                cacheItem.transform.GetChild(0).name = gridItemLastIndex.ToString();
                cacheItem.SetActive(false);
            }
        }
        SetInitHeight();
    }

    private void SetInitHeight()
    {
        if (dataSize > (oneScreenNeedItems + columnCount * 2))
        {
            int needHeight = ((int)((oneScreenNeedRow + 2) * (cellHeight + spaceHeight))) + gridLayoutGroup.padding.bottom; //占满一屏+2行的高度
            Debug.Log("无限循环 init all grid item height " + needHeight);
            Vector2 sd = scrollContentRectTransform.sizeDelta;
            sd.y = needHeight;
            scrollContentRectTransform.sizeDelta = sd;
        }
        else
        {
            Debug.Log("适配数据高度");
            Vector2 sd = scrollContentRectTransform.sizeDelta;
            sd.y = (dataSize % columnCount == 0 ? dataSize / columnCount : dataSize / columnCount + 1) * (cellHeight + spaceHeight) + gridLayoutGroup.padding.bottom;
            scrollContentRectTransform.sizeDelta = sd;
        }
    }

    private void Update()
    {
        scrollOffset = scrollContentRectTransform.anchoredPosition.y;
        if(scrollOffset > maxScrollOffset)
        {
            scrollOffset = maxScrollOffset;
        }
        else if (scrollOffset < 0f)
        {
            scrollOffset = 0f;
        }
        if (scrollOffset - preScrollOffset > 1 && scrollOffset > 1 && Input.GetMouseButton(0)) //向上滑动 && 垂直偏移量要大于0(1更加安全一点)避免滑到最顶部回弹导致底部自动增加行
        {
            //上滑
            this.ScrollTouchUp();
        }
        else if (preScrollOffset - scrollOffset > 1 && Input.GetMouseButton(0)) //Input.GetMouseButton(0)防止scroll rect的松手自动回弹混乱逻辑
        {
            //下滑
            this.ScrollTouchDown();
        }
        preScrollOffset = scrollOffset;
    }

    private void ScrollTouchUp()
    {
        if (this.dataSize > oneScreenNeedItems + 2 * columnCount) //总数据量满足加载更多
        {
            Debug.Log("总数据量满足加载更多");
            if (scrollOffset + (cellHeight + spaceHeight) >= (scrollContentRectTransform.sizeDelta.y - containerHeight))
            {
                Debug.Log("离底部 1个 格子高度，可以加载更多(最后一行刚刚露出来)");
                if (gridItemLastIndex < dataSize - 1)
                {
                    Debug.Log("gridItemLastIndex < dataSize-1 说明还有数据没有加载出来，正式开始加载更多");
                    for (int i = 0; i < columnCount; i++)
                    {
                        gridItemLastIndex++;
                        GameObject firstGO = cacheItems.First.Value;
                        firstGO.transform.SetAsLastSibling();
                        cacheItems.RemoveFirst();
                        cacheItems.AddLast(firstGO);
                        if (gridItemLastIndex >= dataSize) //某行中，一部分超过索引
                        {
                            firstGO.name = "cacheItem_" + gridItemLastIndex;
                            firstGO.transform.GetChild(0).name = gridItemLastIndex.ToString();
                            firstGO.SetActive(false);
                        }
                        else
                        {
                            RoleItem roleItem = this.datas[gridItemLastIndex];
                            SetGridItem(firstGO, gridItemLastIndex, roleItem);
                        }
                    }
                    Debug.Log("增加滚动区域高度，增加了paddingTop高度");
                    Vector2 sd = scrollContentRectTransform.sizeDelta;
                    sd.y += (cellHeight + spaceHeight);
                    scrollContentRectTransform.sizeDelta = sd;
                    gridLayoutGroup.padding.top += (int)(cellHeight + spaceHeight);
                }
                else
                {
                    Debug.Log("数据已经全部显示完全");
                }
            }
            else
            {
                Debug.Log("离底部还有" + (scrollContentRectTransform.sizeDelta.y - scrollOffset - containerHeight));
            }
        }
        else
        {
            Debug.Log("总数据量不满足加载更多");
        }

        if (scrollOffset >= maxHeight - containerHeight)
        {
            Debug.Log("到了真正的底部");
        }
    }

    private void ScrollTouchDown()
    {
        if (scrollOffset <= gridLayoutGroup.padding.top + cellHeight)
        {
            Debug.Log("还有一个(cellHeight-spaceHeight)的距离到达小顶部，也就是最上面行刚露出来，就进来执行了");
            if (gridLayoutGroup.padding.top > originPaddingTop)
            {
                Debug.Log("padding top 高度还可以减少");
                if (int.Parse(scrollContentGameObj.transform.GetChild(0).GetChild(0).name) > 0) //首个gridItem data index > 0
                {
                    Debug.Log("首个gridItem data index > 0，顶部可以继续加载，正式开始加载顶部");
                    for (int i = 0; i < columnCount; i++)
                    {
                        gridItemLastIndex--;
                        GameObject lastGO = cacheItems.Last.Value;
                        lastGO.transform.SetAsFirstSibling();
                        cacheItems.RemoveLast();
                        cacheItems.AddFirst(lastGO);
                        int firstIndex = gridItemLastIndex - ((oneScreenNeedRow + 2) * columnCount) + 1;
                        //Debug.Log("firstIndex " + firstIndex);
                        RoleItem roleItem = this.datas[firstIndex];
                        SetGridItem(lastGO, firstIndex, roleItem);
                    }
                    Vector2 sd = scrollContentRectTransform.sizeDelta;
                    sd.y -= (cellHeight + spaceHeight);
                    scrollContentRectTransform.sizeDelta = sd;
                    gridLayoutGroup.padding.top -= (int)(cellHeight + spaceHeight);
                }
            }
        }

        if (scrollOffset <= 0f)
        {
            Debug.Log("到了真正的顶部");
        }
    }

    public void OnCloseButtonClick()
    {
        this.gameObject.SetActive(false);
    }







    public GameObject imageGO;
    public GameObject buttonGO;
    public GameObject nameGO;
    public GameObject countGO;
    public GameObject effectDescGO;
    public GameObject itemDescGO;

    private void ShowItemDesc(RoleItem roleItem)
    {
        if(roleItem != null)
        {
            imageGO.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/ItemImage/" + roleItem.imageName);
            Button useButton = buttonGO.GetComponent<Button>();
            buttonGO.SetActive(true);
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(() => { OnUseButtonClick(roleItem); });
            nameGO.GetComponent<Text>().text = roleItem.itemName;
            countGO.GetComponent<Text>().text = "数量： " + roleItem.itemCount;
            effectDescGO.GetComponent<Text>().text = "功效：" + (roleItem.recoverHp > 0 ? " 气血+" + roleItem.recoverHp : "") + (roleItem.recoverMp > 0 ? " 灵力+" + roleItem.recoverMp : "");
            itemDescGO.GetComponent<Text>().text = roleItem.itemDesc;
        }
        else
        {
            imageGO.GetComponent<Image>().sprite = null;
            buttonGO.SetActive(false);
            nameGO.GetComponent<Text>().text = "";
            countGO.GetComponent<Text>().text = "";
            effectDescGO.GetComponent<Text>().text = "";
            itemDescGO.GetComponent<Text>().text = "";
        }
    }

    void OnUseButtonClick(RoleItem roleItem)
    {
        MyDBManager.GetInstance().ConnDB();
        MyDBManager.GetInstance().DeleteItemInBag(roleItem.itemId, 1, roleItem.itemCount);
        if (roleItem.itemCount == 1)
        {
            this.datas.Remove(roleItem);
            ShowItemDesc(null);
        }
        else
        {
            roleItem.itemCount--;
            ShowItemDesc(roleItem);
        }
        NotifyDatasetChange();

        //todo 测试，无限循环模式下，从中间删除

        Debug.Log("OnUseButtonClick()");
    }




    public void AddDataTest()
    {
        //for(int i=0; i<10; i++)
        //{
        //    RoleItem a = new RoleItem();
        //    a.itemCount = 1;
        //    a.itemDesc = "desc";
        //    a.itemName = "name";
        //    a.recoverHp = 999;
        //    this.datas.Insert(0, a);
        //}
        
        //i++;
        //a = new RoleItem();
        //a.itemCount = 2;
        //a.itemDesc = "desc";
        //a.itemName = "name" + i;
        //a.recoverHp = 999;
        //this.datas.Insert(i, a);
        //i++;
        //a = new RoleItem();
        //a.itemCount = 3;
        //a.itemDesc = "desc";
        //a.itemName = "name" + i;
        //a.recoverHp = 999;
        //this.datas.Insert(i, a);
        //i++;
        //a = new RoleItem();
        //a.itemCount = 4;
        //a.itemDesc = "desc";
        //a.itemName = "name" + i;
        //a.recoverHp = 999;
        //this.datas.Insert(i, a);
        //i++;
        //a = new RoleItem();
        //a.itemCount = 5;
        //a.itemDesc = "desc";
        //a.itemName = "name" + i;
        //a.recoverHp = 999;
        //this.datas.Insert(i, a);

        if(this.datas.Count > 0) this.datas.RemoveAt(0);

        //if (this.datas.Count > 0) this.datas.RemoveAt(0);

        NotifyDatasetChange();
    }

}
