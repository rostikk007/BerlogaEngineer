using System.Collections;
using UnityEngine;

public class ScaleObject : MonoBehaviour
{
    public Vector3 targetScale = new Vector3(2f, 2f, 2f); // ������� ������ �������
    public float duration = 2f; // �����, �� ������� ������ ������� ������
    private Vector3 originalScale; // �������� ������ �������

    void Start()
    {
        originalScale = transform.localScale; // ��������� �������� ������
        StartCoroutine(ScaleOverTime(originalScale, targetScale, duration)); // ��������� ��������
    }

    private IEnumerator ScaleOverTime(Vector3 from, Vector3 to, float time)
    {
        float elapsedTime = 0f; // �����, ��������� � ������ ��������

        // ����������� ������
        while (elapsedTime < time)
        {
            transform.localScale = Vector3.Lerp(from, to, (elapsedTime / time)); // ������ �������� ������
            elapsedTime += Time.deltaTime; // ����������� ��������� �����
            yield return null; // ���� ���������� �����
        }

        transform.localScale = to; // ������������� �������� ������

        // ��������� ������ �������
        elapsedTime = 0f; // ���������� �����
        while (elapsedTime < time)
        {
            transform.localScale = Vector3.Lerp(to, from, (elapsedTime / time)); // ������ �������� ������ �������
            elapsedTime += Time.deltaTime; // ����������� ��������� �����
            yield return null; // ���� ���������� �����
        }

        transform.localScale = from; // ������������� �������� ������

        // ��������� �������� ����� ��� ������������ �����
        StartCoroutine(ScaleOverTime(originalScale, targetScale, duration));
    }
}
