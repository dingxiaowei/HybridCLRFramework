using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RpgLitLoader : MonoBehaviour
{
    public JoyStickMove JoyStickController;
    void Start()
    {
        JoyStickController.onMoveStart += OnMoveStart;
        JoyStickController.onMoving += OnMoving;
        JoyStickController.onMoveEnd += OnMoveEnd;
    }

    void OnMoveStart()
    {
        //Debug.Log("OnMoveStart");
    }

    void OnMoving(Vector2 vector2Move)
    {
        //Debug.Log($"OnMoving  x:{vector2Move.x} y:{vector2Move.y}");
    }

    void OnMoveEnd()
    {
        //Debug.Log("OnMoveEnd");
    }

    private void OnDestroy()
    {
        JoyStickController.onMoveStart -= OnMoveStart;
        JoyStickController.onMoving -= OnMoving;
        JoyStickController.onMoveEnd -= OnMoveEnd;
    }
}
