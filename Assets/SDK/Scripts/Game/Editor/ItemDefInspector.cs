﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BS
{
    [CustomEditor(typeof(ItemDefinition))]
    public class ItemDefInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            ItemDefinition item = (ItemDefinition)target;

            base.OnInspectorGUI();

            if (item.itemId == "")
            {
                EditorGUILayout.HelpBox("Do not forget to assign a valid Item ID.", MessageType.Error);
            }

            foreach (MeshCollider col in item.gameObject.GetComponentsInChildren<MeshCollider>())
            {
                if (!col.convex)
                {
                    EditorGUILayout.HelpBox("The mesh collider " + col.name + " has to be set to convex.", MessageType.Error);
                }
            }

            if (!item.GetComponentInChildren<Collider>())
            {
                EditorGUILayout.HelpBox("There are no colliders assigned to this object. It will fall through the ground.", MessageType.Error);
            }

            GameObject[] collObj = new GameObject[item.GetComponentsInChildren<Collider>().Length];
            List<GameObject> hasRepeatedColliders = new List<GameObject>(item.GetComponentsInChildren<Collider>().Length);
            for (int i = 0; i < item.GetComponentsInChildren<Collider>().Length; i++)
            {
                bool isRepeated = false;
                foreach (GameObject ExistingCol in collObj)
                {
                    if (item.GetComponentsInChildren<Collider>()[i] && item.GetComponentsInChildren<Collider>()[i].gameObject == ExistingCol)
                    {
                        isRepeated = true;
                        hasRepeatedColliders.Add(item.GetComponentsInChildren<Collider>()[i].gameObject);
                    }
                }
                if (!isRepeated && collObj[i] == null)
                {
                    collObj[i] = item.GetComponentsInChildren<Collider>()[i].gameObject;

                }
            }
            foreach (GameObject col in collObj)
            {
                if (col)
                {
                    if (hasRepeatedColliders.Contains(col))
                    {
                        EditorGUILayout.HelpBox("Collider object " + col.name + " has more than one collider script.", MessageType.Error);
                    }
                }
            }

            foreach (Collider collider in item.GetComponentsInChildren<Collider>())
            {
                if (!collider.sharedMaterial && collider != item.customInertiaTensorCollider)
                {
                    EditorGUILayout.HelpBox("Collider " + collider.name + " is missing a physics material and will not make sounds nor decals on impact.", MessageType.Warning);
                }
                if (!collider.GetComponentInParent<ColliderGroup>() && collider != item.customInertiaTensorCollider)
                {
                    EditorGUILayout.HelpBox("Collider " + collider.name + " do not have any Collider Group on parent.", MessageType.Warning);
                }
            }

            if (item.holderPoint == null)
            {
                EditorGUILayout.HelpBox("There is no holder point assigned to this object. It will not be able to be holstered.", MessageType.Warning);
            }

            if (item.parryPoint == null)
            {
                EditorGUILayout.HelpBox("There is no parry point assigned to this object. Enemies will not know how to weild it.", MessageType.Warning);
            }

            if (item.transform.lossyScale != Vector3.one)
            {
                EditorGUILayout.HelpBox("The scale of the Item definition object should set to 1. Sercale the Mesh object instead.", MessageType.Warning);
            }

            if (!item.mainHandleLeft || !item.mainHandleRight)
            {
                EditorGUILayout.HelpBox("There are no handles on this object. It will not be able to be picked up from the spawn book.", MessageType.Warning);
            }

        }

        private void OnDisable()
        {
            if (target)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }

    [CustomEditor(typeof(Preview))]
    public class PreviewInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            Preview preview = (Preview)target;
            EditorGUILayout.HelpBox("The dark blue arrow points towards the viewer.", MessageType.Info);
        }
    }

}