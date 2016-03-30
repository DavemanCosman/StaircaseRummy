using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace FreeCell
{
    /// <summary>
    /// This class implements a few buttons to interact with the freecell game.
    /// </summary>
    public class ExampleGui : MonoBehaviour
    {
        void OnGUI()
        {
            GUI.Label(new Rect(10, Screen.height - 80, 1000, 100), "Right click to auto complete cards. Double click to move card to a free cell.");
            if (GUI.Button(new Rect(10, Screen.height - 60, 50, 50), "Undo"))
                FreeCellBehavior.Instance.Undo();

            if (GUI.Button(new Rect(70, Screen.height - 60, 90, 50), "New Normal"))
            {
                FreeCellBehavior.Cells = 4;
                FreeCellBehavior.Suits = 4;
                FreeCellBehavior.Stacks = 8;
                FreeCellBehavior.Instance.StartCoroutine(FreeCellBehavior.Instance.NewGame(UnityEngine.Random.Range(1, 20000)));
            }

            if (GUI.Button(new Rect(170, Screen.height - 60, 90, 50), "New Hard"))
            {
                FreeCellBehavior.Cells = 6;
                FreeCellBehavior.Suits = 8;
                FreeCellBehavior.Stacks = 12;
                FreeCellBehavior.Difficulty = 2;

                FreeCellBehavior.Instance.StartCoroutine(FreeCellBehavior.Instance.NewGame(UnityEngine.Random.Range(1, 20000)));
            }

            if (GUI.Button(new Rect(270, Screen.height - 60, 90, 50), "New Easy"))
            {
                FreeCellBehavior.Cells = 1;
                FreeCellBehavior.Suits = 2;
                FreeCellBehavior.Stacks = 8;
                FreeCellBehavior.Difficulty = -1;

                FreeCellBehavior.Instance.StartCoroutine(FreeCellBehavior.Instance.NewGame(UnityEngine.Random.Range(1, 20000)));
            }
        }
    }
}