using System.Collections;
using UnityEngine;

public class RandomRotate : MonoBehaviour
{
    public float rotationAmount = 15f; // ������������ ���� ��������
    public float minInterval = 1f; // ����������� �������� ����� ����������
    public float maxInterval = 3f; // ������������ �������� ����� ����������
    public float rotationSpeed = 2f; // �������� ��������

    void Start()
    {
        StartCoroutine(Rotate()); // ��������� �������� Rotate
    }

    IEnumerator Rotate()
    {
        while (true) // ����������� ����
        {
            // ���������� ��������� ����� ��������
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime); // ������� ��������� �����

            // ���������� ��������� ���� ��������
            float randomRotation = Random.Range(-rotationAmount, rotationAmount);
            Quaternion targetRotation = Quaternion.Euler(0, 0, randomRotation); // ������� �������

            float elapsedTime = 0f;
            Quaternion initialRotation = transform.rotation; // ��������� ��������� �������

            while (elapsedTime < rotationSpeed) // ������� ��������
            {
                transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, (elapsedTime / rotationSpeed));
                elapsedTime += Time.deltaTime; // ����������� �����
                yield return null; // ���� ���������� �����
            }

            // ������������� ������������� �������
            transform.rotation = targetRotation;
        }
    }
}

