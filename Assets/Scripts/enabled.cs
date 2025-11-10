using UnityEngine;

public class enabled : MonoBehaviour// add this on all ticks

{
    [SerializeField]
    GameObject gm;
    [SerializeField]
    GameManager gameManager;
    private void OnEnable()
    {
        gameManager.enableobj = false;
        if(gm != null ) 
        gm.SetActive(true);
    }


    public void enable()// last procced button
    {

        gameManager.selectionindex++;
        if (gameManager.selectionindex >= 7)
        {

            gameManager.Screens[2].SetActive(true);
            gameManager.selectionindex = 0;
        }
        else
            gameManager.enableobj = true;

    }
}
