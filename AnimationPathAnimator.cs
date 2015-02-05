﻿using ATP.ReorderableList;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace ATP.AnimationPathTools {

    /// <summary>
    /// Component that allows animating transforms position along predefined
    /// Animation Paths and also animate their rotation on x and y axis in
    /// time.
    /// </summary>
    [RequireComponent(typeof(AnimationPath))]
    [RequireComponent(typeof(TargetAnimationPath))]
    [ExecuteInEditMode]
    public class AnimationPathAnimator : GameComponent {

        #region CONSTANTS

        /// <summary>
        /// Key shortcut to jump backward.
        /// </summary>
        public const KeyCode JumpBackward = KeyCode.LeftArrow;

        /// <summary>
        /// Key shortcut to jump forward.
        /// </summary>
        public const KeyCode JumpForward = KeyCode.RightArrow;

        /// <summary>
        /// Key shortcut to jump to the end of the animation.
        /// </summary>
        public const KeyCode JumpToEnd = KeyCode.DownArrow;

        /// <summary>
        /// Key shortcut to jump to the beginning of the animation.
        /// </summary>
        public const KeyCode JumpToStart = KeyCode.UpArrow;

        public const float JumpValue = 0.01f;

        /// <summary>
        /// Keycode used as a modifier key.
        /// </summary>
        /// <remarks>Modifier key changes how other keys works.</remarks>
        public const KeyCode ModKey = KeyCode.A;

        /// <summary>
        /// Value of the jump when modifier key is pressed.
        /// </summary>
        public const float ShortJumpValue = 0.002f;

        #endregion CONSTANTS

        #region EDITOR

        /// <summary>
        /// Transform to be animated.
        /// </summary>
        [SerializeField]
#pragma warning disable 649
        private Transform animatedObject;

        /// <summary>
        /// Path used to animate the <c>animatedObject</c> transform.
        /// </summary>
        [SerializeField]
        private AnimationPath animatedObjectPath;

        /// Current play time represented as a number between 0 and 1.
        [SerializeField]
        private float animTimeRatio;

        /// <summary>
        /// Animation duration in seconds.
        /// </summary>
        [SerializeField]
        private float duration = 20;

        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private AnimationCurve easeCurve = new AnimationCurve();

        /// <summary>
        /// Transform that the <c>animatedObject</c> will be looking at.
        /// </summary>
        [SerializeField]
#pragma warning disable 649
        private Transform followedObject;

        /// <summary>
        /// Path used to animate the <c>lookAtTarget</c>.
        /// </summary>
        [SerializeField]
        private TargetAnimationPath followedObjectPath;

        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local ReSharper
        // disable once ConvertToConstant.Local
        private float rotationSpeed = 3.0f;
#pragma warning restore 649
#pragma warning restore 649
        [SerializeField]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private AnimationCurve tiltingCurve = new AnimationCurve();

        [SerializeField]
        private AnimationCurve lookForwardCurve = new AnimationCurve();

        #endregion EDITOR

        #region FIELDS

        /// <summary>
        /// Current animation time in seconds.
        /// </summary>
        private float currentAnimTime;

        /// <summary>
        /// If animation is currently enabled.
        /// </summary>
        /// <remarks>
        /// Used in play mode. You can use it to stop animation.
        /// </remarks>
        private bool isPlaying;

        private const float LookForwardGizmoSize = 0.5f;

        /// <summary>
        /// How much look forward point should be positioned away from the
        /// animated object.
        /// </summary>
        /// <remarks>
        /// Value is a time in range from 0 to 1.
        /// </remarks>
        private const float LookForwardTimeDelta = 0.03f;

        #endregion FIELDS

        #region UNITY MESSAGES

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnValidate() {
            // Limit duration value.
            if (duration < 1) {
                duration = 1;
            }

            // Limit animation time ratio to <0; 1>.
            if (animTimeRatio < 0) {
                animTimeRatio = 0;
            }
            else if (animTimeRatio > 1) {
                animTimeRatio = 1;
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Awake() {
            // Initialize animatedObject field.
            if (animatedObject == null && Camera.main.transform != null) {
                animatedObject = Camera.main.transform;
            }
            // Initialize animatedObjectPath field.
            animatedObjectPath = GetComponent<AnimationPath>();
            // Initialize followedObjectPath field.
            followedObjectPath = GetComponent<TargetAnimationPath>();

            //CreateTargetGO();
        }

        public void CreateTargetGO() {
            string followedGOName = name + "-target";
            GameObject followedGO = GameObject.Find(followedGOName);
            // If nothing was found, create a new one.
            if (followedGO == null) {
                followedObject = new GameObject(followedGOName).transform;
                followedObject.parent = gameObject.transform;
            }
            else {
                followedObject = followedGO.transform;
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Start() {
            InitializeEaseCurve();
            InitializeRotationCurve();
            InitializeLookForwardCurve();

            // Start playing animation on Start().
            isPlaying = true;

            // Start animation from time ratio specified in the inspector.
            currentAnimTime = animTimeRatio * duration;

            if (Application.isPlaying) {
                StartCoroutine(EaseTime());
            }
        }

        private void InitializeLookForwardCurve() {
            var firstKey = new Keyframe(0, LookForwardTimeDelta, 0, 0);
            var lastKey = new Keyframe(1, LookForwardTimeDelta, 0, 0);

            lookForwardCurve.AddKey(firstKey);
            lookForwardCurve.AddKey(lastKey);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void Update() {
            // In play mode, update animation time with delta time.
            if (Application.isPlaying && isPlaying) {
                Animate();
            }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private void OnDrawGizmosSelected() {
            if (followedObject != null) return;

            Vector3 forwardPoint = GetForwardPoint();
            Vector3 size = new Vector3(
                LookForwardGizmoSize,
                LookForwardGizmoSize,
                LookForwardGizmoSize);
            Gizmos.DrawWireCube(forwardPoint, size);
        }

        #endregion UNITY MESSAGES

        #region PUBLIC METHODS

        public float[] GetTargetPathTimestamps() {
            return followedObjectPath.GetNodeTimestamps();
        }

        /// <summary>
        /// Call in edit mode to update animation.
        /// </summary>
        public void UpdateAnimation() {
            if (!Application.isPlaying) {
                Animate();
            }
        }
        #endregion PUBLIC METHODS

        #region PRIVATE METHODS

        private void Animate() {
            // Animate target.
            AnimateTarget();

            // Animate transform.
            AnimateObject();

            // Rotate transform.
            RotateObject();

            TiltObject();
        }

        private void AnimateObject() {
            if (animatedObject == null
                || animatedObjectPath == null
                || !animatedObjectPath.IsInitialized) {

                return;
            }

            // Update position.
            animatedObject.position =
                animatedObjectPath.GetVectorAtTime(animTimeRatio);
        }

        private void AnimateTarget() {
            if (followedObject == null
               || followedObjectPath == null
               || !followedObjectPath.IsInitialized) {

                return;
            }

            // Update position.
            followedObject.position =
                followedObjectPath.GetVectorAtTime(animTimeRatio);
        }

        // TODO Add possibility to stop when isPlaying is disabled.
        private IEnumerator EaseTime() {
            do {
                // Increase animation time.
                currentAnimTime += Time.deltaTime;

                // Convert animation time to <0; 1> ratio.
                var timeRatio = currentAnimTime / duration;

                animTimeRatio = easeCurve.Evaluate(timeRatio);

                yield return null;
            } while (animTimeRatio < 1.0f);
        }

        private void InitializeEaseCurve() {
            var firstKey = new Keyframe(0, 0, 0, 0);
            var lastKey = new Keyframe(1, 1, 0, 0);

            easeCurve.AddKey(firstKey);
            easeCurve.AddKey(lastKey);
        }

        private void InitializeRotationCurve() {
            var firstKey = new Keyframe(0, 0, 0, 0);
            var lastKey = new Keyframe(1, 0, 0, 0);

            tiltingCurve.AddKey(firstKey);
            tiltingCurve.AddKey(lastKey);
        }
        // TODO Rename to HandleObjectRotation().
        private void RotateObject() {
            // TODO Move this condition to Animate().
            if (!animatedObjectPath.IsInitialized) return;

            // Look at target.
            if (animatedObject != null && followedObject != null) {
                RotateObjectToPosition(followedObject.position);
            }
            // Look forward.
            else if (animatedObject != null) {
                Vector3 forwardPoint = GetForwardPoint();
                RotateObjectToPosition(forwardPoint);
            }
        }

        private Vector3 GetForwardPoint() {
            // Timestamp offset of the forward point.
            var forwardPointDelta = lookForwardCurve.Evaluate(animTimeRatio);
            // Forward point timestamp.
            var forwardPointTimestamp = animTimeRatio + forwardPointDelta;

            return animatedObjectPath.GetVectorAtTime(forwardPointTimestamp);
        }

        private void RotateObjectToPosition(Vector3 targetPosition) {
// Calculate direction to target.
            var targetDirection = targetPosition - animatedObject.position;
            // Calculate rotation to target.
            var rotation = Quaternion.LookRotation(targetDirection);
            // Calculate rotation speed.
            var speed = Time.deltaTime * rotationSpeed;

            // Lerp rotation.
            animatedObject.rotation = Quaternion.Slerp(
                animatedObject.rotation,
                rotation,
                speed);
        }

        private void TiltObject() {
            if (animatedObject == null
                || followedObject == null
                || !animatedObjectPath.IsInitialized) {

                return;
            }

            var eulerAngles = transform.rotation.eulerAngles;
            // Get rotation from AnimationCurve.
            var zRotation = tiltingCurve.Evaluate(animTimeRatio);
            eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, zRotation);
            transform.rotation = Quaternion.Euler(eulerAngles);

        }
        #endregion PRIVATE METHODS
    }
}