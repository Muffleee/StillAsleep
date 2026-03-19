using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [SerializeField] private Texture2D cursorNormal;
    [SerializeField] private Texture2D cursorClick;
    private Vector2 clickspot = new Vector2(20, 0);
    void Start()
    {
        Cursor.SetCursor(cursorNormal, clickspot, CursorMode.Auto);
    }
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Cursor.SetCursor(cursorClick, clickspot, CursorMode.Auto);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Cursor.SetCursor(cursorNormal, clickspot, CursorMode.Auto);
        }
    }
}
