using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Параметры пули")]
    [Tooltip("Урон, который наносит пуля.")]
    public int damage = 10;

    [Tooltip("Скорость пули.")]
    public float speed = 10f;

    [Tooltip("Радиус проверки попадания.")]
    public float hitRadius = 0.1f;

    private Vector2 direction;
    private bool isInitialized = false;

    public void Initialize(float bulletSpeed, Vector2 bulletDirection)
    {
        speed = bulletSpeed;
        direction = bulletDirection.normalized;
        isInitialized = true;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, 2f);
    }

    void Update()
    {
        if (!isInitialized) return;

        Vector2 currentPosition = transform.position;
        Vector2 nextPosition = currentPosition + direction * speed * Time.deltaTime;

        Collider2D[] hits = Physics2D.OverlapCircleAll(currentPosition, hitRadius);
        foreach (Collider2D hit in hits)
        {
            // Проверка на ближнего врага
            MeleeEnemy enemy = hit.GetComponent<MeleeEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                // Воспроизводим звук попадания по врагу
                AudioManager.Instance.PlayBulletHitEnemy(transform.position);
                Destroy(gameObject);
                return;
            }

            // Проверка на дальнего врага
            RangedEnemy enemyRanged = hit.GetComponent<RangedEnemy>();
            if (enemyRanged != null) 
            {
                enemyRanged.TakeDamage(damage);
                // Воспроизводим звук попадания по врагу
                AudioManager.Instance.PlayBulletHitEnemy(transform.position);
                Destroy(gameObject);
                return;
            }
        }

        transform.position = nextPosition;
    }

    void OnDrawGizmos()
    {
        if (isInitialized)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(direction * 0.5f));
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, hitRadius);
        }
    }
}