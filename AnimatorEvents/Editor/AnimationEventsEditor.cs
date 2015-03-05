﻿using System.Collections.Generic;
using ATP.SimplePathAnimator.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace ATP.SimplePathAnimator.PathEvents {

    [CustomEditor(typeof (AnimatorEvents))]
    public class AnimationEventsEditor : Editor {
        #region FIELDS


        #endregion

        #region PROPERTIES

        public float DefaultNodeLabelHeight {
            get { return 30; }
        }

        public float DefaultNodeLabelWidth {
            get { return 100; }
        }

        public int MethodNameLabelOffsetX {
            get { return 30; }
        }

        public int MethodNameLabelOffsetY {
            get { return -20; }
        }

        private AnimatorEvents Script { get; set; }

        #endregion

        #region UNITY MESSAGES

        #region SERIALIZED PROPERTIES

        private SerializedProperty animator;

        private SerializedProperty drawMethodNames;

        private SerializedProperty nodeEvents;

        private SerializedProperty skin;
        private SerializedProperty eventsData;
        #endregion

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawEventsDataAssetField();

            EditorGUILayout.PropertyField(animator);
            EditorGUILayout.PropertyField(skin);

            ReorderableListGUI.Title("Events");
            ReorderableListGUI.ListField(nodeEvents);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable() {
            Script = (AnimatorEvents) target;

            nodeEvents = serializedObject.FindProperty("nodeEvents");
            animator = serializedObject.FindProperty("animator");
            drawMethodNames = serializedObject.FindProperty("drawMethodNames");
            skin = serializedObject.FindProperty("skin");
            eventsData = serializedObject.FindProperty("eventsData");
        }

        private void OnSceneGUI() {
            // TODO Guard against null Skin.
            HandleDrawingMethodNames();
        }

        #endregion

        #region METHODS

        private void DrawNodeLabel(
            Vector3 nodePosition,
            string value,
            int offsetX,
            int offsetY,
            GUIStyle style) {

            // Translate node's 3d position into screen coordinates.
            var guiPoint = HandleUtility.WorldToGUIPoint(nodePosition);

            // Create rectangle for the label.
            var labelPosition = new Rect(
                guiPoint.x + offsetX,
                guiPoint.y + offsetY,
                DefaultNodeLabelWidth,
                DefaultNodeLabelHeight);

            Handles.BeginGUI();

            // Draw label.
            GUI.Label(
                labelPosition,
                value,
                style);

            Handles.EndGUI();
        }

        private void DrawNodeLabels(
            IList<Vector3> nodePositions,
            IList<string> textValues,
            int offsetX,
            int offsetY,
            GUIStyle style) {

            for (var i = 0; i < textValues.Count; i++) {
                DrawNodeLabel(
                    nodePositions[i],
                    textValues[i],
                    offsetX,
                    offsetY,
                    style);
            }
        }

        private void HandleDrawingMethodNames() {
            if (!drawMethodNames.boolValue) return;

            var nodePositions = Script.GetNodePositions();
            var methodNames = Script.GetMethodNames();
            var style = Script.Skin.GetStyle("MethodNameLabel");

            DrawNodeLabels(
                nodePositions,
                methodNames,
                MethodNameLabelOffsetX,
                MethodNameLabelOffsetY,
                style);
        }

        protected virtual void DrawEventsDataAssetField() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                eventsData,
                new GUIContent(
                    "Events Data",
                    ""));

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }

}