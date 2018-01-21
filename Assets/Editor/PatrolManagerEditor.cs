using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PatrolManager))]
public class PatrolManagerEditor : Editor {

    PatrolManager patrolManager;
    SelectionInfo selectionInfo;
    bool shapeChangedSinceLastRepaint = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        int pointDeleteIndex = -1;
        patrolManager.showPointList = EditorGUILayout.Foldout(patrolManager.showPointList, "Show Point List");
        if (patrolManager.showPointList)
        {
            for (int i = 0; i < patrolManager.points.Count; i++)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label("Point " + (i + 1));

                GUI.enabled = i != selectionInfo.pointIndex;
                if (GUILayout.Button("Select"))
                {
                    selectionInfo.pointIndex = i;
                }
                GUI.enabled = true;

                if (GUILayout.Button("Delete"))
                {
                    pointDeleteIndex = i;
                }

                GUILayout.EndHorizontal();
            }
        }

        if (pointDeleteIndex != -1)
        {
            Undo.RecordObject(patrolManager, "Delete point");
            patrolManager.points.RemoveAt(pointDeleteIndex);
            selectionInfo.pointIndex = Mathf.Clamp(selectionInfo.pointIndex, 0, patrolManager.points.Count - 1);
        }

        if (GUI.changed)
        {
            shapeChangedSinceLastRepaint = true;
            SceneView.RepaintAll();
        }
    }

    void OnSceneGUI()
    {
        Event guiEvent = Event.current;

        if (guiEvent.type == EventType.Repaint)
        {
            Draw();
        }
        else if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else
        {
            HandleInput(guiEvent);
            if (shapeChangedSinceLastRepaint)
            {
                HandleUtility.Repaint();
            }
        }
    }

    void CreateNewPoint(Vector3 position)
    {
        int newPointIndex = (selectionInfo.mouseIsOverLine)
            ? selectionInfo.lineIndex + 1 : patrolManager.points.Count;

        Undo.RecordObject(patrolManager, "Add point");
        patrolManager.points.Insert(newPointIndex, position);
        selectionInfo.pointIndex = newPointIndex;

        shapeChangedSinceLastRepaint = true;

        SelectedPointUnderMouse();
    }

    void DeletePointUnderMouse()
    {
        Undo.RecordObject(patrolManager, "Delete point");
        patrolManager.points.RemoveAt(selectionInfo.pointIndex);
        selectionInfo.pointsIsSelected = false;
        selectionInfo.mouseIsOverPoint = false;
        shapeChangedSinceLastRepaint = true;
    }

    void SelectedPointUnderMouse()
    {
        selectionInfo.pointsIsSelected = true;
        selectionInfo.mouseIsOverPoint = true;
        selectionInfo.mouseIsOverLine = false;
        selectionInfo.lineIndex = -1;

        selectionInfo.positionAtStartOfDrag = patrolManager.points[selectionInfo.pointIndex];
        shapeChangedSinceLastRepaint = true;
    }


    void HandleShiftLeftMouseDown(Vector3 mousePosition)
    {
        if (selectionInfo.mouseIsOverPoint)
        {
            DeletePointUnderMouse();
        }
        else
        {
            CreateNewPoint(mousePosition);
        }
    }

    void HandleLeftMouseDown(Vector3 mousePosition)
    {
        if (selectionInfo.mouseIsOverPoint)
        {
            SelectedPointUnderMouse();
        }
    }

    void HandleLeftMouseUp(Vector3 mousePosition)
    {
        if (selectionInfo.pointsIsSelected)
        {
            patrolManager.points[selectionInfo.pointIndex] = selectionInfo.positionAtStartOfDrag;
            Undo.RecordObject(patrolManager, "Move point");
            patrolManager.points[selectionInfo.pointIndex] = mousePosition;

            selectionInfo.pointsIsSelected = false;
            selectionInfo.pointIndex = -1;
            shapeChangedSinceLastRepaint = true;
        }
    }

    void HandleLeftMouseDrag(Vector3 mousePosition)
    {
        if (selectionInfo.pointsIsSelected)
        {
            patrolManager.points[selectionInfo.pointIndex] = mousePosition;
            shapeChangedSinceLastRepaint = true;
        }
    }

    void HandleInput(Event guiEvent)
    {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        float drawPlaneHeight = 0.0f;
        float destToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
        Vector3 mousePosition = mouseRay.GetPoint(destToDrawPlane);

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift)
        {
            HandleShiftLeftMouseDown(mousePosition);
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            HandleLeftMouseDown(mousePosition);
        }

        if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
        {
            HandleLeftMouseUp(mousePosition);
        }

        if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            HandleLeftMouseDrag(mousePosition);
        }

        if (selectionInfo.pointsIsSelected == false)
        {
            UpdateMouseOverSeletion(mousePosition);
        }
    }

    void UpdateMouseOverSeletion(Vector3 mousePosition)
    {
        int mouseOverPointIndex = -1;

        for (int i = 0; i < patrolManager.points.Count; i++)
        {
            if (Vector3.Distance(mousePosition, patrolManager.points[i]) < patrolManager.handleRadius)
            {
                mouseOverPointIndex = i;
                break;
            }
        }

        if (mouseOverPointIndex != selectionInfo.pointIndex)
        {
            selectionInfo.pointIndex = mouseOverPointIndex;
            selectionInfo.mouseIsOverPoint = mouseOverPointIndex != -1;

            shapeChangedSinceLastRepaint = true;
        }

        if (selectionInfo.mouseIsOverPoint == true)
        {
            selectionInfo.mouseIsOverLine = false;
            selectionInfo.lineIndex = -1;
        }
        else
        {
            int mouseOverLineIndex = -1;
            float closestListDest = patrolManager.handleRadius;

            for (int i = 0; i < patrolManager.points.Count; i++)
            {
                Vector3 nextPointInShape = patrolManager.points[(i + 1) % patrolManager.points.Count];
                float destFromMouseToLine = HandleUtility.DistancePointToLineSegment(
                    mousePosition.ToXZ(), patrolManager.points[i].ToXZ(), nextPointInShape.ToXZ());

                if (destFromMouseToLine < closestListDest)
                {
                    closestListDest = destFromMouseToLine;
                    mouseOverLineIndex = i;
                }
            }

            if (selectionInfo.lineIndex != mouseOverLineIndex)
            {
                selectionInfo.lineIndex = mouseOverLineIndex;
                selectionInfo.mouseIsOverLine = mouseOverLineIndex != -1;
                shapeChangedSinceLastRepaint = true;
            }
        }
    }

    void Draw()
    {
        for (int i = 0; i < patrolManager.points.Count; i++)
        {
            Vector3 nextPoint = patrolManager.points[(i + 1) % patrolManager.points.Count];

            if (i == selectionInfo.lineIndex)
            {
                Handles.color = Color.red;
                Handles.DrawLine(patrolManager.points[i], nextPoint);
            }
            else
            {
                Handles.color = Color.black;
                Handles.DrawDottedLine(patrolManager.points[i], nextPoint, 4);
            }

            if (i == selectionInfo.pointIndex)
            {
                Handles.color = selectionInfo.pointsIsSelected ? Color.black : Color.red;
            }
            else
            {
                Handles.color = Color.green;
            }
            Handles.DrawSolidDisc(patrolManager.points[i], Vector3.up, patrolManager.handleRadius);

            Handles.color = Color.white;
            Handles.Label(patrolManager.points[i], "point : " + (i + 1));
        }

        if (shapeChangedSinceLastRepaint)
        {
            //shapeCreator.UpdateMeshDisplay();
        }

        shapeChangedSinceLastRepaint = false;
    }

    void OnEnable()
    {
        shapeChangedSinceLastRepaint = true;

        patrolManager = target as PatrolManager;
        selectionInfo = new SelectionInfo();

        Undo.undoRedoPerformed += OnUndoOrRedo;
        //Tools.hidden = true;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoOrRedo;
        //Tools.hidden = false;
    }

    void OnUndoOrRedo()
    {
        shapeChangedSinceLastRepaint = true;
    }

    public class SelectionInfo
    {
        public int pointIndex = -1;
        public bool mouseIsOverPoint = false;
        public bool pointsIsSelected = false;
        public Vector3 positionAtStartOfDrag;

        public int lineIndex = -1;
        public bool mouseIsOverLine = false;
    }
}
