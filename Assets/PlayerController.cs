using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{   

    public Camera Camera;
    
    public float MinDragDistancePixel = 5f;
    public float MinDragDistanceWorld = 0.1f;
    
    bool dragSelecting = false;
    Vector2 dragStartPixel;
    Vector2 dragStartWorld;
    List<MovableInfo> selected = new List<MovableInfo>(); 
    
    Vector2 mousePosWorld => Camera.ScreenToWorldPoint(Input.mousePosition);
    Vector2 mousePosPixel => Input.mousePosition;
    int movableLayerMask; 

    void Start()
    {
        movableLayerMask = LayerMask.GetMask("Movable");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
        {
            dragStartPixel = Input.mousePosition;
            dragStartWorld = mousePosWorld;

            if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftAlt))
            {
                selected.Clear();    
            }

            Collider2D collider = Physics2D.OverlapPoint(mousePosWorld, movableLayerMask);
            
            selected.Add(new MovableInfo
            {
                Transform = collider.transform,
                Offset = Vector2.zero
            });
        }
        
        if (Input.GetMouseButton((int)MouseButton.LeftMouse))
        {
            if (selected.Count > 0)
            {
                
            }
            else if (!dragSelecting)
            {
                var pixelDist = (mousePosPixel - dragStartPixel).magnitude;
                var worldDist = (mousePosWorld - dragStartWorld).magnitude;

                dragSelecting = pixelDist >= MinDragDistancePixel || worldDist >= MinDragDistanceWorld;
            }
            if (dragSelecting)
            {
                
            }
        }

        if (Input.GetMouseButtonUp((int) MouseButton.LeftMouse))
        {
            List<MovableInfo> selectedMovables = new List<MovableInfo>();
            
            if (dragSelecting)
            {
                // Collider2D[] colliders = Physics2D.OverlapPointAll(mousePosWorld, movableLayerMask);
                // foreach (var collider in colliders)
                // {
                //     selectedMovables.Add(new MovableInfo
                //     {
                //         Transform = collider.transform,
                //         Offset = Vector2.zero
                //     });
                // }
                //
                dragSelecting = false;
            }
            
            selected = selectedMovables;
            // TODO ctrl/alt functionality
            // if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftAlt))
            // {
            //     selected.Clear();                
            // }
        }
    }

    private class MovableInfo
    {
        public Transform Transform;
        public Vector2 Offset;
    }
}
