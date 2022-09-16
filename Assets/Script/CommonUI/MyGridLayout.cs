using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyGridLayout<T>
{
    GridLayoutGroup gridLayoutGroup;
    RectTransform scrollContentRectTransform;

    //public bool isNeedDefaultSelectStateColor = true;

    //格子UI prefab
    private GameObject gridItemUIPrefab;

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

    //List<T> datas = new List<T>();

    /// <summary>
    /// 从数据库查
    /// </summary>
    int dataSize = 0;

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

    /// <summary>
    /// 当前加载的最后一个数据的索引(最小值为(oneScreenNeedRow+2)*columnCount)
    /// </summary>
    int cacheGridItemLastDataIndex = -1;

    //占满一屏需要多少行
    int oneScreenNeedRow;

    //占满一屏需要多少格
    int oneScreenNeedItems;

    //滚动GameObject
    GameObject scrollContentGameObj;

    //当前已滑动的偏移量
    float scrollOffset;

    //最大允许的偏移量
    float maxScrollOffset;

    GameObject scrollRectGameObject;

    public void StartInit(GameObject scrollRectGameObject, GameObject gridItemUIPrefab, Adapter<T> adapter)
    {
        this.scrollRectGameObject = scrollRectGameObject;
        this.gridItemUIPrefab = gridItemUIPrefab;
        this.adapter = adapter;
        //this.datas = datas;
        InitDatas();
        InitUIDatas();
        InitItemCache();
        SelectActiveItem(0);
    }

    private void InitDatas()
    {
        //MyDBManager.GetInstance().ConnDB();
        //this.datas = MyDBManager.GetInstance().GetRoleItemInBag(1, false);

        this.dataSize = this.adapter.GetDataCount();

    }

    private void InitUIDatas()
    {
        gridLayoutGroup = scrollRectGameObject.GetComponentInChildren<GridLayoutGroup>();

        scrollContentGameObj = gridLayoutGroup.gameObject;

        scrollContentRectTransform = scrollContentGameObj.GetComponent<RectTransform>();

        columnCount = gridLayoutGroup.constraintCount;
        cellHeight = gridLayoutGroup.cellSize.y;
        spaceHeight = gridLayoutGroup.spacing.y;
        if (originPaddingTop == 0) originPaddingTop = gridLayoutGroup.padding.top;

        containerHeight = scrollRectGameObject.transform.rectTransform().rect.height;
        Debug.Log("containerHeight " + containerHeight);

        float containerWidth = scrollRectGameObject.transform.rectTransform().rect.width;
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
        this.dataSize = this.adapter.GetDataCount();
        InitUIDatas();

        if (this.dataSize > originDataSize) //增加了数据
        {

            GameObject lastActiveGridItem = FindLastActiveGridItem();
            if (lastActiveGridItem != null) //说明有数据
            {
                Debug.Log("lastActiveDataIndex " + lastActiveGridItem.name);
                //最后一行剩余的空位
                int lastRowNonUseCount;
                if ((int.Parse(lastActiveGridItem.name) + 1) % columnCount == 0)
                {
                    lastRowNonUseCount = 0;
                }
                else
                {
                    lastRowNonUseCount = columnCount - ((int.Parse(lastActiveGridItem.name) + 1) % columnCount);
                }
                Debug.Log("lastRowNonUseCount " + lastRowNonUseCount);
                //增加了多少数据
                int addDataCount = this.dataSize - originDataSize;
                Debug.Log("addDataCount " + addDataCount);
                if (addDataCount > lastRowNonUseCount) //增加的数据量超过剩余空位
                {
                    if (originDataSize <= (oneScreenNeedItems + 2 * columnCount))
                    {
                        Debug.Log("原数据量在范围内");
                        if (this.dataSize <= (oneScreenNeedItems + 2 * columnCount))
                        {
                            Debug.Log("范围内 增加到 范围内");
                            //cacheGridItemLastDataIndex // padding // scrollOffset // scrollAreaHeight // all cahceItem data index change // RefreshAllGridItem()
                            SetInitHeight();
                        }
                        else if (this.dataSize > (oneScreenNeedItems + 2 * columnCount))
                        {
                            Debug.Log("范围内 增加到 范围外");
                            //cacheGridItemLastDataIndex // padding // scrollOffset // scrollAreaHeight // all cahceItem data index change // RefreshAllGridItem()
                            SetInitHeight();
                        }
                    }
                    else if (originDataSize > (oneScreenNeedItems + 2 * columnCount))
                    {
                        Debug.Log("范围外 到 范围外，无需处理");
                        //cacheGridItemLastDataIndex // padding // scrollOffset // scrollAreaHeight // all cahceItem data index change // RefreshAllGridItem()
                    }
                }
                else
                {
                    Debug.Log("添加的数据量小于等于隐藏格子，无需处理");
                }
            }
            else //说明添加数据之前零数据
            {
                Debug.LogWarning("添加数据之前零数据");
                if (originDataSize != 0) Debug.LogError("数据异常 originDataSize " + originDataSize);
                SetInitHeight();
            }
            RefreshAllGridItem();

        }
        else if (this.dataSize == originDataSize) //没有增减数据
        {

            Debug.Log("数据量没变");
            RefreshAllGridItem();

        }
        else //减少了数据
        {

            Debug.Log("originDataSize " + originDataSize);
            if (originDataSize <= (oneScreenNeedItems + 2 * columnCount)) //原来数据量范围内
            {
                Debug.Log("范围内 减到 范围内");
                //cacheGridItemLastDataIndex // padding // scrollOffset // scrollAreaHeight // all cahceItem data index change // RefreshAllGridItem()
                RefreshAllGridItem();
                SetInitHeight();
            }
            else if (originDataSize > (oneScreenNeedItems + 2 * columnCount)) //原来数据量范围外
            {
                if (this.dataSize <= (oneScreenNeedItems + 2 * columnCount)) //范围外 减到 范围内
                {
                    Debug.Log("范围外 减到 范围内");

                    //cacheGridItemLastDataIndex // padding // scrollOffset // scrollAreaHeight // cahceItem data index // RefreshAllGridItem()

                    cacheGridItemLastDataIndex = (oneScreenNeedRow + 2) * columnCount - 1;

                    gridLayoutGroup.padding.top = originPaddingTop;

                    SetInitHeight();

                    AdjustGridItemDataIndexToInitState();

                    RefreshAllGridItem();

                }
                else if (this.dataSize > (oneScreenNeedItems + 2 * columnCount))
                {
                    Debug.Log("范围外 减到 范围外");
                    //gridItemLastIndex // padding // scrollOffset // scrollAreaHeight // all cahceItem data index change // RefreshAllGridItem()
                    GameObject lastActiveGridItem = FindLastActiveGridItem();
                    int lastActiveGridItemDataIndex = int.Parse(lastActiveGridItem.name);
                    if (this.dataSize <= lastActiveGridItemDataIndex) //数据量变得太小导致入侵了cache索引范围
                    {
                        Debug.Log("剩余数据量小到入侵了cache item data index最大值，需要操作UI");

                        //int reduceLineCount = GetReduceLineCount(originDataSize); //todo 逻辑有问题，如果100万数据减少到10万呢，那岂不是很多行，应该是要计算入侵了多少行?

                        int exceedCount = lastActiveGridItemDataIndex - this.dataSize + 1;

                        Debug.Log("exceedCount " + exceedCount);

                        int reduceLineCount = 0;

                        if ((int.Parse(lastActiveGridItem.name) + 1) % columnCount == 0)
                        {
                            Debug.Log("最后一行是完整排满的");
                        }
                        else
                        {
                            int lastRowActiveGridItemCount = (int.Parse(lastActiveGridItem.name) + 1) % columnCount;
                            if (exceedCount < lastRowActiveGridItemCount) //入侵的数量，最后排的 (active grid item count - 1) 足够扣减，无需减行
                            {
                                Debug.Log("入侵的数量，最后排的 (active grid item count - 1) 足够扣减，无需减行");
                            }
                            else if (exceedCount == lastRowActiveGridItemCount) //入侵的数量，最后排的 active grid item count 刚好够扣减，需减行
                            {
                                reduceLineCount++;
                            }
                            else if (exceedCount > lastRowActiveGridItemCount)
                            {
                                exceedCount -= lastRowActiveGridItemCount;
                                reduceLineCount++;
                                reduceLineCount += (exceedCount / columnCount);
                            }
                        }

                        Debug.Log("reduceLineCount " + reduceLineCount);

                        if (reduceLineCount > 0)
                        {
                            cacheGridItemLastDataIndex -= reduceLineCount * columnCount;

                            //调整滑动区域内高度
                            Vector2 sd = scrollContentRectTransform.sizeDelta;
                            sd.y -= (reduceLineCount * (cellHeight + spaceHeight));
                            scrollContentRectTransform.sizeDelta = sd;

                            gridLayoutGroup.padding.top -= (int)(reduceLineCount * (cellHeight + spaceHeight));

                            AdjustGridItemDataIndex(reduceLineCount * columnCount);
                        }
                        else
                        {
                            Debug.Log("凑不够1行，无需处理UI");
                        }
                    }
                    else
                    {
                        Debug.Log("无需处理末端UI");
                    }
                    RefreshAllGridItem();

                }
            } //原来数据量范围外

        } //减少了数据

    }

    private GameObject FindLastActiveGridItem()
    {
        LinkedListNode<GameObject> tmp = cacheItems.Last;
        while (true)
        {
            if (tmp.Value.activeInHierarchy)
            {
                return tmp.Value;
            }
            else
            {
                tmp = tmp.Previous;
            }
            if (tmp == null) break;
        }
        return null;
    }

    private void AdjustGridItemDataIndex(int adjustOffset)
    {
        Debug.Log("AdjustGridItemDataIndex()");
        LinkedListNode<GameObject> tmp = cacheItems.First;
        while (true)
        {
            if (tmp == null) break;
            int dataIndex = int.Parse(tmp.Value.name);
            dataIndex -= adjustOffset;
            tmp.Value.name = dataIndex.ToString();
            tmp = tmp.Next;
        }
    }

    private void AdjustGridItemDataIndexToInitState()
    {
        Debug.Log("AdjustGridItemDataIndexToInitState()");
        LinkedListNode<GameObject> tmp = cacheItems.First;
        int i = -1;
        while (true)
        {
            if (tmp == null) break;
            i++;
            tmp.Value.name = i.ToString();
            tmp = tmp.Next;
        }
    }

    private void RefreshAllGridItem()
    {
        Debug.Log("RefreshAllGridItem()");

        LinkedListNode<GameObject> node = cacheItems.First;
        while (true)
        {
            GameObject cacheGridItem = node.Value;
            int itemDataIndex = int.Parse(cacheGridItem.name);
            if (itemDataIndex < this.dataSize)
            {
                //RoleItem roleItem = this.datas[itemDataIndex];
                SetGridItem(cacheGridItem, itemDataIndex);
            }
            else
            {
                cacheGridItem.SetActive(false);
            }
            node = node.Next;
            if (node == null) break;
        }

        if (dataSize > 0)
        {
            int lastI = lastClickGridItemIndex;
            if (lastI >= this.dataSize)
            {
                lastI = this.dataSize - 1;
            }
            lastClickGridItemIndex = lastI;
            //todo 改到OnSelectGridItem回调中去
            //ShowItemDesc(this.datas[lastI]);
            SelectActiveItem(lastI);
        }
        else
        {
            lastClickGridItemIndex = 0;
            //ShowItemDesc(null);
            this.adapter.OnSelectGridItem(null, -1);
        }
    }

    public void OnGridItemClick(GameObject gridItem)
    {
        int clickIndex = int.Parse(gridItem.transform.name);
        Debug.LogWarning("OnGridItemClick clickIndex " + clickIndex);
        lastClickGridItemIndex = clickIndex;

        this.adapter.OnItemClick(gridItem, clickIndex);
        SelectActiveItem(clickIndex);

        //todo 改到OnSelectGridItem回调中去
        //ShowItemDesc(this.datas[clickIndex]);
    }

    private void SelectActiveItem(int targetIndex)
    {

        if (this.dataSize == 0) return;

        int _targetIndex = targetIndex;

        int firstActiveGridItemDataIndex = int.Parse(cacheItems.First.Value.name);
        GameObject lastActiveGridItem = FindLastActiveGridItem();
        int lastActiveGridItemDataIndex = int.Parse(lastActiveGridItem.name);

        //int _targetIndex = targetIndex;
        if (_targetIndex > lastActiveGridItemDataIndex || _targetIndex < firstActiveGridItemDataIndex)
        {
            return;
        }

        LinkedListNode<GameObject> node = cacheItems.First;
        do
        {
            int dataIndex = int.Parse(node.Value.name);
            if (_targetIndex == dataIndex)
            {
                if (this.adapter.IsNeedAutoSelectState()) node.Value.GetComponent<Image>().color = Color.green;
                this.adapter.OnSelectGridItem(node.Value, dataIndex);
            }
            else
            {
                if (this.adapter.IsNeedAutoSelectState()) node.Value.GetComponent<Image>().color = Color.white;
            }
        }
        while ((node = node.Next) != null);
    }

    private void SetGridItem(GameObject cacheItem, int index)
    {
        //cacheItem.GetComponentInChildren<Text>().text = roleItem.itemId + "_" + roleItem.itemName + "_" + roleItem.itemCount; //todo 测试用id
        //cacheItem.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("Images/ItemImage/" + roleItem.imageName);

        this.adapter.BindView(cacheItem, index);

        cacheItem.name = "" + index;
        cacheItem.transform.name = index.ToString();
        cacheItem.SetActive(true);
        Button bt = cacheItem.GetComponent<Button>();
        bt.onClick.RemoveAllListeners();
        bt.onClick.AddListener(() =>
        {
            OnGridItemClick(cacheItem);
        });

        if (this.adapter.IsNeedAutoSelectState())
        {
            if (index == lastClickGridItemIndex)
            {
                cacheItem.GetComponent<Image>().color = Color.green;
            }
            else
            {
                cacheItem.GetComponent<Image>().color = Color.white;
            }
        }

    }

    private void InitItemCache()
    {

        Transform gridItemParent = scrollRectGameObject.GetComponent<ScrollRect>().content;
        //if (dataSize > (oneScreenNeedItems + columnCount * 2)) //数据量超出1屏+2行
        //{

        for (int i = 0; i < (oneScreenNeedItems + columnCount * 2); i++)
        {
            cacheGridItemLastDataIndex++;
            GameObject cacheItem = GameObject.Instantiate(gridItemUIPrefab, gridItemParent);
            cacheItems.AddLast(cacheItem);
            if (cacheGridItemLastDataIndex < this.dataSize)
            {
                //RoleItem roleItem = this.datas[cacheGridItemLastDataIndex];
                SetGridItem(cacheItem, cacheGridItemLastDataIndex);
            }
            else
            {
                cacheItem.name = "" + cacheGridItemLastDataIndex;
                cacheItem.transform.name = cacheGridItemLastDataIndex.ToString();
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

    public void Update()
    {
        scrollOffset = scrollContentRectTransform.anchoredPosition.y;
        if (scrollOffset > maxScrollOffset)
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
            this.OnScrollDragUp();
        }
        else if (preScrollOffset - scrollOffset > 1 && Input.GetMouseButton(0)) //Input.GetMouseButton(0)防止scroll rect的松手自动回弹混乱逻辑
        {
            //下滑
            this.OnScrollDragDown();
        }
        preScrollOffset = scrollOffset;
    }

    private void OnScrollDragUp()
    {
        if (this.dataSize > oneScreenNeedItems + 2 * columnCount) //总数据量满足加载更多
        {
            //Debug.Log("总数据量满足加载更多");
            if (scrollOffset + (cellHeight + spaceHeight) >= (scrollContentRectTransform.sizeDelta.y - containerHeight))
            {
                //Debug.Log("离底部 1个 格子高度，可以加载更多(最后一行刚刚露出来)");
                if (cacheGridItemLastDataIndex < dataSize - 1)
                {
                    Debug.LogWarning("cacheGridItemLastDataIndex < dataSize-1 说明还有数据需要增行来显示,正式开始底部加载更多");
                    for (int i = 0; i < columnCount; i++)
                    {
                        cacheGridItemLastDataIndex++;
                        GameObject firstGO = cacheItems.First.Value;
                        firstGO.transform.SetAsLastSibling();
                        cacheItems.RemoveFirst();
                        cacheItems.AddLast(firstGO);
                        if (cacheGridItemLastDataIndex >= dataSize) //某行中，一部分超过索引
                        {
                            firstGO.name = cacheGridItemLastDataIndex.ToString();
                            firstGO.SetActive(false);
                        }
                        else
                        {
                            //RoleItem roleItem = this.datas[cacheGridItemLastDataIndex];
                            SetGridItem(firstGO, cacheGridItemLastDataIndex);
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
                    Debug.Log("数据已经全部加载完全");
                }
            }
            else
            {
                //Debug.Log("离底部还有" + (scrollContentRectTransform.sizeDelta.y - scrollOffset - containerHeight));
            }
        }
        else
        {
            //Debug.Log("总数据量不满足加载更多");
        }

        if (scrollOffset >= maxHeight - containerHeight)
        {
            Debug.Log("到了真正的底部");
        }
    }

    private void OnScrollDragDown()
    {
        if (scrollOffset <= gridLayoutGroup.padding.top + cellHeight)
        {
            //Debug.Log("还有一个(cellHeight-spaceHeight)的距离到达小顶部，也就是最上面行刚露出来，就进来执行了");
            if (gridLayoutGroup.padding.top > originPaddingTop)
            {
                //Debug.Log("padding top 高度还可以减少");
                if (int.Parse(scrollContentGameObj.transform.GetChild(0).name) > 0) //首个gridItem data index > 0
                {
                    Debug.Log("首个gridItem data index > 0，顶部可以继续加载，正式开始加载顶部");
                    for (int i = 0; i < columnCount; i++)
                    {
                        cacheGridItemLastDataIndex--;
                        GameObject lastGO = cacheItems.Last.Value;
                        lastGO.transform.SetAsFirstSibling();
                        cacheItems.RemoveLast();
                        cacheItems.AddFirst(lastGO);
                        int firstIndex = cacheGridItemLastDataIndex - ((oneScreenNeedRow + 2) * columnCount) + 1;
                        //Debug.Log("firstIndex " + firstIndex);
                        //RoleItem roleItem = this.datas[firstIndex];
                        SetGridItem(lastGO, firstIndex);
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

    private Adapter<T> adapter;

    public void SetAdapter(Adapter<T> adapter)
    {
        this.adapter = adapter;
    }

}

public abstract class Adapter<T>
{

    protected List<T> datas;

    public Adapter(List<T> datas)
    {
        this.datas = datas;
    }

    public abstract void BindView(GameObject gridItem, int index);

    public abstract void OnItemClick(GameObject gridItem, int index);

    /// <summary>
    /// 点击了会有选中，但是选中未必是点击了，有可能是之前选中的删除了，就会重新选中
    /// </summary>
    /// <param name="gridItem"></param>
    /// <param name="index"></param>
    public abstract void OnSelectGridItem(GameObject gridItem, int index);

    /// <summary>
    /// 是否需要默认的选中状态（选中背景颜色更改）
    /// </summary>
    /// <returns></returns>
    public abstract bool IsNeedAutoSelectState();

    public int GetDataCount()
    {
        return this.datas.Count;
    }
}