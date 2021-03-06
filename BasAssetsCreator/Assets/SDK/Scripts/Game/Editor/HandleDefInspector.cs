﻿using System;
using UnityEngine;
using UnityEditor;

namespace BS
{
    [CustomEditor(typeof(HandleDefinition))]
    public class HandleDefInspector : Editor
    {
        Vector3 handlePoint1;
        Vector3 handlePoint2;
        Quaternion axis;
        bool toolsHidden = false;
        bool centerTransform = true;
        Tool previousTool;

        public override void OnInspectorGUI()
        {

            HandleDefinition handle = (HandleDefinition)target;
            ItemDefinition item = handle.transform.GetComponentInParent<ItemDefinition>();

            if (handle.axisLength > 0)
            {
                if (centerTransform)
                {
                    if (GUILayout.Button("Enable Point to Point transforms"))
                    {
                        centerTransform = false;
                        //handle.transform.hideFlags = HideFlags.NotEditable;
                    }
                }
                else
                {
                    if (GUILayout.Button("Enable Center Transform"))
                    {
                        centerTransform = true;
                        //handle.transform.hideFlags = 0;
                    }
                }
            }
            else
            {
                if (!centerTransform)
                {
                    centerTransform = true;
                }
            }

            base.OnInspectorGUI();

            if (GUILayout.Button("Calculate Reach") && item)
            {
                handle.CalculateReach();
            }

            if (handle.transform.localScale != Vector3.one)
            {
                EditorGUILayout.HelpBox("Handle object scale must be set to 1.", MessageType.Error);
            }

            if (handle.axisLength < 0)
            {
                EditorGUILayout.HelpBox("Handle axis length must be a positive number or zero.", MessageType.Error);
            }

            if (handle.touchRadius <= 0)
            {
                EditorGUILayout.HelpBox("Handle touch radius must be a positive number.", MessageType.Error);
            }

            if (handle.reach <= 0)
            {
                EditorGUILayout.HelpBox("Handle reach must be a positive number.", MessageType.Error);
            }

            if (handle.slideToHandleOffset <= 0)
            {
                EditorGUILayout.HelpBox("Slide to handle offset must be a positive number.", MessageType.Error);
            }

            foreach (HandleDefinition.Orientation orientations in handle.allowedOrientations)
            {
                for (int i = 0; i < handle.allowedOrientations.Count; i++)
                {
                    if (handle.allowedOrientations.IndexOf(orientations) < i && handle.allowedOrientations[i].rotation == handle.allowedOrientations[handle.allowedOrientations.IndexOf(orientations)].rotation && handle.allowedOrientations[i].allowedHand == handle.allowedOrientations[handle.allowedOrientations.IndexOf(orientations)].allowedHand && handle.allowedOrientations[i].isDefault == handle.allowedOrientations[handle.allowedOrientations.IndexOf(orientations)].isDefault)
                    {
                        EditorGUILayout.HelpBox("Allowed orientations " + handle.allowedOrientations.IndexOf(orientations) + " and " + i + " are equal.", MessageType.Warning);
                    }
                }
            }

            for (int i = 0; i < handle.allowedOrientations.Count; i++)
            {
                if (handle.allowedOrientations[i].allowedHand == HandleDefinition.HandSide.Left && (handle.allowedOrientations[i].isDefault == HandleDefinition.HandSide.Right || handle.allowedOrientations[i].isDefault == HandleDefinition.HandSide.Both))
                {
                    EditorGUILayout.HelpBox("Handle orientation " + i + " must have 'Is Default' set to None or Left if 'Allowed Hand' is set to Left.", MessageType.Warning);
                }
                if (handle.allowedOrientations[i].allowedHand == HandleDefinition.HandSide.Right && (handle.allowedOrientations[i].isDefault == HandleDefinition.HandSide.Left || handle.allowedOrientations[i].isDefault == HandleDefinition.HandSide.Both))
                {
                    EditorGUILayout.HelpBox("Handle orientation " + i + " must have 'Is Default' set to None or Right if 'Allowed Hand' is set to Right.", MessageType.Warning);
                }
            }

            if (!centerTransform)
            {
                handlePoint1 = handle.transform.position + (handle.transform.up * (handle.axisLength * 0.5f));
                handlePoint2 = handle.transform.position + (handle.transform.up * -(handle.axisLength * 0.5f));
            }

        }

        private void OnEnable()
        {
            HandleDefinition handle = (HandleDefinition)target;

            handlePoint1 = handle.transform.position + (handle.transform.up * (handle.axisLength * 0.5f));
            handlePoint2 = handle.transform.position + (handle.transform.up * -(handle.axisLength * 0.5f));

            //handle.transform.hideFlags = HideFlags.NotEditable;
        }
        
        private void OnSceneGUI()
        {

            HandleDefinition handle = (HandleDefinition)target;

            if (handle.axisLength > 0 && !centerTransform)
            {
                if (Tools.current != Tool.None)
                {
                    previousTool = Tools.current;
                    Tools.current = Tool.None;
                    toolsHidden = true;
                }
                
                EditorGUI.BeginChangeCheck();
                handlePoint1 = Handles.DoPositionHandle(handlePoint1, Quaternion.LookRotation(Vector3.forward, Vector3.up));
                handlePoint2 = Handles.DoPositionHandle(handlePoint2, Quaternion.LookRotation(Vector3.forward, Vector3.up));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Moved Handle");
                }
                Handles.color = Color.green;
                try
                {
                    if (EditorWindow.focusedWindow && EditorWindow.focusedWindow.ToString() == " (UnityEditor.SceneView)")
                    {
                        handle.transform.position = Vector3.Lerp(handlePoint1, handlePoint2, 0.5f);
                        handle.axisLength = (handlePoint1 - handlePoint2).magnitude;

                        if (Event.current.control)
                        {
                            axis = Handles.Disc(handle.transform.rotation, handle.transform.position, (handlePoint1 - handlePoint2), HandleUtility.GetHandleSize(handle.transform.position), false, 15f);
                        }
                        else
                        {
                            axis = Handles.Disc(handle.transform.rotation, handle.transform.position, (handlePoint1 - handlePoint2), HandleUtility.GetHandleSize(handle.transform.position), false, 0.1f);
                        }
                        handle.transform.rotation = Quaternion.LookRotation(handlePoint2 - handlePoint1, axis * Vector3.forward) * Quaternion.AngleAxis(-90, Vector3.right);
                    }
                    else
                    {
                        axis = Handles.Disc(handle.transform.rotation, handle.transform.position, (handlePoint1 - handlePoint2), HandleUtility.GetHandleSize(handle.transform.position), false, 0.1f);
                    }
                }
                catch (Exception) { }
            }
            else if (toolsHidden)
            {
                Tools.current = previousTool;
                toolsHidden = false;
            }
        }

        private void OnDisable()
        {
            EditorUtility.SetDirty(target);
            if (toolsHidden)
            {
                Tools.current = previousTool;
                toolsHidden = false;
            }
        }
    }
}