using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BackgroundParallax : MonoBehaviour {

	public GameObject[] backgrounds;
	public Tilemap tilemap;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pos = gameObject.transform.position;
		if (gameObject.GetComponent<Camera>().rect.min.x < tilemap.GetComponent<TilemapCollider2D>().bounds.min.x) {
			gameObject.transform.position = new Vector3(tilemap.GetComponent<TilemapCollider2D>().bounds.min.x + gameObject.GetComponent<Camera>().rect.center.x, transform.position.y);
		}
		for(int i = 0; i < backgrounds.Length; i++) {
			backgrounds[i].transform.position = pos / (i+2);
		}
		
	}
}
