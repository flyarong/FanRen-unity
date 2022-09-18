using UnityEngine;
using UnityEngine.UI;

public class BattleTipsScript : MonoBehaviour
{
    // Start is called before the first frame update

    Image bgImageView;
    Text tipsTextView;
    bool doShow = false;
    bool doHide = false;
    float speed = 0.02f;

    void Start()
    {
        tipsTextView = transform.GetChild(0).GetComponent<Text>();
        Color c = tipsTextView.color;
        c.a = 0f;
        tipsTextView.color = c;

        bgImageView = this.GetComponent<Image>();
        c = bgImageView.color;
        c.a = 0f;
        bgImageView.color = c;

        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (doShow)
        {
            Color c = tipsTextView.color;
            c.a += speed;
            if (c.a >= 1f)
            {
                doShow = false;
                Invoke(nameof(HideTips), 1.2f);
                return;
            }
            tipsTextView.color = c;

            c = bgImageView.color;
            c.a += speed;
            bgImageView.color = c;
        }
        if (doHide)
        {
            Color c = tipsTextView.color;
            c.a -= speed;
            if (c.a <= 0f)
            {
                doHide = false;
                this.gameObject.SetActive(false);
                return;
            }
            tipsTextView.color = c;

            c = bgImageView.color;
            c.a -= speed;
            bgImageView.color = c;
        }
    }

    public void ShowTips(string text)
    {
        this.gameObject.SetActive(true);
        this.doShow = true;
        this.doHide = false;
    }

    private void HideTips()
    {
        this.doShow = false;
        this.doHide = true;
    }

}
