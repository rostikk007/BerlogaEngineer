using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class KnockBack : MonoBehaviour
{
    [SerializeField] private float _knockBackForce = 3f;
    [SerializeField] private float _knockBAckMovingTimerMax = 0.3f;

    private float _knockBAckMovingTimer;
    private Rigidbody2D _rb;

    public bool IsGettingKnockedBack { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        _knockBAckMovingTimer -= Time.deltaTime;
        if (_knockBAckMovingTimer < 0 )
        {
            StopKnockBackMovement();
        }
    }

    public void GetKnockBack(Transform damageSource)
    {
        IsGettingKnockedBack = true;
        _knockBAckMovingTimer = _knockBAckMovingTimerMax;
        Vector2 difference = (transform.position - damageSource.position).normalized * _knockBackForce / _rb.mass;
        _rb.AddForce(difference, ForceMode2D.Impulse);
    }
    public void StopKnockBackMovement()
    {
        _rb.velocity = Vector2.zero;
        IsGettingKnockedBack = false;
    }
}
