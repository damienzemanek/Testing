using System.Collections;
using System.Collections.Generic;
using EMILtools.Extensions;
using Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using static EMILtools.Extensions.FadeEX;


public class LoadScene : MonoBehaviour
{
    [SerializeField] float minTimeLoading = 1f;
    [SerializeField] FadeSettings fade;

    private void OnDisable()
    {
        fade.SetAlpha(0);
        print("a");
    }

    public void LoadSceneFadeScreenToOpaque(int num)
    {
        StartCoroutine(C_FadeToOpaque(fade, 
            () => StartCoroutine(Load(num))));
    }

    public GameObject[] LoadingScreenObjects;
    public GameObject[] disables;


    IEnumerator Load(int sceneId)
    {
        LoadingScreenObjects.SetAllActive(true);
        disables.SetAllActive(false);
        yield return new WaitForSeconds(minTimeLoading);
        StartCoroutine(LoadSceneAsync(sceneId));
    }
    IEnumerator LoadSceneAsync(int sceneId)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);

        while(!operation.isDone)
        {
            yield return null;
        }

        fade.SetAlpha(0);
    }
}
