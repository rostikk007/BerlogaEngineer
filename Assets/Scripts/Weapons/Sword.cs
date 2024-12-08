using System;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] private int damageAmount = 2;
    public event EventHandler OnSwordSwing;

    private PolygonCollider2D polygonCollider2D;

    private void Awake()
    {
        polygonCollider2D = GetComponent<PolygonCollider2D>();
    }

    private void Start()
    {
        AttackColliderTurnOff();
    }
    public void Attack()
    {
        AttackColliderTurnOffOn();
        OnSwordSwing?.Invoke(this,EventArgs.Empty);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.TryGetComponent(out EnemyEntity enemyEntity))
        {
            enemyEntity.TakeDamage();
        }
    }

    public void AttackColliderTurnOff()
    {
        if (polygonCollider2D != null)
        {
            polygonCollider2D.enabled = false;
        }
       
    }

    private void AttackColliderTurnOn()
    {
        if (polygonCollider2D != null)
        {
            polygonCollider2D.enabled = true;
        }
    }

    private void AttackColliderTurnOffOn()
    {
        AttackColliderTurnOff();
        AttackColliderTurnOn();
    }
}
