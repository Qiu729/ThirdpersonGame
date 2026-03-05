using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Starter.ThirdPersonCharacter;
using ThirdpersonGame;

public class UIManager : MonoBehaviour {

    public static UIManager Instance;

    [Header("UI")]
    public GameObject PlayerHurtView;
    public GameObject CrossHair;

    // 引用
    public GameManager gameManager;
    private NetworkRunner runner;

    // 私有
    private int _lastHealth;

    #region Untiy LifeCycle

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        if (CrossHair == null) {
            CrossHair.SetActive(false);
        } else {
            Debug.Log("[UIManager]CrossHair未设置");
        }
        PlayerHurtView.SetActive(false);
    }

    private void Update() {
        var player = gameManager.LocalPlayer;
        if (player == null) {
            // 当前不在游戏阶段
            return;
        }
        var p = player.GetComponent<ThirdPersonController>();
        CrossHair.SetActive(p.IsFire);

        if (p.CurrHealth < _lastHealth) {
            StartCoroutine(ShowHurtViewCoroutine());
        }
        _lastHealth = p.CurrHealth;

    }
    #endregion

    private IEnumerator ShowHurtViewCoroutine() {
        PlayerHurtView.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        PlayerHurtView.SetActive(false);
    }

}
