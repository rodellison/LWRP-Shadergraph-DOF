using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Kino.PostProcessing.Utilities
{
    [ExecuteInEditMode]
    public sealed class FocusPuller : MonoBehaviour
    {
        [SerializeField] Camera _camera = null;
        [SerializeField] float _offset = 0;
        [SerializeField] private float maxDistance = 100;
        public LayerMask hitLayer;

        [Header("Camera DOF Settings")] [SerializeField]
        private float defaultFocalDistance = 8f;

        [SerializeField] [Range(1.4f, 64f)] private float defaultAperture = 8;
        [SerializeField] [Range(1.4f, 64f)] private float defaultFocalLengthMM = 35;

        public bool interpolateFocus = false;
        public float interpolationTime = 0.5f;

        private Collider lastColliderHit;
        private Collider nullCollider;

        private bool bDofChangeInProgress = false;
        public string GroundTagName;

        DepthOfField _dof;
        PostProcessProfile _profile;
        PostProcessVolume _volume;


        void Start()
        {
            _dof = ScriptableObject.CreateInstance<DepthOfField>();
            _dof.hideFlags = HideFlags.DontSave;
            _dof.focusDistance.overrideState = true;
            _dof.aperture.overrideState = true;
            _dof.focalLength.overrideState = true;

            _dof.enabled.value = true;

            _profile = ScriptableObject.CreateInstance<PostProcessProfile>();
            _profile.hideFlags = HideFlags.DontSave;
            _profile.AddSettings(_dof);

            _volume = gameObject.AddComponent<PostProcessVolume>();
            _volume.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            _volume.sharedProfile = _profile;
            _volume.isGlobal = true;
            _volume.priority = 1000;

            _dof.focusDistance.value = defaultFocalDistance + _offset;
            _dof.aperture.value = defaultAperture;
            _dof.focalLength.value = defaultFocalLengthMM;
            
            nullCollider = new Collider();
            lastColliderHit = nullCollider;

            LateUpdate();
        }

        void OnDestroy()
        {
            DestroyAsset(_dof);
            DestroyAsset(_profile);
        }

        void LateUpdate()
        {
            if (_camera == null) return;
            Focus();
        }

        IEnumerator InterpolateFocusToDefault()
        {
            bDofChangeInProgress = true;
            if (!interpolateFocus)
            {
                _dof.focusDistance.value = defaultFocalDistance;
        //        _dof.aperture.value = defaultAperture;
        //        _dof.focalLength.value = defaultFocalLengthMM;
            }
            else
            {
                float currDofDistance = _dof.focusDistance.value;
       //         float currDofAperture = _dof.aperture.value;
       //         float currDofFocalLength = _dof.focalLength.value;

                float dTime = 0;
                while (dTime < 1)
                {
                    yield return null;
                    dTime += Time.deltaTime / this.interpolationTime;
                    _dof.focusDistance.value = Mathf.Lerp(currDofDistance, defaultFocalDistance, dTime);
        //            _dof.aperture.value = Mathf.Lerp(currDofAperture, defaultAperture, dTime);
       //             _dof.focalLength.value = Mathf.Lerp(currDofFocalLength, defaultFocalLengthMM, dTime);
                }
            }

            bDofChangeInProgress = false;
        }

        IEnumerator InterpolateFocus(Vector3 theHitPoint)
        {
            bDofChangeInProgress = true;

            float endDofDistance = (_camera.transform.position - theHitPoint).magnitude + _offset;
            if (endDofDistance <= 1f)
                endDofDistance = 1f;
       //     float endDofAperture = maxDistance / endDofDistance * 0.25f;
            
       //     Debug.Log(endDofDistance);
            
            float endDofFocalLength = _dof.focalLength.value;

            if (!interpolateFocus)
            {
                _dof.focusDistance.value = endDofDistance;
   //             _dof.aperture.value = endDofAperture;
   //             _dof.focalLength.value = endDofFocalLength;
            }
            else
            {
                float currDofDistance = _dof.focusDistance.value;
                float currDofAperture = _dof.aperture.value;
                float currDofFocalLength = _dof.focalLength.value;

                float dTime = 0;

                while (dTime < 1)
                {
                    yield return null;
                    dTime += Time.deltaTime / this.interpolationTime;
                    _dof.focusDistance.value = Mathf.Lerp(currDofDistance, endDofDistance, dTime);
             //       _dof.aperture.value = Mathf.Lerp(currDofAperture, endDofAperture, dTime);
             //       _dof.focalLength.value = Mathf.Lerp(currDofFocalLength, endDofFocalLength, dTime);
                }
            }

            bDofChangeInProgress = false;
        }

        void Focus()
        {
            // our ray
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            //	Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, this.maxDistance, this.hitLayer))
            {
                if (hit.collider.tag.Equals(GroundTagName))
                {
                    _dof.focusDistance.value = (_camera.transform.position - hit.point).magnitude + _offset;
                    lastColliderHit = hit.collider;
                }
                else
                {
                    if (hit.collider.Equals(lastColliderHit) || bDofChangeInProgress)
                        return;

                    lastColliderHit = hit.collider;
                    Debug.Log("Changing Focal Distance " + hit.collider.name);
                    StopCoroutine("InterpolateFocus");
                    StartCoroutine(InterpolateFocus(hit.point));
                }
            }
            else
            {
                if (bDofChangeInProgress)
                    return;

                if (!lastColliderHit.Equals(nullCollider))
                {
                    lastColliderHit = nullCollider;

                    Debug.Log("Changing Focal Distance to default");
                    StopCoroutine("InterpolateFocusToDefault");
                    StartCoroutine(InterpolateFocusToDefault());
                }
            }
        }


        static void DestroyAsset(Object o)
        {
            if (o == null) return;
            if (Application.isPlaying)
                Object.Destroy(o);
            else
                Object.DestroyImmediate(o);
        }
    }
}