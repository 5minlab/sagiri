using System;
using UnityEngine;

public class Main : MonoBehaviour {
    // Use this for initialization
    void Start() {
        Debug.Log("Start");
    }

    private void OnDestroy() {
        Debug.Log("OnDestory");
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetButtonDown("Fire1")) {
            PrintLog();
        }
    }

    void PrintLog() {
        Debug.Log("this is log", this);
        Debug.LogWarning("this is warning", this);
        Debug.LogError("this is error", this);
        Debug.LogException(new NullReferenceException("this is exception"), this);
    }
}
