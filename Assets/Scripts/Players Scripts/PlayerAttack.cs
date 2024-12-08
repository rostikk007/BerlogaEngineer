using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackCooldown = 0.5f;
    public int attackDamage = 4;
    public KeyCode attackKey = KeyCode.Space;

    [Header("Dash Settings")]
    public float dashDistance = 2f;
    public float dashDuration = 0.1f;
    public float dashReturnDuration = 0.05f;
    public AnimationCurve dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visual Feedback")]
    public Color attackColor = Color.white;
    public float attackFlashDuration = 0.1f;

    private float nextAttackTime;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isAttacking = false;
    private Vector3 originalScale;
    private Rigidbody2D rb;
    private MonoBehaviour damagedEnemy; // Изменено для поддержки обоих типов врагов

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        originalScale = transform.localScale;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(attackKey) && Time.time >= nextAttackTime && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        // Сбрасываем поражённого врага в начале новой атаки
        damagedEnemy = null;

        // Сохраняем начальную позицию и вычисляем конечную
        Vector3 startPosition = transform.position;
        Vector3 dashDirection = transform.right;
        Vector3 endPosition = startPosition + dashDirection * dashDistance;

        // Начинаем выпад
        float elapsedTime = 0f;

        // Растягиваем спрайт в направлении движения
        Vector3 stretchScale = new Vector3(
            originalScale.x * 1.2f,  // Растягиваем по X
            originalScale.y * 0.8f,  // Сжимаем по Y
            originalScale.z
        );

        // Запускаем визуальный эффект атаки
        StartCoroutine(AttackFlash());

        // Выпад вперед
        while (elapsedTime < dashDuration)
        {
            float t = elapsedTime / dashDuration;
            float curvedT = dashCurve.Evaluate(t);

            // Интерполируем позицию
            transform.position = Vector3.Lerp(startPosition, endPosition, curvedT);

            // Интерполируем масштаб
            transform.localScale = Vector3.Lerp(originalScale, stretchScale, curvedT);

            elapsedTime += Time.deltaTime;

            // Проверяем попадание по врагу во время выпада
            CheckAndDamageEnemies();

            yield return null;
        }

        // Возвращаемся назад
        elapsedTime = 0f;
        Vector3 dashEndPosition = transform.position;

        while (elapsedTime < dashReturnDuration)
        {
            float t = elapsedTime / dashReturnDuration;
            float curvedT = dashCurve.Evaluate(t);

            // Интерполируем позицию назад
            transform.position = Vector3.Lerp(dashEndPosition, startPosition, curvedT);

            // Возвращаем нормальный масштаб
            transform.localScale = Vector3.Lerp(stretchScale, originalScale, curvedT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Убеждаемся, что вернулись точно в начальную позицию и масштаб
        transform.position = startPosition;
        transform.localScale = originalScale;

        isAttacking = false;
    }

    void CheckAndDamageEnemies()
    {
        // Если уже нанесли урон врагу в этой атаке, пропускаем проверку
        if (damagedEnemy != null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);

        float closestDistance = float.MaxValue;
        MonoBehaviour closestEnemy = null;

        // Находим ближайшего врага в конусе атаки
        foreach (Collider2D enemy in hitEnemies)
        {
            MeleeEnemy meleeEnemy = enemy.GetComponent<MeleeEnemy>();
            RangedEnemy rangedEnemy = enemy.GetComponent<RangedEnemy>();

            if (meleeEnemy != null || rangedEnemy != null)
            {
                Vector2 direction = (enemy.transform.position - transform.position).normalized;
                float angle = Vector2.Angle(transform.right, direction);

                if (angle <= 45f)
                {
                    float distance = Vector2.Distance(transform.position, enemy.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = meleeEnemy != null ? (MonoBehaviour)meleeEnemy : rangedEnemy;
                    }
                }
            }
        }

        // Наносим урон ближайшему врагу
        if (closestEnemy != null)
        {
            if (closestEnemy is MeleeEnemy meleeEnemy)
            {
                meleeEnemy.TakeDamage(attackDamage);
            }
            else if (closestEnemy is RangedEnemy rangedEnemy)
            {
                rangedEnemy.TakeDamage(attackDamage);
            }
            damagedEnemy = closestEnemy;
        }
    }

    private IEnumerator AttackFlash()
    {
        spriteRenderer.color = attackColor;
        yield return new WaitForSeconds(attackFlashDuration);
        spriteRenderer.color = originalColor;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Рисуем направление и дистанцию выпада
        Gizmos.color = Color.blue;
        Vector3 dashEnd = transform.position + transform.right * dashDistance;
        Gizmos.DrawLine(transform.position, dashEnd);

        // Рисуем конус атаки
        Vector3 rightDir = transform.right * attackRange;
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, 0, 45) * rightDir);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, 0, -45) * rightDir);
    }
}