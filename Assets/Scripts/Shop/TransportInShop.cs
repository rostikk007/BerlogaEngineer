using UnityEngine.SceneManagement;
using UnityEngine;

public class TransportInShop : MonoBehaviour
{
    [SerializeField] private int scene;
    public static bool isLoadingSceneShop = false;
    public void OnTriggerEnter2D(Collider2D other)
    {
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null && other.CompareTag("Player")) // Предположим, что второй объект имеет BoxCollider2D
        {
            // Проверяем, пересекается ли коллайдер второго объекта с CircleCollider2D
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
        // Получаем массив всех коллайдеров этого объекта
        Collider2D[] colliders = GetComponents<Collider2D>();

        foreach (Collider2D collider in colliders)
        {
            // Проверяем, является ли коллайдер CircleCollider2D
            if (collider is CircleCollider2D)
            {
                // Проверяем, пересекается ли CircleCollider2D с другим коллайдером
                if (collider.IsTouching(other))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
