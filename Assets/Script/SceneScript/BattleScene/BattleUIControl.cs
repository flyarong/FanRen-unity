using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleUIControl : BaseMono
{

    private BaseRole selectedRoleCS;

    private GameObject passButton;
    private GameObject resetButton;
    private GameObject[] buttons = new GameObject[12];

    private List<GameObject> allRole;

    public GameObject battleUIPanel;
    public GameObject winUIGameObj;

    //private List<SlideAvatarController> allSlideAvatarCS = new List<SlideAvatarController>();

    public void Init(List<GameObject> allRole)
    {

        this.allRole = allRole;
        passButton = GameObject.FindGameObjectWithTag("PassButton");
        resetButton = GameObject.FindGameObjectWithTag("ResetButton");
        passButton.SetActive(false);
        resetButton.SetActive(false);

        for (int i = 0; i < 12; i++)
        {                        
            buttons[i] = GameObject.Find("st" + i);
            buttons[i].SetActive(false);
            //ColorBlock cb = buttons[i].GetComponent<Button>().colors;
            //cb.selectedColor = Color.red; 修改无效，很奇怪
        }

        
        GameObject sliderAvatarPrefab = Resources.Load<GameObject>("Prefab/UIPrefab/SliderAvatar");
        GameObject avatarParent = GameObject.FindGameObjectWithTag("SliderForAction");
        float parentWidth = avatarParent.GetComponent<RectTransform>().rect.width;
        float parentHeight = avatarParent.GetComponent<RectTransform>().rect.height;

        foreach (GameObject roleGO in allRole)
        {
            //头像移动代码
            BaseRole roleCS = roleGO.GetComponent<BaseRole>();
            GameObject sliderAvatarGO = Instantiate(sliderAvatarPrefab, avatarParent.transform);
            SlideAvatarController slideAvatarController = sliderAvatarGO.GetComponent<SlideAvatarController>();
            //todo 头像滑动速度公式待定
            slideAvatarController.speed = roleCS.speed / 5f;
            sliderAvatarGO.GetComponent<Image>().sprite = Resources.Load<Sprite>(roleCS.roleAvatar);
            if (roleCS.teamNum == TeamNum.TEAM_ONE)
            {
                //sliderAvatarGO.GetComponent<Image>().color = Color.blue;
                sliderAvatarGO.transform.position = new Vector2(avatarParent.transform.position.x - parentWidth / 2, avatarParent.transform.position.y - parentHeight / 2);
            }
            else
            {
                //sliderAvatarGO.GetComponent<Image>().color = Color.red;
                sliderAvatarGO.transform.position = new Vector2(avatarParent.transform.position.x - parentWidth / 2, avatarParent.transform.position.y + parentHeight / 2);
            }
            slideAvatarController.roleGO = roleGO;
            //allSlideAvatarCS.Add(slideAvatarController);


            //初始化血条代码
            GameObject hpSliderPrefab = Resources.Load<GameObject>("Prefab/UIPrefab/HP_Slider");

            GameObject hpSlideGameObject = Instantiate(hpSliderPrefab, this.transform);
            //hpSlideGameObject.name = roleCS.GetHpUIGameObjectName();

            Text roleName = hpSlideGameObject.GetComponentsInChildren<Text>()[0];
            roleName.text = roleCS.roleName;

            HPRotation hpRotation = hpSlideGameObject.GetComponent<HPRotation>();
            hpRotation.target = roleGO;

            Slider hpSlider = hpSlideGameObject.GetComponent<Slider>();
            hpSlider.maxValue = roleCS.maxHp;
            hpSlider.minValue = 0;
            hpSlider.value = roleCS.hp;

            roleCS.hpGO = hpSlideGameObject;
            roleCS.sliderAvatarGO = sliderAvatarGO;

            Slider mpSlider = hpSlideGameObject.transform.Find("MP_Slider").GetComponent<Slider>();
            mpSlider.maxValue = roleCS.maxMp;
            mpSlider.minValue = 0;
            mpSlider.value = roleCS.mp;
        }




    }

    //public void OnRoleSelected(BaseRole selectedRoleCS)
    //{
    //    this.selectedRoleCS = selectedRoleCS;
    //    ShowAndHideShentongButton();
    //    passButton.SetActive(true);
    //    resetButton.SetActive(true);
    //}

    private bool isWin;

    //战斗结束回调
    public void OnBattleEnd(bool isWin)
    {
        this.isWin = isWin;
        winUIGameObj.transform.SetParent(null); //为了让结束界面在最上面
        winUIGameObj.transform.SetParent(this.transform);
        winUIGameObj.SetActive(true);

        WinUIScript battleEndUIScript = winUIGameObj.GetComponent<WinUIScript>();

        if (isWin)
        {
            //战利品显示
            battleEndUIScript.ShowGainWupin();
        }
        else
        {
            Camera.main.GetComponent<SceneMusic>().StopBGM();
            //todo 播放小段悲惨音乐
            MyAudioManager audioManager = MyAudioManager.GetInstance();
            audioManager.PlaySE("SoundEff/battleFail");
            battleEndUIScript.ShowFailUI();
        }
    }

    BattleController battleController;

    void Start()
    {
        damageTextPrefab = Resources.Load<GameObject>("Prefab/UIPrefab/TextDamage");
        battleController = GameObject.FindGameObjectWithTag("Terrain").GetComponent<BattleController>();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            OnCloseWinUI();
        }
    }

    public void OnCloseWinUI()
    {
        if (winUIGameObj.activeInHierarchy && winUIGameObj.activeSelf)
        {
            if (isWin)
            {
                int lastSceneIndex = SaveUtil.GetLastSceneBuildIndex();
                if (lastSceneIndex >= 0)
                {
                    SceneManager.LoadScene(lastSceneIndex);
                }
                else
                {
                    Debug.LogError("lastSceneIndex < 0， 如果是从战斗场景启动，请忽略本log");
                }
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    public void OnChangeRoleAction(GameObject activingRoleGO)
    {
        foreach (GameObject roleGO in this.allRole)
        {
            if (roleGO == null || !roleGO.activeInHierarchy || !roleGO.activeSelf) continue;
            BaseRole baseRole = roleGO.GetComponent<BaseRole>();
            SlideAvatarController sac = baseRole.sliderAvatarGO.GetComponent<SlideAvatarController>();
            sac.PauseRun();
        }
        //foreach (SlideAvatarController sac in allSlideAvatarCS)
        //{
        //    if (sac.gameObject.activeInHierarchy && sac.gameObject.activeSelf) sac.PauseRun();
        //}

        this.selectedRoleCS = activingRoleGO.GetComponent<BaseRole>();
        if(selectedRoleCS.GetActionStrategy() != null) //有AI
        {
            this.battleUIPanel.SetActive(false);
        }
        else //无AI
        {
            this.battleUIPanel.SetActive(true);
            ShowAndHideShentongButton();
            passButton.SetActive(true);
            resetButton.SetActive(true);
        }
        
    }

    

    //点击待机按钮，会执行此方法
    public void OnClickPassButton()
    {
        if (battleController.isPlayingAnim)
        {
            return;
        }
        else
        {
            battleController.OnClickPassAllowInvokeOutsideOnly();
        }

        HideAllShentongButton();
        passButton.SetActive(false);
        resetButton.SetActive(false);
        //foreach (SlideAvatarController sac in allSlideAvatarCS)
        //{
        //    if (sac.gameObject.activeInHierarchy && sac.gameObject.activeSelf) sac.RePlayRun();
        //}

        KeepSliderAvatarGoing();
    }

    public void KeepSliderAvatarGoing()
    {
        foreach (GameObject roleGO in this.allRole)
        {
            if (roleGO == null || !roleGO.activeInHierarchy || !roleGO.activeSelf) continue;
            BaseRole baseRole = roleGO.GetComponent<BaseRole>();
            SlideAvatarController sac = baseRole.sliderAvatarGO.GetComponent<SlideAvatarController>();
            sac.RePlayRun();
        }
    }

    public void OnClickResetButton()
    {
        if (battleController.isPlayingAnim)
        {
            return;
        }
        else
        {
            battleController.OnClickReset();
        }
    }

    private void HideAllShentongButton()
    {
        if(buttons != null && buttons.Length > 0)
        {
            foreach (GameObject bt in buttons)
            {
                bt.SetActive(false);
            }
        }
    }

    public void ShowAndHideShentongButton()
    {
        if (selectedRoleCS == null || selectedRoleCS.shentongInBattle == null)
        {
            Debug.LogError(selectedRoleCS.name + "shentongInBattle not inited or selectedRoleCS is null");
            return;
        }        
        for (int i=0; i<12; i++)
        {            
            GameObject shentongButtonGO = buttons[i];
            if (selectedRoleCS.shentongInBattle[i] != null)
            {
                buttons[i] = shentongButtonGO;
                shentongButtonGO.SetActive(true);
                Text buttonText = shentongButtonGO.GetComponentInChildren<Text>();
                buttonText.text = selectedRoleCS.shentongInBattle[i].shenTongName;
            }
            else
            {
                buttons[i] = shentongButtonGO;
                shentongButtonGO.SetActive(false);
            }
        }
    }


    //GameObject uiParent = GameObject.FindGameObjectWithTag("UI_Canvas");
    GameObject damageTextPrefab;

    public void ShowDamageTextUI(int damageText, GameObject targetGO)
    {
        GameObject damageTextGO = Instantiate(this.damageTextPrefab, this.transform);
        damageTextGO.GetComponent<TextDamageController>().damageRole = targetGO;
        damageTextGO.GetComponent<Text>().text = "-" + damageText;

        //Vector2 tp2 = RectTransformUtility.WorldToScreenPoint(Camera.main, targetGO.transform.position);
        //damageTextGO.GetComponent<RectTransform>().position = tp2;
    }

    private void changeButtonColor(int clickButtonIndex)
    {
        //for(int i=0; i<12; i++)
        //{
        //    if(i== clickButtonIndex)
        //    {
        //        ColorBlock cb = buttons[i].GetComponent<Button>().colors;
        //        cb.selectedColor = Color.red;
        //    }
        //    else
        //    {
        //        ColorBlock cb = buttons[i].GetComponent<Button>().colors;
        //        cb.selectedColor = Color.clear;
        //    }
        //}
    }

    public void OnClickShentong0()
    {
        if (battleController.isPlayingAnim) return;
        selectedRoleCS.OnSelectShentong(0);
        changeButtonColor(0);
    }

    public void OnClickShentong1()
    {
        if (battleController.isPlayingAnim) return;
        selectedRoleCS.OnSelectShentong(1);
        changeButtonColor(1);
    }

    public void OnClickShentong2()
    {
        if (battleController.isPlayingAnim) return;
        selectedRoleCS.OnSelectShentong(2);
        changeButtonColor(2);
    }

    public void OnClickShentong3()
    {
        if (battleController.isPlayingAnim) return;
        selectedRoleCS.OnSelectShentong(3);
        changeButtonColor(3);
    }

    public void OnClickShentong4()
    {
        if (battleController.isPlayingAnim) return;
        selectedRoleCS.OnSelectShentong(4);
        changeButtonColor(4);
    }

    public void OnClickShentong5()
    {
        if (battleController.isPlayingAnim) return;
        selectedRoleCS.OnSelectShentong(5);
        changeButtonColor(5);
    }

    public void OnClickShentong6()
    {
        if (battleController.isPlayingAnim) return;
        selectedRoleCS.OnSelectShentong(6);
        changeButtonColor(6);
    }

    public void OnClickShentong7()
    {
        if (battleController.isPlayingAnim) return;
        selectedRoleCS.OnSelectShentong(7);
        changeButtonColor(7);
    }

    public void OnClickShentong8()
    {
        if (battleController.isPlayingAnim) return;
        selectedRoleCS.OnSelectShentong(8);
        changeButtonColor(8);
    }

    public void OnClickShentong9()
    {
        if (battleController.isPlayingAnim) return;
        selectedRoleCS.OnSelectShentong(9);
        changeButtonColor(9);
    }

    public void OnClickShentong10()
    {
        if (battleController.isPlayingAnim) return;
        selectedRoleCS.OnSelectShentong(10);
        changeButtonColor(10);
    }

    public void OnClickShentong11()
    {
        if (battleController.isPlayingAnim) return;
        selectedRoleCS.OnSelectShentong(11);
        changeButtonColor(11);
    }

    

}


