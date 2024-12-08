using UnityEngine;

public class FlameParticle : MonoBehaviour
{
    private float speed;
    private Vector2 direction;
    private float damage;
    private float lifetime;
    private float currentLifetime = 0f;
    private SpriteRenderer spriteRenderer;
    private Vector3 initialScale;
    private Vector2 lastPosition; // Добавляем для отслеживания предыдущей позиции

    public LayerMask enemyLayer; // Добавляем слой для врагов

    public void Initialize(float flameSpeed, Vector2 flameDirection, float flameDamage, float flameLifetime)
    {
        speed = flameSpeed;
        direction = flameDirection;
        damage = flameDamage;
        lifetime = flameLifetime;
        lastPosition = transform.position;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        float randomScale = Random.Range(0.8f, 1.2f);
        transform.localScale *= randomScale;
        initialScale = transform.localScale;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        // Сохраняем текущую позицию перед движением
        Vector2 currentPosition = transform.position;

        // Двигаем частицу
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // Проверяем попадания между предыдущей и текущей позицией
        CheckHits(currentPosition, (Vector2)transform.position);

        // Обновляем lastPosition
        lastPosition = transform.position;

        // Обновляем визуальные эффекты
        currentLifetime += Time.deltaTime;
        float lifeProgress = currentLifetime / lifetime;

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(1f, 0f, lifeProgress);
            spriteRenderer.color = color;
        }

        transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, lifeProgress);

        if (currentLifetime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    void CheckHits(Vector2 fromPosition, Vector2 toPosition)
    {
        float distance = Vector2.Distance(fromPosition, toPosition);
        Vector2 direction = (toPosition - fromPosition).normalized;

        // Используем RaycastHit2D[] для получения всех попаданий
        RaycastHit2D[] hits = Physics2D.RaycastAll(fromPosition, direction, distance, enemyLayer);

        // Визуализация луча для отладки
        Debug.DrawLine(fromPosition, toPosition, Color.red, 0.1f);

        foreach (RaycastHit2D hit in hits)
        {
            // Проверяем, что мы попали во что-то
            if (hit.collider != null)
            {
                MeleeEnemy enemy = hit.collider.GetComponent<MeleeEnemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        // Визуализация пути частицы для отладки
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}