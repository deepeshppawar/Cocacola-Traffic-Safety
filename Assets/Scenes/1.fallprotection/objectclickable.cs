// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.XR.Interaction.Toolkit;
// using UnityEngine.XR.Interaction.Toolkit.Interactables;

// public class objectclickable : MonoBehaviour
// {
//     public GameObject parent;
//     public InputActionProperty actionProperty;
//     public GameManager gameManager;
//     private string green = "#79E668";// green
//     private string red = "#C23333";// red
//     // Start is called once before the first execution of Update after the MonoBehaviour is created

//     // Update is called once per frame
//     void Update()
//     {
//         if (!gameManager.enableobj) return;
//         if (actionProperty.action.WasPressedThisFrame())
//         {
//             //Debug.Log("pressed");
//             CheckForObjectClick();
//         }
//     }
//     private void CheckForObjectClick()
//     {


//         // Perform a raycast from the controller
//         if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity))
//         {
//             if (hit.collider.gameObject.CompareTag("Player"))
//             {


//                 if (gameManager.moduleindex == 1)
//                 {
//                     hit.collider.transform.GetChild(0).gameObject.SetActive(true);

//                     hit.collider.transform.GetChild(0).SetParent(parent.transform);
//                     hit.collider.gameObject.SetActive(false);
//                 }


//                 else if (gameManager.moduleindex == 2)
//                 {
//                     characters(hit.collider.gameObject);

//                 }



//                 /* else if (hit.collider.gameObject.CompareTag("Wrong") && gameManager.enableobj == true)
//                  {

//                      if (gameManager.moduleindex != 2)

//                      {
//                          Color newColor = new Color();
//                          if (ColorUtility.TryParseHtmlString(red, out newColor))
//                          {

//                              hit.collider.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = newColor;
//                              //hit.collider.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
//                              hit.collider.transform.GetChild(0).gameObject.SetActive(true);

//                          }
//                          if (gameManager.moduleindex == 1)
//                          {   hit.collider.transform.GetChild(0).SetParent(parent.transform);
//                              hit.collider.gameObject.SetActive(false);
//                          }
//                          else
//                          {
//                              hit.collider.GetComponent<XRSimpleInteractable>().enabled = false;
//                          }
//                      }
//                      else//  for module2
//                      {
//                          Color newColor = new Color();
//                          if (ColorUtility.TryParseHtmlString(red, out newColor))
//                          {
//                              Debug.Log("red");
//                              hit.collider.GetComponent<SpriteRenderer>().color = newColor;

//                          }

//                      }

//                  }*/


//             }
//             void characters(GameObject gm)
//             {
//                /* if (gameManager.moduleindex == 1 && gameManager.selectionindex == 9)
//                      {

//                           // first module over screen
//                           gameManager.Screens[2].SetActive(true);// success of mod1
//                           gameManager.Screens[1].SetActive(false);
//                           // gameManager.enableobj = false;
//                       }
//                       else */
//                     if (gameManager.moduleindex == 2)
//                     {
//                         if (gm.name == "fault1")
//                         {
//                             gameManager.mod2railing.SetActive(true);
//                       gm.  GetComponent<Collider>().enabled = false;    
                        
//                         }
//                         else if (gm.name == "fault2")
//                         {

//                             gameManager.solveerror1(gm);
//                        gm. GetComponent<Collider>().enabled = false;
                      

//                          }
//                     else if (gm.name == "fault3")
//                         {
//                             gm.SetActive(false);
//                       gm.  GetComponent<Collider>().enabled = false;

//                          }
//                     else if (gm.name == "harness")
//                         {
//                             gm.SetActive(false);
                          
//                         } 
//                         else if (gm.name == "helmet")
//                         {
//                             gm.SetActive(false);
//                         }
//                         else if (gm.name == "cagedoor"&&gameManager.selectionindex==3)
//                         {
//                             gm.GetComponent<Collider>().enabled = false;
//                             gameManager.climbworker.SetActive(true);
//                             gameManager.modelworker.SetActive(false);
                       
//                         } 
//                     else if (gm.name == "truck1")// scene3
//                         {
                           
//                             gameManager.truck1.SetActive(true);
                           
//                             gm.GetComponent<Collider>().enabled = false;

//                     }
//                     else if (gm.name == "truck2")// scene3

//                         {
//                             gm.GetComponent<Collider>().enabled = false;
//                             gameManager.truck2.SetActive(true);
                           
                       
//                         }
//                          else if (gm.name == "roof")// scene4

//                         {
//                             gm.GetComponent<Collider>().enabled = false;
//                             gameManager.railing.SetActive(true);
//                         gameManager.clickCount = 1;
                           
                       
//                         }
//                          else if (gm.name == "solar1")// scene4

//                         {
//                             gm.GetComponent<Collider>().enabled = false;
//                             gameManager.solar1.SetActive(true);
                           
                       
//                         }
//                          else if (gm.name == "solar2")// scene4

//                         {
//                             gm.GetComponent<Collider>().enabled = false;
//                             gameManager.solar2.SetActive(true);
                           
                       
//                         }

                     

                       
//                         //  gameManager.Screens[4].SetActive(true);

//                     }
//                    /* else if (gameManager.moduleindex == 3)
//                     {
//                         //add cap and harness
//                         gm.GetComponent<DragDropSnap>().SnapToTarget();
//                         gm.transform.GetChild(0).gameObject.SetActive(false);
//                     }
//                     else if (gameManager.moduleindex == 5)
//                     {
//                         gm.GetComponent<XRSimpleInteractable>().enabled = false;
//                         gm.GetComponent<BoxCollider>().enabled = false;

//                         if (gm.name == "Streacher")
//                         {
//                             gm.transform.GetChild(0).gameObject.SetActive(false);
//                             gameManager.retrieveWorker.SetActive(true);
//                             gameManager.fallenworker.SetActive(false);
//                         }
//                         if (gameManager.selectionindex == 2)
//                         {
//                             gameManager.Screens[8].SetActive(true);
//                             gameManager.enableobj = false;
//                             // gameManager.emergency.SetActive(false);
//                         }

//                     }
//                     else if (gameManager.moduleindex == 6)
//                     {
//                         gm.GetComponent<XRSimpleInteractable>().enabled = false;
//                         gm.GetComponent<BoxCollider>().enabled = false;

//                         if (gameManager.selectionindex == 2)
//                         {
//                             gameManager.Screens[9].SetActive(true);
//                             gameManager.enableobj = false;
//                         }

//                     }
//                 // next 
//                 gameManager.selectionindex++;*/
//             }

//         }
//     }
// }
