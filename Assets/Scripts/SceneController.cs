using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    
    // public Animator transition;
    [SerializeField]
    private float transitionTime;
    //[SerializeField]
    //private GameObject winText;

    public void LoadNextLevel(){
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator LoadLevel(int index){ // coroutine

        if (index < SceneManager.sceneCountInBuildSettings){
            yield return new WaitForSeconds(transitionTime);
            SceneManager.LoadScene(index);
        } 
        
        //else {
        //    winText.SetActive(true);
        //}
    }

}
