using System;
using System.Collections;
using UnityEngine;

[SelectionBase]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    public event EventHandler OnPlayerDeath;
    [SerializeField] private float _movingSpeed = 5f;
    [SerializeField] public static int _maxHealth = 10;
    [SerializeField] private float _damageRecoveryTime = 0.5f;
    private Rigidbody2D _rb;
    private KnockBack _knockBack;
    private float _minMovingSpeed = 0.1f;
    private bool _isRunning = false;
    private int _currentHealth;
    public static int _currentHealthBar;
    private bool _canTakeDamage;
    private bool _isAlive;
    Vector2 inputVector;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        Instance = this;   
        _knockBack = GetComponent<KnockBack>();

    }

    private void Start()
    {
        _canTakeDamage = true;
        _isAlive = true;
        _currentHealth = _maxHealth;
        GameImput.Instance.OnPlayerAttack += GameInput_OnPlayerAttack;
    }

    private void GameInput_OnPlayerAttack(object sender, System.EventArgs e)
    {
        ActiveWeapon.Instance.GetActiveWeapon().Attack();
    }

    private void Update()
    {
        if (GameImput.Instance != null)
        {
            inputVector = GameImput.Instance.GetMovementVector();
        }

        
    }
    private void FixedUpdate()
    {
        if (_knockBack.IsGettingKnockedBack)
        {
            return;
        }
        HandleMovement();
    }

    public bool IsAlive()
    {
        return _isAlive;
    }
    public void TakeDamage(Transform damageSource, int damage)
    {
        if (_canTakeDamage && _isAlive)
        {
            _canTakeDamage = false;
            _currentHealth = Mathf.Max(0, _currentHealth -= damage);
            _currentHealthBar = _currentHealth;
            _knockBack.GetKnockBack(damageSource);
            StartCoroutine(DamageRecoveryRoutine());
        }
        DetectDeath();
    }

    private void DetectDeath()
    {
        if (_currentHealth == 0 && _isAlive)
        {
            _isAlive = false;
            _knockBack.StopKnockBackMovement();
            GameImput.Instance.DisableMovement();
            OnPlayerDeath?.Invoke(this, EventArgs.Empty);
        }
    }

    private IEnumerator DamageRecoveryRoutine()
    {
        yield return new WaitForSeconds(_damageRecoveryTime);
        _canTakeDamage = true;
    }
    private void HandleMovement()
    {
        Vector2 inputVector = GameImput.Instance.GetMovementVector();
        _rb.MovePosition(_rb.position + inputVector * (Time.fixedDeltaTime * _movingSpeed));
        if (Mathf.Abs(inputVector.x) > _minMovingSpeed || Mathf.Abs(inputVector.y) > _minMovingSpeed)
        {
            _isRunning = true;
        }
        else
        {
            _isRunning = false;
        }
    }

    public bool IsRunning()
    {
        return _isRunning;
    }

    public Vector3 GetPlayerScreenPosition()
    {
        Vector3 playerScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
        return playerScreenPosition;
    }
}