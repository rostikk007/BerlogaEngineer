using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SwordSkins", menuName = "Game/SwordSkins")] // Изменил путь меню
public class SwordSkins : ScriptableObject
{
    [System.Serializable]
    public class SkinData
    {
        public int id;
        public Sprite sprite;
    }

    [SerializeField] private List<SkinData> skins;
    [SerializeField] private Sprite defaultSkin;

    public Sprite GetSkinById(int id)
    {
        var skin = skins.Find(s => s.id == id);
        return skin?.sprite ?? defaultSkin;
    }
}