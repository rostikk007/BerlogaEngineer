using UnityEngine;
using TMPro;
using System.IO;
using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Collections.Generic;
public class SwordVisual : MonoBehaviour
{
    [SerializeField] private Sword sword;
    [SerializeField] private SwordSkins swordSkins;
    [SerializeField] private SpriteRenderer swordRenderer;

    private Animator animator;
    private const string ATTACK = "Attack";
    private string monsper;

    private void Awake()
    {
        InitializeComponents();
        StartCoroutine(LoadSkinWithDelay());
    }

    private void InitializeComponents()
    {
        if (sword == null) Debug.LogError("sword is null");
        if (swordSkins == null) Debug.LogError("swordSkins is null");

        animator = GetComponent<Animator>();

        if (swordRenderer == null)
        {
            swordRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private IEnumerator LoadSkinWithDelay()
    {
        // Ждем несколько кадров, чтобы дать время инициализироваться другим системам
        yield return new WaitForSeconds(0.1f);
        LoadSelectedSkin();
    }

    private void LoadSelectedSkin()
    {
        try
        {
            // Получаем путь к файлу сохранения
            string savePath = Path.Combine(Application.persistentDataPath, "PlayerSave.json");
            Debug.Log($"Пытаемся загрузить файл: {savePath}");

            if (!File.Exists(savePath))
            {
                Debug.LogError($"Файл сохранения не найден по пути: {savePath}");
                return;
            }

            string jsonContent = File.ReadAllText(savePath);
            Debug.Log($"Содержимое файла: {jsonContent}");

            PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(jsonContent);
            if (playerData == null)
            {
                Debug.LogError("Не удалось десериализовать PlayerData");
                return;
            }

            Debug.Log($"Выбранный скин: {playerData.SelectedCharacterSkin}");
            if (swordSkins != null && swordRenderer != null)
            {
                Sprite selectedSkin = swordSkins.GetSkinById((int)playerData.SelectedCharacterSkin);
                if (selectedSkin != null)
                {
                    swordRenderer.sprite = selectedSkin;
                    Debug.Log("Скин успешно установлен");
                }
                else
                {
                    Debug.LogError($"Не найден скин с индексом {(int)playerData.SelectedCharacterSkin}");
                }
            }
            else
            {
                if (swordSkins == null) Debug.LogError("swordSkins is null");
                if (swordRenderer == null) Debug.LogError("swordRenderer is null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при загрузке скина меча: {e.Message}\nStackTrace: {e.StackTrace}");
        }
    }

    private void Start()
    {
        sword.OnSwordSwing += Sword_OnSwordSwing;
    }

    private void Sword_OnSwordSwing(object sender, System.EventArgs e)
    {
        if (animator != null)
        {
            animator.SetTrigger(ATTACK);
        }
    }

    public void TriggerEndAttackAnimation()
    {
        sword.AttackColliderTurnOff();
    }

    // Метод для обновления скина меча
    public void UpdateSwordSkin(int skinId)
    {
        if (swordRenderer != null && swordSkins != null)
        {
            Sprite newSkin = swordSkins.GetSkinById(skinId);
            swordRenderer.sprite = newSkin;
        }
    }
}