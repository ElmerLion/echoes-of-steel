using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bucket : Item {

    [SerializeField] private GameObject emptyVisual;

    private Refiller.RefillType refillType;
    private bool isFilled = false;

    public void Fill(Refiller.RefillType refillType, GameObject filledVisual) {
        isFilled = true;
        this.refillType = refillType;

        Destroy(transform.GetChild(0).gameObject);
        Instantiate(filledVisual, transform);
        Debug.Log("Bucket filled with " + refillType.ToString());
    }

    public void Empty() {
        isFilled = false;
        refillType = Refiller.RefillType.None;

        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        Instantiate(emptyVisual, transform);
    }

    public bool IsFilled() {
        return isFilled;
    }

    public Refiller.RefillType GetRefillType() {
        return refillType;
    }
}
