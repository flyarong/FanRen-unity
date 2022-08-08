using UnityEngine;
using UnityEngine.UI;

public class TextDamageController : BaseMono
{

    public GameObject damageRole;
    RectTransform rectTransform;
    //float targetY;
    Vector2 startScreenPosition;
    bool flag = false;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        //targetY = rtf.position.y + 100;

        //Debug.Log("rtf.rect.y " + rtf.rect.y);
        //Debug.Log("rtf.rect.position.y " + rtf.rect.position.y);
        //Debug.Log("rtf.position.y " + rtf.position.y);
        //Vector2 targetPosition = damageRole.transform.position;
        startScreenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, damageRole.transform.position);
        rectTransform.position = startScreenPosition;

        //targetY = screenPosition.y + 100;
    }

    float counter = 0f;

    // Update is called once per frame
    void Update()
    {
        if (flag || damageRole == null || !damageRole.activeInHierarchy || !damageRole.activeSelf) return;
        //transform.Translate(Vector2.up * Time.deltaTime * 100, Space.Self);

        //镜头运动的偏移量
        Vector2 offset = RectTransformUtility.WorldToScreenPoint(Camera.main, damageRole.transform.position) - startScreenPosition;

        //tp2.y = tp2.y + 100f * Time.deltaTime;
        //tp2.x = RectTransformUtility.WorldToScreenPoint(Camera.main, damageRole.transform.position).x;
        //startScreenPosition
        //rectTransform.position = tp2;

        //transform.Translate((Vector2.up - (offset - startScreenPosition)) * Time.deltaTime * 100, Space.Self);

        counter += 100f * Time.deltaTime;

        Vector2 targetPoint = startScreenPosition;
        targetPoint += offset;
        //targetPoint.x = targetPoint.x;
        targetPoint.y += counter;

        rectTransform.position = targetPoint;

        //if (rtf.position.y > targetY)
        //{
        //    flag = true;
        //    Destroy(this.gameObject);
        //}

        if (counter > 100f)
        {
            flag = true;
            Destroy(this.gameObject);
        }

    }

}
