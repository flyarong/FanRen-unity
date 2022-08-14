using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSimpleInfoUIScript : MonoBehaviour
{

    public GameObject sliderHPGO;
    public GameObject sliderMPGO;

    public GameObject nameBookGO;
    public GameObject placeBookGO;

    // Start is called before the first frame update
    void Start()
    {
        
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
