using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {
    public Transform player;
    // Start is called before the first frame update
    void Start() {
        //player = GetComponent<GameObject>();
    }

    // Update is called once per frame
    void Update() {
        if(player != null)
            transform.position = new Vector3(player.position.x, transform.position.y, player.position.z);
    }
}
