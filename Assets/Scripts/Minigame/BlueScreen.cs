using UnityEngine;
using System.Collections;

public class BlueScreen : MonoBehaviour
{
    public GameObject firstObject;
    public GameObject secondObject;
    public float switchTime = 2f;

    private Coroutine loop;

    void OnEnable()
    {
        if (firstObject != null) firstObject.SetActive(true);
        if (secondObject != null) secondObject.SetActive(false);

        loop = StartCoroutine(SwitchLoop());
    }

    void OnDisable()
    {
        if (loop != null)
            StopCoroutine(loop);
    }

    IEnumerator SwitchLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(switchTime);

            bool firstActive = firstObject.activeSelf;
            firstObject.SetActive(!firstActive);
            secondObject.SetActive(firstActive);
        }
    }
}
