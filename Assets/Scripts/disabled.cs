using UnityEngine;

public class disabled : MonoBehaviour
{
    [SerializeField]
    GameObject gm;
    private void OnDisable()
    {
        gm.SetActive(true);
    }
}
