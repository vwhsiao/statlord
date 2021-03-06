﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MonoBehaviour {
    private Transform tform;
    private Rigidbody myRigidbody;
    public bool freezeUpdate = false;
    public bool grounded = true;
    private PlayerStats stats;
    private CameraManager cam;
    private RectTransform canvas;
    private Game game;
    private Image healthBar;
    private Image backBar;
    private float timeSinceMeleed = 3.0f;
    // timers
    private float timeSinceAttack = 0.0f;
    public float curHealth = 1.0f;

    void Awake() {
        tform = transform;
        myRigidbody = GetComponent<Rigidbody>();
        stats = GetComponent<PlayerStats>();
        cam = Camera.main.GetComponent<CameraManager>();

        canvas = GameObject.Find("StatUI").GetComponent<RectTransform>();
        game = canvas.GetComponent<Game>();

        GameObject healthFront = new GameObject("healthBar");
        healthBar = healthFront.AddComponent<Image>();
        healthBar.rectTransform.SetParent(canvas.transform, false);
        healthBar.color = Color.red;

        GameObject healthBack = new GameObject("healthBarBack");
        backBar = healthBack.AddComponent<Image>();
        backBar.rectTransform.SetParent(canvas.transform, false);
        backBar.color = new Color(0.0f, 0.0f, 0.0f);

        Application.runInBackground = false;
    }

    public void setHealthBar(bool visible) {
        healthBar.gameObject.SetActive(visible);
        backBar.gameObject.SetActive(visible);
    }

    float xbar = 0.0f;
    private void updateHealth() {

        float targetX = curHealth / stats.get(Stats.health).value;
        xbar = Mathf.Lerp(xbar, targetX, Time.deltaTime * 8.0f);

        healthBar.rectTransform.offsetMin = Vector2.zero;
        healthBar.rectTransform.offsetMax = Vector2.zero;
        healthBar.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
        healthBar.rectTransform.anchorMax = new Vector2(xbar, 0.05f);

        backBar.rectTransform.offsetMin = Vector2.zero;
        backBar.rectTransform.offsetMax = Vector2.zero;
        backBar.rectTransform.anchorMin = new Vector2(xbar, 0.0f);
        backBar.rectTransform.anchorMax = new Vector2(1.0f, 0.05f);
    }


    // Update is called once per frame
    void Update() {
        updateHealth();
        timeSinceMeleed -= Time.deltaTime;
        // find where on ground plane your mouse is pointing and look there
        RaycastHit info;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out info, 1000.0f, 1 << 8)) {
            Vector3 fw = info.point - tform.position;
            fw.y = 0.0f;
            fw.Normalize();
            transform.rotation = Quaternion.LookRotation(fw);
        }

        // so you can still look around at least
        if (freezeUpdate) {
            myRigidbody.velocity = Vector3.zero;
            return;
        }

        // calculate movement based on camera view
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        Vector3 dir = Vector3.zero;
        if (inputX != 0.0f || inputY != 0.0f) {
            dir = cam.transform.TransformDirection(new Vector3(inputX, 0.0f, inputY));
            dir.y = 0.0f;
            dir.Normalize();
        }
        dir *= stats.get(Stats.moveSpeed).value;

        // jumping code
        float newY = myRigidbody.velocity.y;
        if (Input.GetKeyDown(KeyCode.Space) && grounded) {
            newY = stats.get(Stats.jumpSpeed).value;
            grounded = false;
        }
        myRigidbody.velocity = new Vector3(dir.x, newY, dir.z);

        // attack if holding down mouse
        timeSinceAttack -= Time.deltaTime;
        if (Input.GetMouseButton(0) && timeSinceAttack < 0.0f) {
            timeSinceAttack = stats.get(Stats.attackRate).value;

            StartCoroutine(stats.multiShot());
        }

        curHealth += stats.get(Stats.healthpersec).value * Time.deltaTime;
        curHealth = Mathf.Clamp(curHealth, 0.0f, stats.get(Stats.health).value);
        // die if no health
        if (curHealth <= 0.0f) {
            game.restartLevel();
        }

    }

    public void reset() {
        transform.position = Vector3.zero;
        curHealth = stats.get(Stats.health).value;
    }

    void OnCollisionEnter(Collision c) {
        checkMeleed(c);
    }
    void OnCollisionStay(Collision c) {
        checkMeleed(c);
    }

    private void checkMeleed(Collision c) {
        if ((c.gameObject.tag == Tags.Enemy) && (timeSinceMeleed < 0.0f)) {
            float damage = c.gameObject.GetComponent<EnemyBasicScript>().damage;
            applyDamage(damage);
            timeSinceMeleed = 1.0f;
        }
    }

    void OnTriggerEnter(Collider c) {
        if (c.gameObject.tag == Tags.EnemyProjectile) {
            Projectile p = c.GetComponent<Projectile>();
            applyDamage(p.damage);
            p.returnSelf();
        }
    }

    public void projectileHit(float damage) {
        curHealth += damage * stats.get(Stats.lifesteal).value;
    }

    private void applyDamage(float damage) {
        if (Random.value < stats.get(Stats.dodge).value) {
            return;
        }

        cam.addShake(damage);

        curHealth -= damage * (1.0f - stats.get(Stats.mitigation).value);
    }

    void FixedUpdate() {
        // check to see if player is grounded
        Vector3 castStart = tform.position + new Vector3(0.0f, 0.5f, 0.0f);
        RaycastHit info;
        grounded = Physics.SphereCast(castStart, 0.25f, Vector3.down, out info, 0.5f);
        if (myRigidbody.velocity.y > 0.1f) { // prevent double jumping
            grounded = false;
        }
    }

}
