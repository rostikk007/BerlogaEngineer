using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;


public class HealthBar : MonoBehaviour
{
    private Player _player;
    private PlayerVisual _visual;
    Image _healthBar;
    private float HP;
    void Start()
    {
        _player = GetComponent<Player>();
        _visual = GetComponent<PlayerVisual>();
        _healthBar = GetComponent<Image>();
        HP = Player._maxHealth;
        _healthBar.fillAmount = 1;
        Player._currentHealthBar = 10;
    }

    void Update()
    {
        _healthBar.fillAmount = HP / Player._maxHealth;
        if (Player._currentHealthBar == 0 && PlayerVisual.IS_DIE_BAR == false)
        {
            HP = Player._maxHealth;
        }
        else
        {
            HP = Player._currentHealthBar;
        }
        if (HP == 0)
        {
            PlayerVisual.IS_DIE_BAR = false;
            
            SceneTransition.SwitchToScene(3);
        }
    }
}
