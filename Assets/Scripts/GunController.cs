using UnityEngine;
using System.Collections; // Thêm dòng này

public class GunController : MonoBehaviour
{
    public Transform Gun;
    public Animator gunAnimator;
    Vector2 direction;
    public GameObject bullet;
    public float bulletSpeed = 10f;
    public Transform shootPoint;
    public float shootRate = 0.5f;
    float nextShootTime = 0f;
    public int currentClip, maxClip = 7, currentAmmo, maxAmmo = 30;
    public float reloadDelay = 1.5f; // Thời gian thay đạn

    bool isReloading = false;

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        direction = mousePosition - (Vector2)Gun.position;
        FaceMouse();
    }

    void FaceMouse()
    {
        Gun.transform.right = direction;
    }

    public void Shoot()
    {
        if (currentClip > 0)
        {
            GameObject bulletIns = Instantiate(bullet, shootPoint.position, shootPoint.rotation);
            bulletIns.GetComponent<Rigidbody2D>().AddForce(bulletIns.transform.right * bulletSpeed);
            gunAnimator.SetTrigger("shoot");
            Destroy(bulletIns, 2f);
            currentClip--;

            // Tự động thay đạn khi hết băng
            if (currentClip == 0 && currentAmmo > 0 && !isReloading)
            {
                StartCoroutine(ReloadCoroutine());
            }
        }
    }

    IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        gunAnimator.SetTrigger("reload");
        yield return new WaitForSeconds(reloadDelay);
        FinishReload();
    }

    public void Reload()
    {
        if (!isReloading && currentClip < maxClip && currentAmmo > 0)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    public void AddAmmo(int ammoAmount)
    {
        currentAmmo += ammoAmount;
        if (currentAmmo > maxAmmo)
        {
            currentAmmo = maxAmmo; // Ensure we don't exceed max ammo
        }

        // Nếu hết băng và có đạn dự trữ, tự động thay đạn
        if (currentClip == 0 && currentAmmo > 0 && !isReloading)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }
    public void FinishReload()
    {
        int reloadAmount = maxClip - currentClip;
        reloadAmount = (currentAmmo >= reloadAmount) ? reloadAmount : currentAmmo;
        currentClip += reloadAmount;
        currentAmmo -= reloadAmount;
        isReloading = false;
    }

}
