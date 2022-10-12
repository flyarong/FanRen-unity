using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogPanelScript : MonoBehaviour
{

    public GameObject messageTextGO;
    public GameObject toggleGO;
    public GameObject okButtonGO;
    public GameObject cancelButtonGO;

    bool isToggleOn = false;

    public void SetDialog(UnityAction okCB, UnityAction cancelCB, string message, bool needToggleComfirm = false)
    {
        this.gameObject.SetActive(true);
        this.toggleGO.SetActive(needToggleComfirm);
        this.messageTextGO.GetComponent<Text>().text = message;

        if (needToggleComfirm)
        {
            this.toggleGO.GetComponent<Toggle>().SetOnValueChangedListener(delegate (bool isOn)
                {
                    isToggleOn = isOn;
                }
            );
        }

        this.okButtonGO.GetComponent<Button>().SetOnClickListener(delegate ()
        {
            if (isToggleOn)
            {
                okCB?.Invoke();
                HideDialog();
            }
        });

        this.cancelButtonGO.GetComponent<Button>().SetOnClickListener(delegate ()
        {
            cancelCB?.Invoke();
            HideDialog();
        });
    }

    public void HideDialog()
    {
        this.gameObject.SetActive(false);
    }

}

