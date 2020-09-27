using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour
{   

    public Camera Camera;
    public RectTransform SelectionImage;
    
    public float MinDragDistancePixel = 5f;
    public float MinDragDistanceWorld = 0.1f;

    bool dragSelecting = false;
    Vector2 dragStartPixel;
    Vector2 dragStartWorld;
    List<MovableInfo> selected = new List<MovableInfo>(); 
    
    Vector2 mousePosWorld => Camera.ScreenToWorldPoint(Input.mousePosition);
    Vector2 mousePosPixel => Input.mousePosition;
    bool ctrl => Input.GetKey(KeyCode.LeftControl);
    bool alt => Input.GetKey(KeyCode.LeftAlt);
    
    int movableLayerMask; 

    public float OutlineWidth = 1.5f;
    
    void Start()
    {
        movableLayerMask = LayerMask.GetMask("Movable");
    }

    void AddToSelected(MovableInfo info)
    {
        selected.Add(info);
        var position = info.Transform.position;
        position.z = OutlineWidth;
        info.Transform.position = position;
    }

    void UpdateOffset(MovableInfo info)
    {
        info.Offset = (Vector2)info.Transform.position - mousePosWorld;
    }
    
    void RemoveFromSelected(MovableInfo info)
    {
        selected.Remove(info);
        var position = info.Transform.position;
        position.z = 0;
        info.Transform.position = position;
    }

    void ClearSelected()
    {
        for (int i = selected.Count - 1; i >= 0; i--)
        {
            RemoveFromSelected(selected[i]);
        }
    }

    void SelectCollider(Collider2D collider, List<MovableInfo> add, List<MovableInfo> remove)
    {
        var existing = selected.FirstOrDefault(s => s.Transform == collider.transform);
        if (alt)
        {
            if (existing != null)
            {
                remove.Add(existing);
            }
        }
        else if (existing == null)
        {
            var info = new MovableInfo
            {
                Transform = collider.transform,
                Offset = (Vector2)collider.transform.position - mousePosWorld
            };
            add.Add(info);
        }
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
        {
            dragStartPixel = Input.mousePosition;
            dragStartWorld = mousePosWorld;
            
            List<MovableInfo> toAdd = new List<MovableInfo>();
            List<MovableInfo> toRemove = new List<MovableInfo>();
            
            Collider2D collider = Physics2D.OverlapPoint(mousePosWorld, movableLayerMask);
            if (collider != null)
            {
                SelectCollider(collider, toAdd, toRemove);
                if (toRemove.Any())
                {
                    RemoveFromSelected(toRemove[0]);
                }
                if (toAdd.Any())
                {
                    if (!ctrl) ClearSelected();
                    AddToSelected(toAdd[0]);
                }
                foreach (var select in selected)
                {
                    UpdateOffset(select);
                }
            }
            else
            {
                ClearSelected();
            }
        }
        
        if (Input.GetMouseButton((int)MouseButton.LeftMouse))
        {
            if (selected.Count > 0)
            {
                foreach (var select in selected)
                {
                    var position = (Vector3)mousePosWorld + select.Offset;
                    position.z = select.Transform.position.z;
                    select.Transform.position = position;
                }
            }
            else if (!dragSelecting)
            {
                var pixelDist = (mousePosPixel - dragStartPixel).magnitude;
                var worldDist = (mousePosWorld - dragStartWorld).magnitude;
        
                dragSelecting = pixelDist >= MinDragDistancePixel || worldDist >= MinDragDistanceWorld;
            }
            if (dragSelecting)
            {
                SelectionImage.gameObject.SetActive(true);
                Vector2 topLeft = dragStartPixel;
                Vector2 bottomRight = mousePosPixel;

                SelectionImage.position = (topLeft + bottomRight) * 0.5f;
                
                Vector2 size = bottomRight - topLeft;
                size.x = Mathf.Abs(size.x);
                size.y = Mathf.Abs(size.y);
                SelectionImage.sizeDelta = size;
            }
        }
        
        if (Input.GetMouseButtonUp((int) MouseButton.LeftMouse))
        {
            if (dragSelecting)
            {
                if (!ctrl && !alt)
                {
                    ClearSelected();
                }
                
                List<MovableInfo> toAdd = new List<MovableInfo>();
                List<MovableInfo> toRemove = new List<MovableInfo>();

                var position = (dragStartWorld + mousePosWorld) * 0.5f;
                var size = mousePosWorld - dragStartWorld;
                size.x = Mathf.Abs(size.x);
                size.y = Mathf.Abs(size.y);
                Collider2D[] colliders = Physics2D.OverlapBoxAll(position, size, 0f, movableLayerMask);
                print(colliders.Length);
                foreach (var collider in colliders)
                {
                    SelectCollider(collider, toAdd, toRemove);
                }

                foreach (var add in toAdd)
                {
                    AddToSelected(add);
                }

                foreach (var remove in toRemove)
                {
                    RemoveFromSelected(remove);
                }
                
                dragSelecting = false;
                SelectionImage.gameObject.SetActive(false);
            }
        }

        if (!dragSelecting)
        {
            Camera.orthographicSize -= Input.mouseScrollDelta.y * 0.5f;    
        }
    }

    void OnDrawGizmos()
    {
        // Gizmos.DrawCube((dragStartWorld + mousePosWorld) * 0.5f, mousePosWorld - dragStartWorld);
    }

    private class MovableInfo
    {
        public Transform Transform;
        public Vector3 Offset;
    }
}
