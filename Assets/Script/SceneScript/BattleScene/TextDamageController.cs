using UnityEngine;
using UnityEngine.UI;

public class TextDamageController : BaseMono
{

    public GameObject damageRole;
    RectTransform rtf;
    float targetY;
    Vector2 tp2;

    // Start is called before the first frame update
    void Start()
    {
        rtf = GetComponent<RectTransform>();
        //targetY = rtf.position.y + 100;

        //Debug.Log("rtf.rect.y " + rtf.rect.y);
        //Debug.Log("rtf.rect.position.y " + rtf.rect.position.y);
        //Debug.Log("rtf.position.y " + rtf.position.y);

        tp2 = RectTransformUtility.WorldToScreenPoint(Camera.main, damageRole.transform.position);
        rtf.position = tp2;

        targetY = tp2.y + 100;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.Translate(Vector2.up * Time.deltaTime * 100, Space.Self);
        tp2.y = tp2.y + 100f * Time.deltaTime;
        tp2.x = RectTransformUtility.WorldToScreenPoint(Camera.main, damageRole.transform.position).x;
        rtf.position = tp2;
        if (rtf.position.y > targetY)
        {
            Destroy(this.gameObject);
        }
    }

}
