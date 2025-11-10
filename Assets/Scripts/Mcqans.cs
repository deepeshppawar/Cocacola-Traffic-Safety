using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Mcqans : MonoBehaviour
{
    public int ans;
    private string green = "#79E668";// green
    private string red = "#C23333";// red
    [SerializeField]
    Button Button; 
    [SerializeField]
    TMP_Text text;
    [SerializeField]
    AudioSource source;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void playclip(AudioClip clip)// play sound
    {
      source.clip = clip;
        source.Play();
    }
    public void changecolor(Button button)// true ans
    {
        Color newColor = new Color();

        if (ColorUtility.TryParseHtmlString(green, out newColor))
           
                button.GetComponent<Image>().color = newColor;
                button.interactable = false;
           
               
    }
    public void greenring(GameObject gm)// change ring color
    {
        Color newColor = new Color();

        if (ColorUtility.TryParseHtmlString(green, out newColor))
            gm.GetComponent<SpriteRenderer>().color = newColor;
    }
    public void redring(GameObject gm)// change ring color
    {
        Color newColor = new Color();

        if (ColorUtility.TryParseHtmlString(red, out newColor))
            gm.GetComponent<SpriteRenderer>().color = newColor;
    }
    public void changecolorf(Button button)
    {
        Color newColor = new Color();// falseans
        
        if (ColorUtility.TryParseHtmlString(red, out newColor))
           
            button.  GetComponent<Image>().color =  newColor;
    button .interactable = false;
           
    }
   public void changestring(string texts)// correct or incorrect
    {
        text.text = texts;

    }
    IEnumerator timer(int seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
    public void procced(int i)
    {
        if (ans == i)
        {
            
            this.gameObject.SetActive(false);
            ans = 0;
        }

    }
    public void correctans(bool tf)
    {
        string quest = "";
        if (text != null)
        {
            quest = text.text;

        }
        if (tf)
        {
            ans++;
            if(text != null)
            text.text = "Correct";

        }
        else
        {
            ans = 0;
            if (text != null)

                text.text = "Incorrect!";
        //  StartCoroutine(  timer(3));
          //  text.text= quest;
        }
    }
   
}
