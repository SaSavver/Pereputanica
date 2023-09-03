using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loader : MonoBehaviour {

    public Image bar;
	
	void Start () {
        StartCoroutine(LoadLevel());

    }

    IEnumerator LoadLevel()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(1);
        while(!async.isDone)
        {
            bar.fillAmount = async.progress;
            yield return null;
        }
        bar.fillAmount = async.progress;
    }



}
