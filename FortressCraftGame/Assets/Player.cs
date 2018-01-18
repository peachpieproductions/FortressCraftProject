using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    Rigidbody2D rb;
    public Item[] inv = new  Item[50];
    public int hotbarSel;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        var inst = Instantiate(C.c.prefabs[0], transform.position + Vector3.up * .5f, Quaternion.identity);
        inst.GetComponent<Block>().UpdateItem(3);
    }

    // Update is called once per frame
    void Update () {
        var vel = rb.velocity;
		if (Input.GetKey(KeyCode.D)) {
            if (vel.x < 3f) vel.x += .2f;
        }
        if (Input.GetKey(KeyCode.A)) {
            if (vel.x > -3f) vel.x -= .2f;
        }
        if (Input.GetKeyDown(KeyCode.W)) {
            vel.y += 4f;
        }
        if (vel.y < -5) vel.y = -5;
        rb.velocity = vel;
        
	}
}
