using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace FreeCell
{
    /// <summary>
    /// This class implements the buttons to interact with the SC Rummy game
    /// </summary>
    public class SCR_GUI : MonoBehaviour
    {

        void OnGUI()
        {
            if (GUI.Button(new Rect(10, Screen.height - 60, 50, 50), "Undo"))
                FreeCellBehavior.Instance.Undo();

            if (GUI.Button(new Rect(70, Screen.height - 60, 90, 50), "New Game"))
            {
                StaircaseRummyBehavior.MaxStaircaseStack = 9;
                StaircaseRummyBehavior.Suits = 8;
                StaircaseRummyBehavior.Instance.StartCoroutine(StaircaseRummyBehavior.Instance.NewGame(UnityEngine.Random.Range(1, 20000)));
            }
        }
    }
}