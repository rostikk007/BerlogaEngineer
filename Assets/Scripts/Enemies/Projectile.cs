using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [Header("Основные настройки")]
    [Tooltip("Время жизни снаряда (в секундах)")]
    public float lifetime = 3f;

    [Tooltip("Радиус коллизии снаряда")]
    public float collisionRadius = 0.1f;

    [Tooltip("Минимальная скорость снаряда")]
    public float minSpeed = 5f;

    [Tooltip("Максимальная скорость снаряда")]
    public float maxSpeed = 10f;

    [Header("Настройки вращения")]
    [Tooltip("Скорость вращения в градусах в секунду")]
    public float rotationSpeed = 360f;

    [Tooltip("Плавное вращение")]
    public bool smoothRotation = true;

    [Header("Настройки следа")]
    [Tooltip("Включить след снаряда")]
    public bool useTrail = true;

    [Tooltip("Время исчезновения следа")]
    public float trailTime = 0.1f;

    [Tooltip("Начальная ширина следа")]
    public float trailStartWidth = 0.1f;

    [Tooltip("Конечная ширина следа")]
    public float trailEndWidth = 0.05f;

    [Header("Настройки анимации попадания")]
    [Tooltip("Время анимации попадания")]
    public float hitAnimationTime = 0.2f;

    [Tooltip("Множитель увеличения при попадании")]
    public float hitScaleMultiplier = 2f;

    // Приватные переменные
    private int projectileDamage;
    private float speed;
    private Transform shooter;
    private Vector2 direction;
    private TrailRenderer trailRenderer;
    private SpriteRenderer spriteRenderer;
    private bool isInitialized = false;
    private bool isHitting = false;
    private float currentRotation = 0f;
    private float targetRotation = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (useTrail)
        {
            SetupTrailRenderer();
        }

        Destroy(gameObject, lifetime);
    }

    private void SetupTrailRenderer()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
        }

        // Создаем материал для следа
        Material trailMaterial = new Material(Shader.Find("Sprites/Default"));
        trailMaterial.SetColor("_Color", Color.green);

        // Настраиваем базовые параметры
        trailRenderer.material = trailMaterial;
        trailRenderer.time = trailTime;
        trailRenderer.startWidth = trailStartWidth;
        trailRenderer.endWidth = trailEndWidth;
        trailRenderer.minVertexDistance = 0.05f;
        trailRenderer.alignment = LineAlignment.View;

        // Создаем и настраиваем градиент цвета
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.4f, 1f, 0.4f), 0.0f),  // Светло-зеленый
                new GradientColorKey(new Color(0.1f, 0.8f, 0.1f), 1.0f) // Темно-зеленый
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.7f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );

        trailRenderer.colorGradient = gradient;
        trailRenderer.enabled = false;
        StartCoroutine(EnableTrailAfterDelay());
    }

    private IEnumerator EnableTrailAfterDelay()
    {
        yield return new WaitForSeconds(0.05f);
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = true;
        }
    }

    public void Initialize(int damage, float projectileSpeed, Transform projectileShooter)
    {
        projectileDamage = damage;
        speed = projectileSpeed;
        shooter = projectileShooter;
        direction = shooter.right;
        isInitialized = true;

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }

    void Update()
    {
        if (!isInitialized || isHitting) return;

        // Движение
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Вращение
        if (smoothRotation)
        {
            currentRotation = Mathf.LerpAngle(currentRotation, targetRotation, Time.deltaTime * 10f);
            targetRotation += rotationSpeed * Time.deltaTime;
        }
        else
        {
            currentRotation += rotationSpeed * Time.deltaTime;
        }

        transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        // Проверка коллизий
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, collisionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.transform == shooter) continue;

            bool damageDealt = false;

            // Сначала проверяем конкретные компоненты
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(projectileDamage);
                damageDealt = true;
            }

            IVI21Antenna antenna = hit.GetComponent<IVI21Antenna>();
            if (antenna != null)
            {
                antenna.TakeDamage(projectileDamage);
                damageDealt = true;
            }

            Turret turret = hit.GetComponent<Turret>();
            if (turret != null)
            {
                turret.TakeDamage(projectileDamage);
                damageDealt = true;
            }

            Flamethrower flamethrower = hit.GetComponent<Flamethrower>();
            if (flamethrower != null)
            {
                flamethrower.TakeDamage(projectileDamage);
                damageDealt = true;
            }

            Barricade barricade = hit.GetComponent<Barricade>();
            if (barricade != null)
            {
                barricade.TakeDamage(projectileDamage);
                damageDealt = true;
            }

            Apiary apiary = hit.GetComponent<Apiary>();
            if (apiary != null)
            {
                apiary.TakeDamage(projectileDamage);
                damageDealt = true;
            }

            IronProcessor ironProcessor = hit.GetComponent<IronProcessor>();
            if (ironProcessor != null)
            {
                ironProcessor.TakeDamage(projectileDamage);
                damageDealt = true;
            }

            // Если был нанесен урон, проигрываем эффект попадания
            if (damageDealt)
            {
                OnHit(hit.transform.position);
                break;
            }
        }
    }

    void OnHit(Vector3 hitPosition)
    {
        if (!isHitting)
        {
            isHitting = true;

            // Проверяем тип объекта, в который попал снаряд
            Collider2D[] hits = Physics2D.OverlapCircleAll(hitPosition, collisionRadius);
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    AudioManager.Instance.PlayProjectileHitPlayer(hitPosition);
                    break;
                }
                else if (hit.CompareTag("Buildings") || hit.GetComponent<IVI21Antenna>() != null)
                {
                    AudioManager.Instance.PlayProjectileHitBuilding(hitPosition);
                    break;
                }
            }

            StartCoroutine(HitAnimation());
        }
    }

    private IEnumerator HitAnimation()
    {
        // Отключаем след
        if (trailRenderer != null)
        {
            trailRenderer.enabled = false;
        }

        Vector3 originalScale = transform.localScale;
        Color originalColor = spriteRenderer.color;
        float elapsedTime = 0;

        while (elapsedTime < hitAnimationTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / hitAnimationTime;

            // Анимация увеличения и исчезновения
            transform.localScale = originalScale * (1 + (hitScaleMultiplier - 1) * progress);
            spriteRenderer.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                1 - progress
            );

            yield return null;
        }

        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        // Отрисовка радиуса коллизии
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);

        // Отрисовка направления движения
        if (isInitialized)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, direction * 0.5f);
        }
    }
}