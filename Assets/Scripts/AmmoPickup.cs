using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int ammoAmount = 7; // Số lượng đạn nhặt được, có thể chỉnh trong Inspector

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GunController gunController = collision.gameObject.GetComponentInChildren<GunController>();
        if (gunController != null)
        {
            gunController.AddAmmo(ammoAmount); // Thêm đúng số lượng đạn mong muốn
            Destroy(gameObject); // Hủy vật phẩm nhặt đạn
        }
    }
}
