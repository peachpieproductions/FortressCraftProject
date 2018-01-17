﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Block : Item {

    internal int xpos;
    internal int ypos;
    internal bool ore;

	// Use this for initialization
	new void Start () {
        base.Start();
        attached = true;
        spr = GetComponent<SpriteRenderer>();
        if (Random.value < .5f) GetComponent<SpriteRenderer>().flipX = true;
        if (Random.value < .5f) GetComponent<SpriteRenderer>().flipY = true;
        //Destroy(GetComponent<Rigidbody2D>());
    }


    public void SpawnOreAround() {
        UpdateItem(1);
        if (xpos < 1 || ypos < 1 || xpos > C.c.chunkWidth - 2 || ypos > C.c.chunkHeight - 2) return;
        for (var i = 0; i < 4; i++) {
            if (Random.value < .4f) {
                Block block = null;
                /*if (i == 0) block = C.terrainGoGrid[xpos + 1, ypos].GetComponent<Block>();
                if (i == 1) block = C.terrainGoGrid[xpos, ypos + 1].GetComponent<Block>();
                if (i == 2) block = C.terrainGoGrid[xpos - 1, ypos].GetComponent<Block>();
                if (i == 3) block = C.terrainGoGrid[xpos, ypos - 1].GetComponent<Block>();*/
                if (block != null) {
                    if (!block.ore) {
                        block.ore = true;
                        block.SpawnOreAround();
                    }
                }
            }
        }
    }

}
