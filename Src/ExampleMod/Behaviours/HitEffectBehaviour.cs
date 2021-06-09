using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AdvShields.Behaviours
{
    public class HitEffectBehaviour : MonoBehaviour
    {
        private float _progress;
        private Vector4 _worldHit;
        private float _duration;
        //private MaterialPropertyBlock _propertyBlock;
        //private Renderer _renderer;
        private Material _material;
        
        public void Initialize(Vector4 worldHit, Color hitColor, float magnitude, float duration)
        {
            Debug.Log("Effect initialized");
            _duration = duration;
            _progress = 0;
            _worldHit = Quaternion.Inverse(transform.rotation) * worldHit;

            _material = GetComponent<MeshRenderer>().material;
            _material.SetColor("_Color", hitColor);
            _material.SetVector("_WorldHit", transform.rotation * _worldHit);
            _material.SetFloat("_Magnitude", magnitude);
            _material.SetFloat("_Progress", _progress);

            //_renderer = GetComponent<MeshRenderer>();

            //_propertyBlock = new MaterialPropertyBlock();
            //_renderer.GetPropertyBlock(_propertyBlock);

            //_propertyBlock.SetColor("_Color", hitColor);
            //_propertyBlock.SetVector("_WorldHit", worldHit);
            //_propertyBlock.SetFloat("_Magnitude", magnitude);
            //_propertyBlock.SetFloat("_Progress", _progress);

            //_renderer.SetPropertyBlock(_propertyBlock);

            enabled = true;
            transform.gameObject.SetActive(true);
            transform.localScale = Vector3.one;

            //var check = this.isActiveAndEnabled;

            //Debug.Log($"Hit animation started at {worldHit}");
        }


        // Update is called once per frame
        void Update()
        {
            if (_material == null)
                return;

            if (_progress >= 1)
            {
                Debug.Log("Effect destroyed");
                Destroy(transform.gameObject);
            }

            //_renderer.GetPropertyBlock(_propertyBlock);

            _progress = Mathf.Clamp01(_progress + Time.deltaTime / _duration);
            _material.SetFloat("_Progress", _progress);
            _material.SetVector("_WorldHit", transform.rotation * _worldHit);
            //_propertyBlock.SetFloat("_Progress", _progress);
            //_renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
