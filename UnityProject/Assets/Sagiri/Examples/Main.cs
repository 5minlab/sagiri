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

    int rightClickCount = 0;

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            // left
            PrintLog();
        }

        if(Input.GetMouseButtonDown(1)) {
            // right
            rightClickCount += 1;
            Debug.LogFormat("right click : {0}", rightClickCount);
        }
    }

    void PrintLog() {
        Debug.Log("this is log", this);
        Debug.LogWarning("this is warning", this);
        Debug.LogError("this is error", this);
        Debug.LogException(new NullReferenceException("this is exception"), this);
    }
}
