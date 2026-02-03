using UnityEngine;
using System.Collections.Generic;

public class RecordPanel : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject slotEmpty;

    private List<GameObject> slotList = new List<GameObject>();

    private void OnEnable()
    {
        RefreshPanel();
    }

    // 닫을 때 슬롯 정리
    private void OnDisable()
    {
        ClearSlots();
    }

    private void RefreshPanel()
    {
        ClearSlots();

        RecordData data = DataManager.LoadRecordData();
        if (data == null || data.records.Count == 0)
        {
            slotEmpty.SetActive(true);
            return;
        }
        slotEmpty.SetActive(false);

        int rank = 1;
        foreach (var record in data.records)
        {
            GameObject go = Instantiate(slotPrefab, container);
            RecordSlot slot = go.GetComponent<RecordSlot>();

            var (_, sprite) = DataManager.LoadCharacter(record.characterId);
            slot.Setup(record, sprite, rank);

            slotList.Add(go);
            rank++;
        }
    }
    private void ClearSlots()
    {
        foreach (GameObject slot in slotList)
        {
            Destroy(slot);
        }
        slotList.Clear();
    }

    public void CloseButton()
    {
        gameObject.SetActive(false);
    }
}