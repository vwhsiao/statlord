﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {

    public int level = 1;

    private StatUI ui;
    private SpawnManager spawner;
    private ProjectileManager shooter;
    private Player player;
    private Text frontInd;
    private Text backInd;

    private GameObject overlay;
    private RawImage fadeImage;

    private bool betweenLevels = false;

    // Use this for initialization
    void Start() {
        ui = GameObject.Find("StatUI").GetComponent<StatUI>();
        spawner = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        shooter = GameObject.Find("ProjectileManager").GetComponent<ProjectileManager>();
        player = GameObject.Find("Player").GetComponent<Player>();
        frontInd = transform.Find("LevelIndicator").GetComponent<Text>();
        backInd = transform.Find("LevelIndicatorBack").GetComponent<Text>();
        overlay = transform.Find("Overlay").gameObject;

        fadeImage = overlay.GetComponent<RawImage>();

        player.reset();
        spawnGuys();
    }

    float fadeTime = 2.0f;
    // fades in from whatever color the image is at
    private IEnumerator fade(bool fadein) {
        if (!fadein) {
            overlay.SetActive(true);
        }
        float t = fadeTime;
        while (t > 0.0f) {
            Color c = fadeImage.color;
            if (fadein) {
                c.a = t / fadeTime;
            } else {
                c.a = 1.0f - t / fadeTime;
            }
            fadeImage.color = c;
            t -= Time.deltaTime;
            yield return null;
        }
        // reset fade variables back to defaults
        fadeImage.color = Color.black;
        fadeTime = 2.0f;
        if (fadein) {
            overlay.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update() {
        if (spawner.activeEnemies <= 0 && !betweenLevels && !spawner.spawning) {
            betweenLevels = true;
            StartCoroutine(loadLevelSequence());
        }
    }

    private void spawnGuys() {
        int number = level * 10;

        for (int i = 0; i < number; i++) {
            float rnd = Random.value;
            if (rnd < 0.3f) {
                spawner.BuildEnemy(EnemyType.SKELETON);
            } else if (rnd < 0.5f) {
                spawner.BuildEnemy(EnemyType.RANGER);
            } else if (rnd < 0.70f) {
                spawner.BuildEnemy(EnemyType.CROSSBOW);
            } else if (rnd < 0.90f) {
                spawner.BuildEnemy(EnemyType.MAGE);
            } else {
                spawner.BuildEnemy(EnemyType.ORC);
            }
        }

        spawner.spawning = true;
    }

    private IEnumerator loadLevelSequence() {
        player.freezeUpdate = true;

        frontInd.enabled = true;
        frontInd.text = "Level " + level + " defeated!";
        frontInd.color = Color.red;
        backInd.enabled = true;
        backInd.text = "Level " + level + " defeated!";
        backInd.color = Color.black;
        // play sound or something?

        yield return new WaitForSeconds(3.0f);

        frontInd.enabled = false;
        backInd.enabled = false;

        ui.buildUI(level);

        // wait for ui stat upgrading to finish
        while (ui.visible) {
            yield return null;
        }

        // next level!
        level++;
        frontInd.enabled = true;
        frontInd.text = "Level " + level;
        frontInd.color = Color.white;
        backInd.enabled = true;
        backInd.text = "Level " + level;
        StartCoroutine(fadeOutText(2.0f, 3.0f));

        player.reset();
        player.freezeUpdate = false;
        betweenLevels = false;
        spawnGuys();
    }

    public void restartLevel() {
        if (betweenLevels) {
            return;
        }
        betweenLevels = true;
        StartCoroutine(restartSequence());
    }

    private IEnumerator restartSequence() {
        player.freezeUpdate = true;

        frontInd.enabled = true;
        frontInd.text = "A GLORIOUS DEATH!";
        frontInd.color = Color.red;
        backInd.enabled = true;
        backInd.text = "A GLORIOUS DEATH!";
        backInd.color = Color.black;

        yield return new WaitForSeconds(1.0f);
        yield return fade(false);
        frontInd.enabled = false;
        backInd.enabled = false;
        yield return new WaitForSeconds(1.0f);
        spawner.killAll();
        shooter.destroyAll();
        player.reset();
        player.freezeUpdate = false;
        frontInd.enabled = true;
        frontInd.text = "Level " + level;
        frontInd.color = Color.white;
        backInd.enabled = true;
        backInd.text = "Level " + level;
        yield return fade(true);

        StartCoroutine(fadeOutText(0.1f, 2.0f));

        betweenLevels = false;

        spawnGuys();
    }

    private IEnumerator fadeOutText(float delay, float speed) {
        yield return new WaitForSeconds(delay);
        float t = speed;
        while (t > 0.0f) {
            t -= Time.deltaTime;
            frontInd.color = new Color(1.0f, 1.0f, 1.0f, t / 2.0f);
            backInd.color = new Color(0.0f, 0.0f, 0.0f, t / 2.0f);
            yield return null;
        }
        frontInd.enabled = false;
        backInd.enabled = false;
    }
}
