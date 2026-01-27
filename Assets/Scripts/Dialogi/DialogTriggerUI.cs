using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    public TutorialTalk talker;
    public int ID;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        talker.currentID = ID;
        talker.DialogStart = true;
        Destroy(gameObject);
    }
}
