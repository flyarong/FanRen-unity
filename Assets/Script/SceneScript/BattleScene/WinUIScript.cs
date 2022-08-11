using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinUIScript : MonoBehaviour
{

    public GameObject winTextGO;
    public GameObject winTextGO2;
    public GameObject gridLayout;
    public List<GameObject> imageViewGameObjs;
    public GameObject moreGainTextGameObj;

    public void ShowGainWupin()
    {
        if (RootBattleInit.enemyRoleIds != null)
        {
            MyDBManager dBManager = MyDBManager.GetInstance();
            dBManager.ConnDB();

            Dictionary<int, int> itemId_count = new Dictionary<int, int>();

            for (int i = 0; i < RootBattleInit.enemyRoleIds.Length; i++)
            {

                int count = RootBattleInit.countOfEnemyRole[i];//该兵种的数量
                int roleId = RootBattleInit.enemyRoleIds[i];//兵种id
                RoleInfo roleInfo = dBManager.GetRoleInfo(roleId);

                List<float> gainSuccPercentList = roleInfo.CanGetItemIdPercentList();
                List<int> itemIds = roleInfo.CanGetItemIdList();

                for (int j=0; j< gainSuccPercentList.Count; j++)
                {
                    float randomF = Random.Range(0f, 1f);
                    if (randomF <= gainSuccPercentList[j]) //判定获得物品
                    {
                        int gainItemId = itemIds[j];
                        int c = itemId_count.GetValueOrDefault(gainItemId, 0);
                        if(c == 0)
                        {
                            itemId_count.TryAdd(gainItemId, 1 * count);//计算每样物品的数量
                        }
                        else
                        {
                            itemId_count[gainItemId] = c + 1 * count;//计算每样物品的数量
                        }
                    }
                    else
                    {
                        Debug.Log("没有获得战利品 -- 敌人 roleId " + roleId);
                    }
                }
            }

            Dictionary<int, int>.KeyCollection allKeys = itemId_count.Keys;
            if(allKeys.Count > 0) //有战利品
            {
                gridLayout.SetActive(true);
                int k = 0;
                foreach (int itemId in allKeys) //k : 0-13
                {
                    RoleItem itemInfo = dBManager.GetItemDetailInfo(itemId);
                    Sprite itemImage = Resources.Load<Sprite>("Images/ItemImage/" + itemInfo.imageName);
                    //Debug.LogError("k " + k);
                    imageViewGameObjs[k].SetActive(true);
                    if (itemImage != null)
                    {
                        imageViewGameObjs[k].GetComponent<Image>().sprite = itemImage;
                    }
                    else
                    {
                        Debug.LogError("该道具没有头像图片，需要补全。 itemId " + itemId);
                        if (PlayerControl.IS_DEBUG)
                        {
                            imageViewGameObjs[k].GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/ItemImage/yuJian");
                        }
                    }
                    int itemCount = itemId_count[itemId];
                    string forText = itemInfo.itemName + (itemCount > 1 ? ("X" + itemCount) : "");
                    imageViewGameObjs[k].GetComponentInChildren<Text>().text = forText;

                    //存入储物袋
                    dBManager.AddItemToBag(itemId, itemCount);

                    k++;
                    if (k == 14)
                    {
                        moreGainTextGameObj.SetActive(true);
                        moreGainTextGameObj.GetComponent<Text>().text = "...等" + (allKeys.Count-14) + "种战利品";
                        break;
                    }
                }
            }
            else
            {
                winTextGO2.GetComponent<Text>().text = "战利品：无";
                Debug.LogError("没有战利品");
            }

        }
        else
        {
            Debug.LogError("RootBattleInit.enemyRoleIds is null, 如果是从战斗场景启动的请忽略本日志");
        }
    }

    public void ShowFailUI()
    {
        winTextGO.GetComponent<Text>().text = "战斗失败，身死道消";
        winTextGO2.SetActive(false);
        gridLayout.SetActive(false);
    }

}
