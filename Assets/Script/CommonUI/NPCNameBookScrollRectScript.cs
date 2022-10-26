using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCNameBookScrollRectScript : MonoBehaviour
{

    MyGridLayout myGridLayout;
    public GameObject gridItemPrefab;

    // Start is called before the first frame update
    void Start()
    {
        MyDBManager.GetInstance().ConnDB();
        List<NPCCollectionEntity> datas = MyDBManager.GetInstance().GetAllCollectionNPC();

        NPCBookAdapter npcBookAdapter = new NPCBookAdapter(datas, gridItemPrefab);
        myGridLayout = new MyGridLayout(this.gameObject, npcBookAdapter);
    }

    // Update is called once per frame
    void Update()
    {
        myGridLayout.Update();
    }

    class NPCBookAdapter : GridLayoutAdapter<NPCCollectionEntity>
    {
        private readonly GameObject gridItemPrefab;

        public NPCBookAdapter(List<NPCCollectionEntity> datas, GameObject gridItemPrefab) : base(datas)
        {
            this.gridItemPrefab = gridItemPrefab;
        }

        public override void BindView(GameObject gridItemView, int index)
        {
            gridItemView.GetComponentInChildren<Text>().text = datas[index].npcName;
        }

        public override GameObject GetGridItemView(int index, Transform parent)
        {
            return GameObject.Instantiate(gridItemPrefab);
        }

        public override bool IsNeedAutoSelectState()
        {
            return true;
        }

        public override void OnGridItemClick(GameObject gridItemView, int index)
        {
        }

        public override void OnGridItemSelect(GameObject gridItemView, int index)
        {
        }
    }

}


