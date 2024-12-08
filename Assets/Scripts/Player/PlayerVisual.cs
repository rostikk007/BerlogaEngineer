using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private GameObject _playerShadow;
    [SerializeField] private GameObject _playerHealth;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private const string IS_RUNNING = "IsRunning";
    private const string IS_DIE = "IsDie";
    public static bool IS_DIE_BAR = false;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        Player.Instance.OnPlayerDeath += Player_OnPlayerDeath;
    }

    private void Player_OnPlayerDeath(object sender, System.EventArgs e)
    {
        animator.SetBool(IS_DIE, true);
        IS_DIE_BAR = true;
        _playerShadow.SetActive(false);
        if (_playerHealth != null)
        {
            _playerHealth.SetActive(false);
        }
    }

    private void Update()
    {
        if (Player.Instance != null)
        {
            animator.SetBool(IS_RUNNING, Player.Instance.IsRunning());
            if (Player.Instance.IsAlive())
            {
                AdjustPlayerFacingDiection();
            }
        }
    }

    private void AdjustPlayerFacingDiection()
    {
        if (GameImput.Instance != null)
        {
            Vector3 mousePos = GameImput.Instance.GetMousePosition();
            Vector3 playerPosition = Player.Instance.GetPlayerScreenPosition();
            spriteRenderer.flipX = mousePos.x < playerPosition.x;
        }
    }
}
