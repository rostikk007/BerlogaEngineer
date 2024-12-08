using UnityEngine;

public class ActiveWeapon : MonoBehaviour
{
    public static ActiveWeapon Instance { get; private set; }
    [SerializeField] private Sword sword;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Player.Instance.IsAlive())
        {
            FollowMousePosition();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Sword GetActiveWeapon()
    {
        return sword;
    }

    private void FollowMousePosition()
    {
        Vector3 mousePos = GameImput.Instance.GetMousePosition();
        Vector3 playerPosition = Player.Instance.GetPlayerScreenPosition();
        if (mousePos.x < playerPosition.x)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
