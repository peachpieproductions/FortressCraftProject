﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct Sounds {
    public AudioClip ac;
    public bool pitchChange;
    public float vol;
}

public struct Chunk {
    public GameObject go;
    public TerrainBlock[,] terrain;
}

public struct TerrainBlock {
    public GameObject go;
    public int id;
}

public class C : MonoBehaviour {

    public static C c;
    public static ItemStruct[] itemData;
    public GameObject[] prefabs;
    //public static int[,] terrainGrid;
    //public static GameObject[,] terrainGoGrid;
    public int chunkWidth;
    public int chunkHeight;
    public int chunkStartSpawnRadius;
    public Transform player;
    internal Player playerScript;
    public Sounds[] snd;
    AudioSource AS;
    int camZoomLevel;
    public static GameObject hotBar;
    public static RectTransform InvRoot;
    public bool invOpen;
    int seed;
    public int currChunkX;
    public int currChunkY;
    public int chunkSurfaceY;
    public static Chunk[,] chunkGrid = new Chunk[200,200];
    public static Transform chunkRoot;
    public List<GameObject> chunksLoaded = new List<GameObject>();

    // Use this for initialization
    void Start () {
        c = GetComponent<C>();
        hotBar = GameObject.Find("Hotbar");
        InvRoot = GameObject.Find("InvRoot").GetComponent<RectTransform>();
        playerScript = player.GetComponent<Player>();
        InitItemData();
        AS = gameObject.AddComponent<AudioSource>();
        chunkRoot = new GameObject().transform;
        chunkRoot.name = "ChunkRoot";
        currChunkX = 100;
        currChunkY = chunkSurfaceY;
        for (var i = -chunkStartSpawnRadius; i < chunkStartSpawnRadius; i++) {
            for (var j = 0; j < chunkStartSpawnRadius; j++) {
                BuildTerrain(currChunkX + i, chunkSurfaceY - j);
            }
        }

        //InvokeRepeating("UpdateChunks", .1f, 1f);
        StartCoroutine(UpdateChunks());
    }
	
	// Update is called once per frame
	void Update () {

        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position,player.position + Vector3.back,.2f);

        //Place Block
        if (Input.GetMouseButton(0)) {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int gridx = Mathf.Min(chunkWidth-1,(int)Mathf.Round((pos.x % (chunkWidth * .32f)) / .32f));
            int gridy = Mathf.Min(chunkHeight-1,(int)Mathf.Round((pos.y % (chunkHeight * .32f)) / .32f));
            int chunkx = Mathf.FloorToInt(pos.x / (chunkWidth * .32f));
            int chunky = Mathf.FloorToInt(pos.y / (chunkHeight * .32f));
            if (chunkGrid[chunkx, chunky].terrain[gridx, gridy].id == 0) {
                if (playerScript.inv[playerScript.hotbarSel] != null && playerScript.inv[playerScript.hotbarSel].info.stack > 0) {
                    playerScript.inv[playerScript.hotbarSel].info.stack--;
                    chunkGrid[chunkx, chunky].terrain[gridx, gridy].id = playerScript.inv[playerScript.hotbarSel].info.itemId;
                    var inst = Instantiate(prefabs[0], new Vector3 (gridx * .32f + chunkx * chunkWidth * .32f, gridy * .32f + chunky * chunkHeight * .32f) , Quaternion.identity);
                    inst.GetComponent<Block>().UpdateItem(playerScript.inv[playerScript.hotbarSel].info.itemId);
                    inst.GetComponent<Block>().xpos = gridx;
                    inst.GetComponent<Block>().ypos = gridy;
                    inst.GetComponent<Block>().onPlaced();
                    //inst.transform.parent = chunkGrid[chunkx, chunky].go.transform;
                    PlaySound(0, .8f);
                    UpdateHotbar();
                }
            }
        }

        //Break Block
        if (Input.GetMouseButton(1)) {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var colls = Physics2D.OverlapCircleAll(pos, .7f, 1 << 0);
            foreach(Collider2D coll in colls) {
                var block = coll.GetComponent<Block>();
                if (block != null) {
                    if (block.attached) {
                        var rb = coll.gameObject.AddComponent<Rigidbody2D>();
                        rb.AddForce(new Vector2(Random.Range(-20, 20), Random.Range(-20, 20)));
                        chunkGrid[currChunkX, currChunkY].terrain[block.xpos, block.ypos].id = 0;
                        block.attached = false;
                    }
                }
            }
        }

