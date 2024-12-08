using UnityEngine.SceneManagement;
using UnityEngine;

public class TransportInShop : MonoBehaviour
{
    [SerializeField] private int scene;
    public static bool isLoadingSceneShop = false;
    public void OnTriggerEnter2D(Collider2D other)
    {
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null && other.CompareTag("Player")) // �����������, ��� ������ ������ ����� BoxCollider2D
        {
            // ���������, ������������ �� ��������� ������� ������� � CircleCollider2D
            if (IsCollidingWithCircleCollider(other))
            { 
                if (isLoadingSceneShop)
                    return;

                isLoadingSceneShop = true;
                SceneTransition.SwitchToScene(scene);

            }
        }
        
        
    }
    private bool IsCollidingWithCircleCollider(Collider2D other)
    {
        // �������� ������ ���� ����������� ����� �������
        Collider2D[] colliders = GetComponents<Collider2D>();

        foreach (Collider2D collider in colliders)
        {
            // ���������, �������� �� ��������� CircleCollider2D
            if (collider is CircleCollider2D)
            {
                // ���������, ������������ �� CircleCollider2D � ������ �����������
                if (collider.IsTouching(other))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
