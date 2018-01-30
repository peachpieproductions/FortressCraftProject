using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : Item {

    public int xpos;
    public int ypos;
    public int chunkx;
    public int chunky;
    internal bool ore;
    internal bool attached = true;

    public void SpawnOreAround() {
        UpdateItem(2);
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

    public void onPlaced() {
        switch (info.itemId) {
            case 3: StartCoroutine(StorageBlock()); break;
            //case 4: StartCoroutine(Conveyor()); break;
        }
    }

    public void SetBlock(int x, int y, int chunkx, int chunky) {
        xpos = x;
        ypos = y;
        this.chunkx = chunkx;
        this.chunky = chunky;
    }

    public Block GetAdjacentBlock(int offsetx, int offsety) {
        if (attached) {
            var go = C.chunkGrid[chunkx, chunky].terrain[xpos + offsetx, ypos + offsety].go;
            if (go == null) return null;
            var block = go.GetComponent<Block>();
            if (block != null)
                return block;
        }
        return null;
    }

    public IEnumerator StorageBlock() {
        while (true) {
            var colls = Physics2D.OverlapCircleAll(transform.position, .7f, 1 << 0);
            foreach (Collider2D coll in colls) {
                var block = coll.GetComponent<Block>();
                if (block != null) {
                    if (block == this) continue;
                    if (!block.attached) {
                        Item i = block;
                        StartCoroutine(i.PlayerAttract(transform));
                    }
                }
            }
            yield return new WaitForSeconds(.5f);
        }
    }

    /*public IEnumerator Conveyor() {
        var coll = GetComponent<BoxCollider2D>();
        var size = coll.size; size.y *= .5f; coll.size = size;
        var offset = coll.offset; offset.y = -.1f; coll.offset = offset;
        GameObject transferObj = new GameObject();
        var transferSpr = transferObj.AddComponent<SpriteRenderer>();
        transferObj.transform.localScale *= .5f;
        transferObj.transform.parent = transform;
        transferObj.transform.localPosition = Vector3.zero;

        GameObject transferGo = null;
        float transferProgress = 0f;
        while (true) {
            while (transferGo != null) { //transferring item
                transferProgress += Time.deltaTime;
                transferObj.transform.localPosition = new Vector3(transferProgress, 0, 0);
                if (transferProgress >= .15) {
                    transferGo.transform.position = transform.position + new Vector3(.33f, .25f, 0);
                    var nextBlock = GetAdjacentBlock(1, 0);
                    if (nextBlock != null && nextBlock.info.itemId == 4) {
                        //nextBlock
                    } else {
                        transferGo.SetActive(true);
                    }
                    transferGo = null;
                    transferSpr.enabled = false;
                }
                yield return null;
            } 
            if (transferGo == null) { //find item to transfer
                var colls = Physics2D.OverlapCircleAll(transform.position, .16f, 1 << 0);
                foreach (Collider2D collider in colls) {
                    var block = collider.GetComponent<Block>();
                    if (block != null) {
                        if (block == this) continue;
                        if (!block.attached) {
                            transferGo = block.gameObject;
                            transferGo.transform.rotation = Quaternion.identity;
                            transferProgress = -.15f;
                            transferObj.transform.localPosition = new Vector3(transferProgress, 0, 0);
                            transferSpr.sprite = block.info.sprite;
                            transferSpr.enabled = true;
                            block.gameObject.SetActive(false);
                            //Destroy(block.gameObject);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }*/

}