        //Absorb
        if (Input.GetKey(KeyCode.E)) {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var colls = Physics2D.OverlapCircleAll(pos, .7f, 1 << 0);
            foreach (Collider2D coll in colls) {
                var block = coll.GetComponent<Block>();
                if (block != null) {
                    if (!block.attached) {
                        Item i = block;
                        StartCoroutine(i.PlayerAttract(player));
                    }
                }
            }
        }

        //Force Grab
        if (Input.GetKey(KeyCode.F)) {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var colls = Physics2D.OverlapCircleAll(pos, 2, 1 << 0);
            foreach (Collider2D coll in colls) {
                var block = coll.GetComponent<Block>();
                if (block != null) {
                    if (!block.attached) {
                        var forceDir = (pos - (Vector2)coll.transform.position) * 100f;
                        coll.GetComponent<Rigidbody2D>().AddForce(new Vector2(forceDir.x, forceDir.y));
                    }
                }
            }
        }

        //inventory
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab)) {
            invOpen = !invOpen;
        }
        var p = InvRoot.position;
        if (invOpen) {
            p.y = Mathf.Lerp(p.y, 256, .1f);
        } else {
            p.y = Mathf.Lerp(p.y, 0, .1f);
        }
        InvRoot.position = p;

        //camera zoom 
        if (Input.GetKeyDown(KeyCode.Z)) {
            camZoomLevel++;
            if (camZoomLevel > 3) camZoomLevel = 0;
        }
        switch(camZoomLevel) {
            case 0: Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, 5, .05f); break;
            case 1: Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, 7, .05f); break;
            case 2: Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, 10, .05f); break;
            case 3: Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, 2, .05f); break;
        }

        //scroll hotbar
        if (Input.mouseScrollDelta.y != 0) {
            playerScript.hotbarSel -= (int)Input.mouseScrollDelta.y;
            if (playerScript.hotbarSel < 0) playerScript.hotbarSel = 9;
            if (playerScript.hotbarSel > 9) playerScript.hotbarSel = 0;
            UpdateHotbar();
        }

    }

    public IEnumerator UpdateChunks() {
        while (true) {
            currChunkX = (int)(player.position.x / (chunkWidth * .32));
            currChunkY = (int)(player.position.y / (chunkHeight * .32));
            List<GameObject> activeChunksToDisable = new List<GameObject>(chunksLoaded);

            for (var i = -1; i < 2; i++) {
                for (var j = -1; j < 2; j++) {
                    if (chunkGrid[currChunkX + i, currChunkY + j].go != null) {
                        activeChunksToDisable.Remove(chunkGrid[currChunkX + i, currChunkY + j].go);
                        if (!chunkGrid[currChunkX + i, currChunkY + j].go.activeSelf) {
                            chunkGrid[currChunkX + i, currChunkY + j].go.SetActive(true);
                            int counter = 0;
                            foreach (Transform t in chunkGrid[currChunkX + i, currChunkY + j].go.transform) {
                                t.gameObject.SetActive(true);
                                if (counter < 64) { counter++; continue; } else { counter = 0; }
                                yield return null;
                            }
                            chunksLoaded.Add(chunkGrid[currChunkX + i, currChunkY + j].go);
                        }
                    } else {
                        BuildTerrain(currChunkX + i, currChunkY + j);
                    }
                }

            }
            foreach (GameObject go in activeChunksToDisable) {
                if (chunksLoaded.Contains(go)) chunksLoaded.Remove(go);

                int counter = 0;
                foreach (Transform t in go.transform) {
                    t.gameObject.SetActive(false);
                    if (counter < 64) { counter++; continue; } else { counter = 0; }
                    yield return null;
                }
                go.SetActive(false);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public void BuildTerrain(int chunkx, int chunky) {

        if (chunkGrid[chunkx, chunky].go != null) { //chunk exists
            chunkGrid[chunkx, chunky].go.SetActive(true);
            return;
        }
        else {  //chunk not yet created
            string groupName = "ChunkInstanceGroup " + chunkx + "," + chunky;
            GameObject GroupNull = new GameObject();
            GroupNull.transform.position = new Vector3(chunkx * chunkWidth * .32f,chunky * chunkHeight * .32f);
            GroupNull.name = groupName;
            GroupNull.transform.parent = chunkRoot;
            chunkGrid[chunkx, chunky].go = GroupNull;
            chunksLoaded.Add(GroupNull);
        }
        

        seed = Random.Range(0, 10000);
        chunkGrid[chunkx, chunky].terrain = new TerrainBlock[chunkWidth, chunkHeight];

        var xoffset = chunkx * .32f * chunkWidth;
        var yoffset = chunky * .32f * chunkHeight;

        if (chunky <= chunkSurfaceY) {
            for (var i = 0; i < chunkWidth; i++) {
                var currentHeight = (chunky != chunkSurfaceY) ? chunkHeight : chunkHeight - 20 + Mathf.PerlinNoise(i * .1f + seed, seed) * 10;
                for (var j = 0; j < currentHeight; j++) {
                    var inst = Instantiate(prefabs[0], new Vector3(xoffset + i * .32f, yoffset + j * .32f), Quaternion.identity);
                    inst.transform.parent = chunkGrid[chunkx, chunky].go.transform;
                    inst.GetComponent<Block>().xpos = i;
                    inst.GetComponent<Block>().ypos = j;
                    inst.GetComponent<Block>().UpdateItem(1);
                    chunkGrid[chunkx, chunky].terrain[i, j].id = 1; //flagged as grass/dirt
                    chunkGrid[chunkx, chunky].terrain[i, j].go = inst;
                }
            }
        }

        //spawn ores
        /*for (var i = 2; i < chunkWidth-2; i++) {
            for (var j = 2; j < chunkHeight-2; j++) {
                if (terrainGrid[i, j] == 1 && Random.value < .005f) {
                    terrainGrid[i, j] = 2;
                    terrainGoGrid[i, j].GetComponent<Block>().ore = true;
                    terrainGoGrid[i, j].GetComponent<Block>().SpawnOreAround();
                }
            }
        }*/

        if (player.position == Vector3.zero) player.position = new Vector3(chunkWidth * .32f * chunkx + chunkWidth * .5f * .32f, chunky * chunkHeight * .32f + chunkHeight * .32f);
        


    }

    public void PlaySound(int id, float pitchBase = 1) {
        if (snd[id].pitchChange) AS.pitch = pitchBase + Random.Range(-.2f, .2f);
        else AS.pitch = 1;
        AS.PlayOneShot(snd[id].ac,snd[id].vol);
    }

    public void UpdateHotbar() {
        for (var i = 0; i < 10; i++) {
            if (playerScript.hotbarSel == i) hotBar.transform.GetChild(i).GetComponent<Outline>().enabled = true;
            else hotBar.transform.GetChild(i).GetComponent<Outline>().enabled = false;
            if (playerScript.inv[i] != null) {
                var image = hotBar.transform.GetChild(i).GetChild(0).GetComponent<Image>();
                hotBar.transform.GetChild(i).GetChild(1).GetComponent<Text>().text = playerScript.inv[i].info.stack.ToString();
                if (playerScript.inv[i].info.stack <= 0) { //empty
                    image.enabled = false;
                    playerScript.inv[i] = null;
                    continue;
                }
                image.enabled = true;
                image.sprite = playerScript.inv[i].info.sprite;
            } 
        }
    }

    private void OnGUI() {
        var i = 0;
        GUI.Label(new Rect(10, 12 * i, 200, 20), "Debug:"); i++;
        GUI.Label(new Rect(10, 12 * i, 200, 20), "CurrChunkX: " + currChunkX); i++;
        GUI.Label(new Rect(10, 12 * i, 200, 20), "CurrChunkY: " + currChunkY); i++;
        GUI.Label(new Rect(10, 12 * i, 200, 20), "Chunks Loaded: " + chunksLoaded.Count); i++;
    }

    void InitItemData() {
        itemData = new ItemStruct[200];
        Sprite[] blockSprites = Resources.LoadAll<Sprite>("Sprites/Blocks");

        var i = 1;

        //Blocks
        itemData[i].itemId = i;
        itemData[i].itemName = "Dirt Block";
        itemData[i].sprite = blockSprites[0];

        i++; //2

        itemData[i].itemId = i;
        itemData[i].itemName = "Stone Block";
        itemData[i].sprite = blockSprites[2];

        i++; //3

        itemData[i].itemId = i;
        itemData[i].itemName = "Chest";
        itemData[i].sprite = blockSprites[3];

        i++; //4


        //Items
        i = 100;

        

    }
}
