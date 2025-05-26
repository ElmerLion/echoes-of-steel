using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Item")]
public class ItemSO : ScriptableObject {
     
    public LocalizedString itemName;
    public ItemType itemType;
    public Sprite itemSprite;
    public GameObject prefab;

    [Header("Respawn Settings")]
    public int respawnTime;
    public bool respawnable;

    public enum ItemType {
        Generic,
        Taser,
        Axe,
        PhotoCamera,
        AnimalSkull,
    }


}
