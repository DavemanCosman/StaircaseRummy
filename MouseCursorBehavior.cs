using UnityEngine;
using System.Collections;
using System;

namespace FreeCell
{
    /// <summary>
    /// This class simply makes the game object follow the mouse position. It is used when dragging cards. Dragged cards are placed on it as children.
    /// </summary>
    public class MouseCursorBehavior : MonoBehaviour
    {
        void Update()
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 1000);

            //print(Input.mousePosition);
            //print(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)));
        }
    }
}
