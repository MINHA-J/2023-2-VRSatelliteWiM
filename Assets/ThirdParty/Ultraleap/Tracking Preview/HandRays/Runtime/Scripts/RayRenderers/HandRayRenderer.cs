/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2023.                                   *
 *                                                                            *
 * Use subject to the terms of the Apache License 2.0 available at            *
 * http://www.apache.org/licenses/LICENSE-2.0, or another agreement           *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using UnityEngine;

namespace Leap.Unity.Preview.HandRays
{
    /// <summary>
    /// Renders a line based on a handRayInteractor's data
    /// </summary>
    public class HandRayRenderer : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        
        // [TASk01] 대조군
        public GameObject currentContact = null;
        public Vector3 rayEndVector;

        /// <summary>
        /// If true, the ray renderer ignores the points given by the ray interactor.
        /// </summary>
        public bool ignoreRayInteractor { get { return _ignoreRayInteractor; } set { _ignoreRayInteractor = value; } }
        protected bool _ignoreRayInteractor = false;

        [Tooltip("The handRayInteractor the RayRenderer gets its points from")]
        public HandRayInteractor handRayInteractor;
        [Tooltip("If enabled, the ray is hidden when no points are provided by the hand ray interactor")]
        public bool hideRayOnNoPoints = false;

        protected bool _isRayEnabled = true;
        protected bool _isActive = true;
        protected bool _isValid = true;

        [SerializeField, Tooltip("The colour the ray turns when hovering over a valid point")]
        public Gradient validColorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) },
        };

        [SerializeField, Tooltip("The colour the ray turns when not hovering over a valid point")]
        public Gradient invalidColorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.red, 1f) },
            alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) },
        };

        /// <summary>
        /// Sets the ray renderer to be active or inactive
        /// When active, the ray renderer's visuals are enabled
        /// When inactive  the ray renderer's visuals are disabled
        /// </summary>
        /// <param name="isActive">Whether the ray renderer is active or not. Defaults to true</param>
        public virtual void SetActive(bool isActive = true)
        {
            _isActive = isActive;

            if (_ignoreRayInteractor)
            {
                lineRenderer.enabled = _isActive;
            }
            else
            {
                lineRenderer.enabled = _isActive && _isRayEnabled;
            }
        }

        // [TASk01] 대조군
        public Vector3 GetContactVector()
        {
            return rayEndVector;
        }

        public void SetContactVector(Vector3 vector)
        {
            int index = handRayInteractor.linePoints.Length - 1;
            handRayInteractor.linePoints[index] = vector;

            lineRenderer.SetPositions(handRayInteractor.linePoints);
        }

        public bool IsContact()
        {
            if (currentContact == null)
                return false;
            return true;
        }
        
        
        
        protected virtual void OnRaycastStateChange(HandRayDirection direction, bool enabled)
        {
            _isRayEnabled = enabled;

            if (_ignoreRayInteractor)
            {
                lineRenderer.enabled = _isActive;
            }
            else
            {
                lineRenderer.enabled = _isActive && _isRayEnabled;
            }
        }

        /// <summary>
        /// Sets whether the ray is over a valid point or not
        /// </summary>
        /// <param name="isValid"></param>
        public virtual void SetValid(bool isValid = true)
        {
            _isValid = isValid;
            lineRenderer.colorGradient = _isValid ? validColorGradient : invalidColorGradient;
        }

        protected virtual void OnEnable()
        {
            handRayInteractor.OnRaycastUpdate += OnRaycastUpdate;
            handRayInteractor.handRay.OnHandRayStateChange += OnRaycastStateChange;
        }

        protected virtual void OnDisable()
        {
            handRayInteractor.OnRaycastUpdate -= OnRaycastUpdate;
            handRayInteractor.handRay.OnHandRayStateChange -= OnRaycastStateChange;
        }

        protected virtual void OnRaycastUpdate(RaycastHit[] results, RaycastHit primaryHit)
        {
            if (_ignoreRayInteractor)
            {
                return;
            }

            if (!_isActive || !_isRayEnabled)
            {
                return;
            }

            if (hideRayOnNoPoints && (results == null || results.Length == 0))
            {
                lineRenderer.positionCount = 0;
                return;
            }

            lineRenderer.positionCount = handRayInteractor.numPoints;
            lineRenderer.SetPositions(handRayInteractor.linePoints);

            //[TASK01]: 대조군
            if (results.Length > 0)
            {
                //Debug.Log(results[0].transform.name);
                if (results[0].transform.CompareTag("OnGround") || results[0].transform.CompareTag("Target"))
                {
                    currentContact = results[0].transform.gameObject;
                    rayEndVector = results[0].point;
                }
                else
                {
                    currentContact = null;
                }
            }
            
        }
    }
}