using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GunControllerUI : MonoBehaviour
{
    public Image gunImage;
    public Image hitmarkerImage;
    public TextMeshProUGUI ammoText;

    CrosshairController crosshair;
    GunHandler currentGun;
    float showingHit = 0;

    private void Start()
    {
        hitmarkerImage.color = new Color(1f, 1f, 1f, 0f);
        crosshair = FindObjectOfType<CrosshairController>();
    }

    private void Update()
    {
        gunImage.enabled = (currentGun != null && currentGun.gun.gunIcon != null);
        ammoText.enabled = (currentGun != null);

        if (showingHit <= 0)
        {
            Color hitColor = hitmarkerImage.color;
            hitColor.a = Mathf.Lerp(hitColor.a, 0, Time.deltaTime * 16f);
            hitmarkerImage.color = hitColor;
        }
        else
            showingHit -= Time.deltaTime;

        if (currentGun == null) return;
        ammoText.text = currentGun.ammoInClip.ToString();
        if (currentGun.gun.startingClips >= 0)
            ammoText.text += " | " + currentGun.totalAmmo;
    }

    public void UpdateGunUI(GunHandler gunHandler)
    {
        if(gunHandler.gun.gunIcon != null)
            gunImage.sprite = gunHandler.gun.gunIcon;
        ammoText.rectTransform.offsetMax = new Vector2(-gunHandler.gun.ammoOffsetX, 0);
        currentGun = gunHandler;
    }
    public void SetCrosshair(float s, bool h)
    {
        crosshair.SetCrosshair(s, h);
    }

    public void ShowHitmarker(bool killed)
    {
        hitmarkerImage.color = (killed) ? Color.red : Color.white;
        showingHit = 0.1f;
    }
}
