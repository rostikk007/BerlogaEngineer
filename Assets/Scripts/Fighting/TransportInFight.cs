using UnityEngine.SceneManagement;
using UnityEngine;

public class TransportInFight : MonoBehaviour
{
    [SerializeField] public int scene;
    public static bool isLoadingSceneFight = false;
    public void OnTriggerEnter2D(Collider2D other)
    {
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null && other.CompareTag("Player")) // �����������, ��� ������ ������ ����� BoxCollider2D
        {
            // ���������, ������������ �� ��������� ������� ������� � CircleCollider2D
            if (IsCollidingWithCircleCollider(other))
            {
                if (isLoadingSceneFight)
                    return;

                isLoadingSceneFight = true;
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