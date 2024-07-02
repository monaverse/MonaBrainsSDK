using System.Collections;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class TileDragAndDropManipulator : PointerManipulator
{
    // Write a constructor to set target and store a reference to the
    // root of the visual tree.
    public TileDragAndDropManipulator(VisualElement target, VisualElement root, MonaBrainGraphVisualElement search)
    {
        this.target = target;
        _root = root;
        _search = search;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        // Register the four callbacks on target.
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        // Un-register the four callbacks from target.
        target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
        target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
    }

    private Vector2 targetStartPosition { get; set; }

    private Vector3 pointerStartPosition { get; set; }

    private bool enabled { get; set; }

    private VisualElement _root { get; }
    private MonaBrainGraphVisualElement _search { get; }

    private TileMenuItemVisualElement _clone;

    // This method stores the starting position of target and the pointer,
    // makes target capture the pointer, and denotes that a drag is now in progress.
    private void PointerDownHandler(PointerDownEvent evt)
    {
        targetStartPosition = target.transform.position;
        pointerStartPosition = evt.position;

        target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.CapturePointer(evt.pointerId);
        enabled = true;
        //Debug.Log($"{nameof(PointerDownHandler)} enabled");
    }

    // This method checks whether a drag is in progress and whether target has captured the pointer.
    // If both are true, calculates a new position for target within the bounds of the window.
    private void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (enabled && (target.HasPointerCapture(evt.pointerId) || (_clone != null && _clone.HasPointerCapture(evt.pointerId))))
        {
            Vector3 pointerDelta = evt.position - pointerStartPosition;

            if (_clone == null)
            {
                var tileItem = (TileMenuItemVisualElement)target;
                _clone = new TileMenuItemVisualElement(tileItem.Root, tileItem.Search, false);
                _clone.style.width = tileItem.resolvedStyle.width;
                _clone.style.height = tileItem.resolvedStyle.height;
                _clone.SetItem(tileItem.Item);

                target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
                target.ReleasePointer(evt.pointerId);

                _root.Add(_clone);

                _clone.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
                _clone.RegisterCallback<PointerUpEvent>(PointerUpHandler);
                _clone.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
                _clone.CapturePointer(evt.pointerId);


                targetStartPosition = target.worldTransform.GetPosition();
            }

            _clone.transform.position = new Vector2(
                Mathf.Clamp(targetStartPosition.x - _root.worldTransform.GetPosition().x + pointerDelta.x, 0, target.panel.visualTree.worldBound.width),
                Mathf.Clamp(targetStartPosition.y - _root.worldTransform.GetPosition().y + pointerDelta.y, 0, target.panel.visualTree.worldBound.height));
            //Debug.Log($"{nameof(PointerMoveHandler)} clone {_clone.transform.position}");
        }
    }

    // This method checks whether a drag is in progress and whether target has captured the pointer.
    // If both are true, makes target release the pointer.
    private void PointerUpHandler(PointerUpEvent evt)
    {
        if (enabled && (target.HasPointerCapture(evt.pointerId) || (_clone != null && _clone.HasPointerCapture(evt.pointerId))))
        {
            if (_clone != null)
            {
                _clone.ReleasePointer(evt.pointerId);
                _clone.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
            }
            target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.ReleasePointer(evt.pointerId);
        }
    }

    // This method checks whether a drag is in progress. If true, queries the root
    // of the visual tree to find all slots, decides which slot is the closest one
    // that overlaps target, and sets the position of target so that it rests on top
    // of that slot. Sets the position of target back to its original position
    // if there is no overlapping slot.
    private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
    {
        if (enabled)
        {
            List<MonaInstructionTileVisualElement> tiles = new List<MonaInstructionTileVisualElement>();
            for(var i = 0;i < _search.CorePage.Instructions.Count; i++)
            {
                var instructions = _search.CorePage.Instructions[i];
                tiles.AddRange(instructions.Tiles);
            }

            for (var i = 0; i < _search.ActiveStatePage.Instructions.Count; i++)
            {
                var instructions = _search.ActiveStatePage.Instructions[i];
                tiles.AddRange(instructions.Tiles);
            }

            var overlappingSlots = tiles.FindAll(x => OverlapsTarget(x));
            MonaInstructionTileVisualElement closestOverlappingTile = FindClosestTile(overlappingSlots);
            Vector3 closestPos = Vector3.zero;

            if(closestOverlappingTile == null)
            {
                var instructions = new List<MonaInstructionVisualElement>();
                for (var i = 0; i < _search.CorePage.Instructions.Count; i++)
                {
                    instructions.Add(_search.CorePage.Instructions[i]);
                }

                for (var i = 0; i < _search.ActiveStatePage.Instructions.Count; i++)
                {
                    instructions.Add(_search.ActiveStatePage.Instructions[i]);
                }

                var overlappingInstructions = instructions.FindAll(x => OverlapsTarget(x));
                MonaInstructionVisualElement closestOverlappingInstruction = FindClosestInstruction(overlappingInstructions);

                //Debug.Log($"{nameof(PointerCaptureOutHandler)} closestOverlappingInstruction {closestOverlappingInstruction != null} {closestOverlappingInstruction}");
                if(closestOverlappingInstruction != null)
                {
                    AddTileToSelectedInstructions(_clone.Item.Tile, -1, closestOverlappingInstruction.Instruction, closestOverlappingInstruction.Page);
                }

            }
            else if (closestOverlappingTile != null)
            {
                //Debug.Log($"{nameof(PointerCaptureOutHandler)} closestOverlappingTile {closestOverlappingTile?.Tile.Name}");

                closestPos = RootSpaceOfSlot(closestOverlappingTile);
                closestPos = new Vector2(closestPos.x - 5, closestPos.y - 5);

                VisualElement parent = closestOverlappingTile.parent;
                while (!(parent is MonaInstructionVisualElement) && parent != null)
                    parent = parent.parent;

                AddTileToSelectedTile(_clone.Item.Tile, ((MonaInstructionVisualElement)parent).Instruction.InstructionTiles.IndexOf(closestOverlappingTile.Tile)+1, ((MonaInstructionVisualElement)parent).Instruction, ((MonaInstructionVisualElement)parent).Page);
            }

            _clone.transform.position =
                closestOverlappingTile != null ?
                closestPos :
                targetStartPosition;

            enabled = false;
            _clone.RemoveFromHierarchy();
            _clone.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
            _clone.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
            _clone = null;
        }
    }

    private void AddTileToSelectedInstructions(IInstructionTile tile, int index, IInstruction instruction, IMonaBrainPage page)
    {
        instruction.AddTile(tile, index, page);
    }

    private void AddTileToSelectedTile(IInstructionTile tile, int index, IInstruction instruction, IMonaBrainPage page)
    {
        instruction.AddTile(tile, index, page);
    }

    private bool OverlapsTarget(VisualElement slot)
    {
        //Debug.Log($"{nameof(OverlapsTarget)} {_clone.worldBound.position} {_clone.worldBound.width} {_clone.worldBound.height} slot {slot.worldBound.position} {slot.worldBound.width} {slot.worldBound.height}");
        return _clone.worldBound.Overlaps(slot.worldBound);
    }

    private MonaInstructionTileVisualElement FindClosestTile(List<MonaInstructionTileVisualElement> slotsList)
    {
        float bestDistanceSq = float.MaxValue;
        MonaInstructionTileVisualElement closest = null;
        foreach (MonaInstructionTileVisualElement slot in slotsList)
        {
            Vector3 displacement =
                RootSpaceOfSlot(slot) - _clone.transform.position;
            float distanceSq = displacement.sqrMagnitude;
            if (distanceSq < bestDistanceSq)
            {
                bestDistanceSq = distanceSq;
                closest = slot;
            }
        }
        return closest;
    }

    private MonaInstructionVisualElement FindClosestInstruction(List<MonaInstructionVisualElement> slotsList)
    {
        float bestDistanceSq = float.MaxValue;
        MonaInstructionVisualElement closest = null;
        foreach (MonaInstructionVisualElement slot in slotsList)
        {
            Vector3 displacement =
                RootSpaceOfSlot(slot) - _clone.transform.position;
            float distanceSq = displacement.sqrMagnitude;
            if (distanceSq < bestDistanceSq)
            {
                bestDistanceSq = distanceSq;
                closest = slot;
            }
        }
        return closest;
    }


    private Vector3 RootSpaceOfSlot(VisualElement slot)
    {
        Vector2 slotWorldSpace = slot.parent.LocalToWorld(slot.layout.position);
        return _root.WorldToLocal(slotWorldSpace);
    }
}