// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;
// using UnityEngine.XR.Interaction.Toolkit.Interactors;
// using System.Collections.Generic;

// public class XRHighlight : MonoBehaviour
// {
//     private Dictionary<Material, Color> originalColors = new Dictionary<Material, Color>();
//     public bool highlight = true;
//     public int countofClicks;

//     // Function to highlight all materials in red
//     public void Highlight()
//     {
//         if (!highlight)
//         {
//             Renderer[] renderers = GetComponentsInChildren<Renderer>();
//             foreach (Renderer renderer in renderers)
//             {
//                 foreach (Material material in renderer.materials)
//                 {
//                     // Store the original color if not already stored
//                     if (!originalColors.ContainsKey(material))
//                     {
//                         originalColors[material] = material.color;
//                     }
//                     // Set material color to red
//                     material.color = Color.red;
//                 }
//             }
//             highlight = true;
//             GameManager.instance.clickCount += 1;
//             if (GameManager.instance.clickCount > 5)
//             {
//                // GameManager.instance.clickonEachEnd();
//             }
//             Debug.Log("ht");
//         }
//         else
//         {
//             Debug.Log("ht");

//             Renderer[] renderers = GetComponentsInChildren<Renderer>();
//             foreach (Renderer renderer in renderers)
//             {
//                 foreach (Material material in renderer.materials)
//                 {
//                     // Restore original color if it exists in the dictionary
//                     if (originalColors.ContainsKey(material))
//                     {
//                         material.color = originalColors[material];
//                     }
//                 }
//             }
//             highlight = false;
//             countofClicks -= 1;
//         }
       
//     }

//     // Function to reset all materials to their original color
//     public void Unlight()
//     {
       
//     }
// }
