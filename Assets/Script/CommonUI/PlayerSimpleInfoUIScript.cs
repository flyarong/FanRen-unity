using UnityEngine;
using UnityEngine.UI;

public class PlayerSimpleInfoUIScript : MonoBehaviour
{

    public GameObject sliderHPGO;
    public GameObject sliderMPGO;

    public GameObject nameBookGO;
    public GameObject placeBookGO;

    // Start is called before the first frame update
    void Start()
    {
        UpdateHPAndMp();
    }

    public void UpdateHPAndMp()
    {
        MyDBManager.GetInstance().ConnDB();
        RoleInfo roleInfo = MyDBManager.GetInstance().GetRoleInfo(1);

        Slider slider = sliderHPGO.GetComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = roleInfo.maxHp;
        slider.value = roleInfo.currentHp;
        sliderHPGO.GetComponentInChildren<Text>().text = roleInfo.currentHp + "/" + roleInfo.maxHp;

        slider = sliderMPGO.GetComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = roleInfo.maxMp;
        slider.value = roleInfo.currentMp;
        sliderMPGO.GetComponentInChildren<Text>().text = roleInfo.currentMp + "/" + roleInfo.maxMp;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.E))
        {
            OnNameBookButtonClick();
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            OnPlaceBookButtonClick();
        }
    }


    public void OnNameBookButtonClick()
    {
        nameBookGO.SetActive(!nameBookGO.activeInHierarchy);
    }
    public void OnPlaceBookButtonClick()
    {
        placeBookGO.SetActive(!placeBookGO.activeInHierarchy);
    }



    public void OnCloseNameBookButtonClick()
    {
        nameBookGO.SetActive(false);
    }
    public void OnClosePlaceBookButtonClick()
    {
        placeBookGO.SetActive(false);
    }

}
