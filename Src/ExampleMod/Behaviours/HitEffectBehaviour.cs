using UnityEngine;

namespace AdvShields.Behaviours
{
    public class HitEffectBehaviour : MonoBehaviour
    {
        private float _duration;

        private float _magnitude;

        private Color _hitColor;

        private float _progress;

        private Vector4 _worldHit;

        //private MaterialPropertyBlock _propertyBlock;
        //private Renderer _renderer;
        //private Material _material;

        public void Initialize(Vector4 worldHit, Color hitColor, float magnitude, float duration)
        {
            Debug.Log("Effect initialized");

            _duration = duration;
            _magnitude = magnitude;
            _hitColor = hitColor;
            _progress = 0;
            _worldHit = Quaternion.Inverse(transform.rotation) * worldHit;

            //_renderer = GetComponent<MeshRenderer>();

            //_propertyBlock = new MaterialPropertyBlock();
            //_renderer.GetPropertyBlock(_propertyBlock);

            //_propertyBlock.SetColor("_Color", hitColor);
            //_propertyBlock.SetVector("_WorldHit", worldHit);
            //_propertyBlock.SetFloat("_Magnitude", magnitude);
            //_propertyBlock.SetFloat("_Progress", _progress);

            //_renderer.SetPropertyBlock(_propertyBlock);

            enabled = true;
            gameObject.SetActive(true);
            transform.localScale = Vector3.one;

            //var check = this.isActiveAndEnabled;

            //Debug.Log($"Hit animation started at {worldHit}");
        }

        private void Update()
        {
            Material _material = GetComponent<MeshRenderer>().material;
            if (_material == null) return;

            if (_progress >= 1)
            {
                Debug.Log("Effect destroyed");

                enabled = false;
                gameObject.SetActive(false);
                Destroy(this);
                Destroy(gameObject);

                return;
            }

            _progress = Mathf.Clamp01(_progress + Time.deltaTime / _duration);

            _material = GetComponent<MeshRenderer>().material;
            _material.SetFloat("_Magnitude", _magnitude);
            _material.SetColor("_Color", _hitColor);
            _material.SetFloat("_Progress", _progress);
            _material.SetVector("_WorldHit", transform.rotation * _worldHit);

            //_renderer.GetPropertyBlock(_propertyBlock);
            //_propertyBlock.SetFloat("_Progress", _progress);
            //_renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
