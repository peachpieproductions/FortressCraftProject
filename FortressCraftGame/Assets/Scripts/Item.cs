﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ItemStruct {
    public int itemId;
    public string itemName;
    public int stack;
    public float mineTime;
    public Sprite sprite;
    public Sprite altSprite;
    public System.Type script;
}

public class Item : MonoBehaviour {

    public ItemStruct info;
    Transform target;
    Rigidbody2D rb;
    public bool onConveyor;
    
    public SpriteRenderer spr;

    // Use this for initialization
    public void Start () {
        spr = GetComponent<SpriteRenderer>();
	}

    public void UpdateItem(int ID) {
        info.itemId = C.itemData[ID].itemId;
        info.itemName = C.itemData[ID].itemName;
        info.mineTime = C.itemData[ID].mineTime;
        info.sprite = C.itemData[ID].sprite;
        GetComponent<SpriteRenderer>().sprite = info.sprite;
        if (C.itemData[ID].script != null && GetComponent(C.itemData[ID].script) == null) { //check if swap script, if so check if new script exists
            Destroy(GetComponent<MonoBehaviour>()); //delete current script
            Item item = (Item)gameObject.AddComponent(C.itemData[ID].script); //add new script - via type, cast to item
            item.UpdateItem(ID);
        }
        
    }

    public IEnumerator PlayerAttract(Transform player) {
        if (target == null) {
            target = player;
            float attractTime = 8f;
            while (attractTime > 0) {
                if (onConveyor) {
                    target = null;
                    yield break;
                }
                attractTime -= Time.deltaTime;
                if (rb != null) rb.velocity += (Vector2)(target.transform.position - transform.position) * .2f;
                else rb = GetComponent<Rigidbody2D>();
                if (Vector3.Distance(target.transform.position,transform.position) < .7f) {
                    AddToInventory();
                    break;
                }
                yield return null;
            } target = null;
        } 
    }

    public void AddToInventory() {
        bool success = false;
        if (onConveyor) return;
        for (var i = 0; i < 50; i++) { //check for an existing stack
            if (C.c.player.GetComponent<Player>().inv[i] != null) {
                if (C.c.player.GetComponent<Player>().inv[i].info.itemId == info.itemId) {
                    C.c.player.GetComponent<Player>().inv[i].info.stack++;
                    Destroy(gameObject);
                    success = true;
                    break;
                }
            }
        }
        if (!success) {
            for (var i = 0; i < 50; i++) { //then check for an available slot
                if (C.c.player.GetComponent<Player>().inv[i] == null) {
                    C.c.player.GetComponent<Player>().inv[i] = this;
                    C.c.player.GetComponent<Player>().inv[i].info.stack++;
                    gameObject.SetActive(false);
                    success = true;
                    break;
                }
            }
        }
        if (success) {
            C.c.UpdateHotbar();
            C.c.PlaySound(0, 1.2f);
        }
    }

}
