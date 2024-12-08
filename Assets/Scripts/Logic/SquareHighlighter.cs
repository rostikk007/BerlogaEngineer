using UnityEngine;
using System.Collections;


public class SquareHighlighter : MonoBehaviour
{
    private Color originalColor;
    private Color highlightColor = Color.yellow; // ���� ���������
    private SpriteRenderer spriteRenderer;

    private Coroutine colorChangeCoroutine; // ������ ������ �� ���������� ��������

    [SerializeField] // ��������� ������������� ��� ���� � ���������� Unity
    private float animationDuration = 0.2f; // ������������ ��������

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color; // ��������� ������������ ����
    }

    void OnMouseEnter()
    {
        if (!FindObjectOfType<ClickManager>().IsBuildingModeActive()) // ���������, �������� �� ����� �������������
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine); // ������������� ������� ��������, ���� ��� ��������
                colorChangeCoroutine = null; // �������� ������ �� ��������
            }
            // ����� ����� �������� ������ ��� �������� � ������������� �����, ���� ����������
            spriteRenderer.color = originalColor; // ���������� ���� � �������������
        }
        else
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine); // ������������� ������� ��������, ���� ��� ��������
            }
            colorChangeCoroutine = StartCoroutine(ChangeColor(originalColor, highlightColor, animationDuration)); // ������� ��������� �� ������
        }
    }

    void OnMouseExit()
    {
        if (!FindObjectOfType<ClickManager>().IsBuildingModeActive()) // ���������, �������� �� ����� �������������
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine); // ������������� ������� ��������, ���� ��� ��������
                colorChangeCoroutine = null; // �������� ������ �� ��������
            }
            // ����� ����� �������� ������ ��� �������� � ������������� �����, ���� ����������
            spriteRenderer.color = originalColor; // ���������� ���� � �������������
        }
        else
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine); // ������������� ������� ��������, ���� ��� ��������
            }
            colorChangeCoroutine = StartCoroutine(ChangeColor(highlightColor, originalColor, animationDuration)); // ������� ��������� �������
        }
    }

    private IEnumerator ChangeColor(Color fromColor, Color toColor, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            spriteRenderer.color = Color.Lerp(fromColor, toColor, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; // ���� ��������� ����
        }

        spriteRenderer.color = toColor; // ������������� �������� ����
    }
}



