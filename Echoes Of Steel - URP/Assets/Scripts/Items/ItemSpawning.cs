using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawning : MonoBehaviour {

    [SerializeField] private List<SpawnPoint> itemSpawnPoints = new List<SpawnPoint>();
    [SerializeField] private List<ItemToSpawn> itemsToSpawn = new List<ItemToSpawn>();

    private List<ItemSO> respawnableItems = new List<ItemSO>();
    private List<SpawnPoint> spawnPointsAvailable;

    private void Start() {
        spawnPointsAvailable = itemSpawnPoints;

        SpawnItems();
    }

    private void SpawnItems() {
        foreach (ItemToSpawn itemToSpawn in itemsToSpawn) {
            ItemSO itemSO = itemToSpawn.itemSO;

            if (itemToSpawn.itemSO.respawnable) {
                respawnableItems.Add(itemToSpawn.itemSO);
            }

            for (int i = 0; i < itemToSpawn.amount; i++) {
                SpawnItem(itemSO);
            }
        }
    }

    private void SpawnItem(ItemSO itemSO) {
        SpawnPoint spawnPoint = GetRandomSpawnPointForItem(itemSO.itemType);
        if (spawnPoint != null) {
            GameObject item = Instantiate(itemSO.prefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
            spawnPoint.SetItem(item.GetComponent<Item>());
            spawnPoint.OnItemRemoved += SpawnPoint_OnItemRemoved;
        } else {
            Debug.LogWarning("Could not spawn all items, ran out of item spawnpoints!");
        }
    }

    private void SpawnPoint_OnItemRemoved(Item item) {
        if (item == null) return;

        ItemSO itemSO = item.GetItemSO();

        if (respawnableItems.Contains(itemSO)) {
            StartCoroutine(RespawnItem(itemSO));
        }
    }

    private SpawnPoint GetRandomSpawnPointForItem(ItemSO.ItemType itemType) {
        spawnPointsAvailable = itemSpawnPoints;

        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>();
        foreach (SpawnPoint spawnPoint in spawnPointsAvailable) {
            if (spawnPoint == null) continue;

            if (!spawnPoint.IsSpawnPointEmpty()) continue;

            ItemSO.ItemType[] allowedItems = spawnPoint.GetAllowedItems();
            foreach (ItemSO.ItemType allowedItem in allowedItems) {
                if (allowedItem == itemType) {
                    availableSpawnPoints.Add(spawnPoint);
                    break;
                }
            }
        }

        if (availableSpawnPoints.Count == 0) {
            return null;
        }

        int randomIndex = Random.Range(0, availableSpawnPoints.Count);
        SpawnPoint randomSpawnPoint = availableSpawnPoints[randomIndex];
        spawnPointsAvailable.Remove(randomSpawnPoint);

        return availableSpawnPoints[randomIndex];
    }

    private IEnumerator RespawnItem(ItemSO itemSO) {
        int respawnTime = itemSO.respawnTime;

        yield return new WaitForSeconds(respawnTime);

        SpawnItem(itemSO);
    }


    [System.Serializable]
    public class ItemToSpawn {
        public ItemSO itemSO;
        public int amount;

        public ItemToSpawn(ItemSO itemSO, int amount) {
            this.itemSO = itemSO;
            this.amount = amount;
        }
    }
}