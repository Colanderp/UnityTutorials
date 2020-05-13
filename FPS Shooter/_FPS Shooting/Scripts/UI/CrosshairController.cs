using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public Crosshair crosshair;

    void Update()
    {
        crosshair.UpdateCrosshair();
    }

    public void SetCrosshair(float s, bool h)
    {
        crosshair.SetCrosshair(s, h);
    }
}

[System.Serializable]
public class Crosshair
{
    public RectTransform top;
    public RectTransform bottom;
    public RectTransform left;
    public RectTransform right;
    public Image cursor;

    float spread = 0.01f;
    bool initialized = false;
    bool hide = false;

    Color fadedColor;

    Image topImg;
    Image bottomImg;
    Image leftImg;
    Image rightImg;

    void Initialized()
    {
        topImg = top.GetComponent<Image>();
        bottomImg = bottom.GetComponent<Image>();
        leftImg = left.GetComponent<Image>();
        rightImg = right.GetComponent<Image>();

        fadedColor = Color.white;
        initialized = true;
    }

    public void SetCrosshair(float s, bool h)
    {
        hide = h;
        spread = s;
    }

    public void UpdateCrosshair()
    {
        if (!initialized)
            Initialized();
        UpdateSpread();
        UpdateFade();
    }

    void UpdateFade()
    {
        int dir = (hide) ? -1 : 1;
        fadedColor.a = Mathf.Clamp(fadedColor.a + (dir * Time.deltaTime * 2), 0.0f, 1.0f);

        cursor.color = fadedColor;
        topImg.color = fadedColor;
        bottomImg.color = fadedColor;
        leftImg.color = fadedColor;
        rightImg.color = fadedColor;
    }

    void UpdateSpread()
    {
        //1019.6x + 12
        //crosshair equation i got from plotting a bunch of spreads and their respective crosshair distances
        int crosshairDis = (int)(1019.6f * spread) + 12;
        Vector2 xPos = new Vector2(crosshairDis‬, 0);
        Vector2 yPos = new Vector2(0, crosshairDis‬);
        top.anchoredPosition = Vector2.Lerp(top.anchoredPosition, yPos, Time.deltaTime * 8f);
        bottom.anchoredPosition = Vector2.Lerp(bottom.anchoredPosition, -yPos, Time.deltaTime * 8f);
        left.anchoredPosition = Vector2.Lerp(left.anchoredPosition, xPos, Time.deltaTime * 8f);
        right.anchoredPosition = Vector2.Lerp(right.anchoredPosition, -xPos, Time.deltaTime * 8f);
    }
}
