using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;



public class GameManager : MonoBehaviour// cocacola base scene
{
    [SerializeField]
    Transform[] positions; 
    
  

    [SerializeField]
    Sprite tick;
    public int error;
    // module 2 error
    public GameObject roof;
    public GameObject mod2railing;
    public GameObject climbworker;
    public GameObject modelworker;
    public GameObject player;// module2

    //module2 scene 3
    public GameObject truck1;
    public GameObject truck2;

    // module2 scene4
    public GameObject railing;
    public GameObject solar1;
    public GameObject solar2;

    // video player
    public RenderTexture[] render_Texture;
   
    public Button proceed;
    public VideoClip[] clips;
    public GameObject playerobject;
    private VideoPlayer vid;
    public GameObject[] instructions;

    public GameObject retrieveWorker;// module 5
    public GameObject fallenworker;// module 5
  
    public int moduleindex;
   public  objectclickable[] objects;
    private string green = "#79E668";// green
    private string red = "#C23333";// red
    public Button[] buttons;//module4
    public GameObject character;// module 1
    public GameObject safty;// module 3
    public GameObject superwiser;// module 4
    public GameObject emergency;//module 5
    public GameObject falsegaz;//module 5

    public TMP_Text text;//
   public  int  selectionindex;
    public bool enableobj;
    public GameObject[] Screens;
    public GameObject[] ClickableWorkers;
    public Vector3[] position;
    public Vector3[] rotation;

    public int clickCount;
  

    public static GameManager instance;
    Color newColor;

    // void Awake () { instance = this;
    //   //  workertick();
    //     //audio = GetComponent<AudioSource>();    
    // }

    // Start is called before the first frame update

    public void clickonEachBegin(GameObject gm)// add on  ui 7 
    {
        moduleindex = 1;
        enableobj= true;    
        foreach (GameObject worker in ClickableWorkers)
        {
            if (worker.GetComponent<XRSimpleInteractable>() != null)
                worker.GetComponent<XRSimpleInteractable>().enabled = true;

        }
        objects[0].parent = character;
        objects[1].parent = character;
        gm.SetActive(false);
           
       
    }
 void workertick()
    {
        foreach (GameObject worker in ClickableWorkers)
        {
            if (worker.transform.GetChild(0).GetComponent<MeshRenderer>() != null)
                worker.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;

        }
    }
    public void OnClick(GameObject gm)// module1
    {
 /* if (ColorUtility.TryParseHtmlString("#829FB1", out newColor))         
         gm.GetComponent<MeshRenderer>().material.color=newColor;
         gm. GetComponent<MeshRenderer>().enabled = true;*/
     
    }  
    public void Onexit(GameObject gm)// module1
    {  
      /*  if(gm.GetComponent<MeshRenderer>().material.color==newColor)
        gm. GetComponent<MeshRenderer>().enabled = false;
     */
    } 
   
   
    public void module2Begin()// module 2 begin added on  mod1 success
    {
        moduleindex = 2;
        //character.SetActive(false);// all characyer false
       
        selectionindex = 0;
     enableobj = true;
        player.transform.position = positions[0].position;
        player.transform.rotation = positions[0].rotation;
       // objects[0].parent = places;
        //objects[1].parent = places;
     
    }
    public void module3()
    {
        moduleindex = 3;
        player.SetActive(false);// all characyer false
        safty.SetActive(true);
        selectionindex = 0;
        objects[0].parent = safty;
        objects[1].parent = safty;
    }
    public void module4(TMP_Text text)
    {
      selectionindex = 0;
        moduleindex = 4;
        enableobj = false;
       
        Color newcolor;
        ColorUtility.TryParseHtmlString(green, out newcolor);
       
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].GetComponent<Image>().sprite==tick)
            {
                selectionindex +=1;
            }
        }
        if (selectionindex == 8)
        {
            Screens[6].SetActive(true);
            selectionindex = 0;
            Screens[7].SetActive(false);
            superwiser.SetActive(false );
        }
        else
        {
            text.text = "Please tick all the checkboxes or Sign the form";
            text.color= Color.green;
        }
    }

    public void buttoncolor(Button btn)// other button off right/left// module 4

    {
        btn.GetComponent<Image>().sprite = tick;
        
    }
      public void buttonpressedgreen(Button btn)// green or red
    {
     
        Color newColor;
        if (ColorUtility.TryParseHtmlString(green, out newColor))
        {
            Image im =btn. GetComponent<Image>();
            im.color = newColor; // Set the text color
        }
        
    } public void buttonpressedred(Button btn)// green or red
    {
     
        Color newColor;
        if (ColorUtility.TryParseHtmlString(red, out newColor))
        {
            Image im =btn. GetComponent<Image>();
            im.color = newColor; // Set the text color
        }
        
    }

 public void module5()
    {
        emergency.SetActive(true);
        moduleindex = 5;
        objects[0].parent = emergency;
        objects[1].parent = emergency;
        enableobj = true;
    }
    public void module6()
    {
        selectionindex = 0; 
        falsegaz.SetActive(true);
        moduleindex = 6;
        objects[0].parent = falsegaz;
        objects[1].parent = falsegaz;
        enableobj = true;
    }
    public void videopaly()
    {
        proceed.gameObject.SetActive(false);


        ClearRenderTexture();
        vid.clip = clips[moduleindex];
      //  vid.audioOutputMode = VideoAudioOutputMode.AudioSource;
        //vid.SetTargetAudioSource(0,audio);
     // audio.Play();   
       // vid.Play();
      proceed.gameObject.SetActive(true);   
      vid.loopPointReached +=OnVideoEnd;
    }
  void OnVideoEnd(VideoPlayer vp)
    {
       
      proceed.gameObject.SetActive(true);
        Destroy(vid);
    }
    public void afterplay()
    {
        instructions[moduleindex].SetActive(true);
        if (vid != null)
        {
            Destroy(vid);
            vid = null;
        }
        // module inst penal on
    }
    void ClearRenderTexture()
    {
       
     vid = playerobject.AddComponent<VideoPlayer>();
    // vid.playOnAwake = false;
        //    RenderTexture   renderTexture = new RenderTexture(render_Texture.width, render_Texture.height, 24);
       vid.targetTexture = render_Texture[moduleindex];
       vid.GetComponent<RawImage>().texture = render_Texture[moduleindex];
        
    }
    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stops play mode in the editor
#endif
    }
    public void solveerror1(GameObject gm)
    {
       
          gm.transform.rotation = Quaternion.Euler(new Vector3(0f,0f,0f));
        roof.SetActive(true);
        
    } 
    public void solveerror2()
    {
           error++;
        if (error == 2&&selectionindex==6)
            Screens[4].SetActive(true);
    }
    // new code
    public void  changeviewport(int number)
    {
       enableobj=true;

        if (selectionindex >= 2)
        {
            selectionindex = 0;

            player.transform.position =positions[number].position;
            player.transform.rotation=positions[number].rotation;
            if (clickCount > 0)
            { Screens[4].SetActive(true);
                enableobj = false;
            }
                // module end
        }

    }
}
