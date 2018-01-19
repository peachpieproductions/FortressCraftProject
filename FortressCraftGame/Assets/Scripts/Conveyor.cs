using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : Block {

    public GameObject transferGo;
    public GameObject transferObj;
    public float transferProgress;
    public SpriteRenderer transferSpr;
    public Sprite transferSprite;
    public List<Item> itemWaitingQueue = new List<Item>();

    new void Start() {
        base.Start();
        //resize collider to half
        var coll = GetComponent<BoxCollider2D>();
        var size = coll.size; size.y *= .5f; coll.size = size;
        var offset = coll.offset; offset.y = -.1f; coll.offset = offset;
    }

    public new void onPlaced() {
        transferObj = new GameObject();
        transferSpr = transferObj.AddComponent<SpriteRenderer>();
        transferObj.transform.localScale *= .5f;
        transferObj.transform.parent = transform;
        transferObj.transform.localPosition = Vector3.zero;
        var coll = gameObject.AddComponent<CircleCollider2D>();
        coll.isTrigger = true;

    }

    private void Update() {
        if (transferGo != null) { //transferring item
            if (transferProgress >= .15) {
                var nextBlock = GetAdjacentBlock(1, 0);
                if (nextBlock != null && nextBlock.info.itemId == 4) {
                    Conveyor nextConv = nextBlock.GetComponent<Conveyor>();
                    if (nextConv.transferGo == null && nextConv != null) {
                        nextConv.StartConveyor(transferGo, transferSprite);
                        transferGo.GetComponent<Item>().onConveyor = false;
                        transferGo = null;
                        //transferSpr.enabled = false;
                    }
                } else {
                    transferGo.transform.position = transform.position + new Vector3(.33f, .6f, 0);
                    transferGo.SetActive(true);
                    transferGo.GetComponent<Item>().onConveyor = false;
                    transferGo = null;
                    //transferSpr.enabled = false;
                }
            } else {
                transferProgress += Time.deltaTime;
                transferObj.transform.localPosition = new Vector3(transferProgress, 0, 0);
            }
        } else {
            if (itemWaitingQueue.Count > 0) {
                StartConveyor(itemWaitingQueue[0].gameObject, itemWaitingQueue[0].info.sprite);
                itemWaitingQueue.RemoveAt(0);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (attached) {
            var item = collision.GetComponent<Item>();
            if (item != null && !item.onConveyor) {
                if (item == this) return;
                item.onConveyor = true;
                item.gameObject.SetActive(false);
                if (transferGo != null) {
                    itemWaitingQueue.Add(item);
                    return;
                }
                StartConveyor(item.gameObject, item.info.sprite);
            }
        }
    }

    public void StartConveyor(GameObject block, Sprite spr) {
        transferSprite = spr;
        transferGo = block;
        transferGo.transform.rotation = Quaternion.identity;
        transferProgress = -.15f;
        transferObj.transform.localPosition = new Vector3(transferProgress, 0, 0);
        transferSpr.sprite = spr;
        //transferSpr.enabled = true;
    }

    /*
    public IEnumerator Conveyor() {
        
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
