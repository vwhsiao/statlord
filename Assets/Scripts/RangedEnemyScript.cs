﻿using UnityEngine;
using System.Collections;

public class RangedEnemyScript : MonoBehaviour {
    private float timeBetweenShot;
    private GameObject player;
    private ProjectileManager projMan;

	// Use this for initialization
	void Start () {
        timeBetweenShot = 3.0f;
        player = GameObject.Find("Player");
        projMan = GameObject.Find("ProjectileManager").GetComponent<ProjectileManager>();
	}
	
	// Update is called once per frame
	void Update () {
        timeBetweenShot -= Time.deltaTime;
        if (timeBetweenShot<=0.0f)
        {
            if (gameObject.name.Contains("Ranger"))
            {
                Projectile x = projMan.getArrow();
                FireArrow(x);
            }
            else
            {
                Projectile x = projMan.getFireball();
            }
            timeBetweenShot = 3.0f;

        }
	}

    void FireArrow(Projectile proj)
    {
        
        proj.transform.position = transform.position + new Vector3(0, 1.0f, 0) + (transform.forward * 1.25f);
        proj.transform.rotation = transform.rotation;

        proj.damage = 4;

        proj.rb.AddForce(transform.forward * 800);

        proj.gameObject.tag = "EnemyProjectile";
    }
}
