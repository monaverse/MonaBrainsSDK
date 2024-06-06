using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Utils
{
    [RequireComponent(typeof(LineRenderer))]
    public class MonaBodyBoundingBoxRenderer : MonoBehaviour
    {
        public Color BoxColor = Color.white;
        public float LineWidth = 0.05f;
        public float Offset = 0.1f;
        public bool EncompassChildren = true;
        public Material LineMaterial;

        private LineRenderer _lineRenderer;
        private Transform _parent;

        private void Start()
        {
            _parent = transform.parent;
            _lineRenderer = GetComponent<LineRenderer>();

            if (_lineRenderer != null)
            {
                if (_lineRenderer.positionCount != 19)
                    _lineRenderer.positionCount = 19;

                _lineRenderer.loop = false;
                _lineRenderer.useWorldSpace = true;
            }

            if (LineMaterial == null)
                LineMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        private void Update()
        {
            if (_lineRenderer == null || _parent == null)
                return;

            if (_lineRenderer.enabled == false)
                _lineRenderer.enabled = true;

            _lineRenderer.startWidth = _lineRenderer.endWidth = LineWidth;
            _lineRenderer.startColor = _lineRenderer.endColor = BoxColor;

            if (_lineRenderer.material != LineMaterial)
                _lineRenderer.material = LineMaterial;

            DrawBoundingBox();
        }

        private void DrawBoundingBox()
        {
            Bounds bounds = GetBounds();
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;
            float offset = 1f;

            Vector3[] corners = new Vector3[8];
            corners[0] = (center + new Vector3(extents.x + Offset, extents.y + Offset, extents.z + Offset)) * offset;    // 0 top, right, front
            corners[1] = (center + new Vector3(-extents.x - Offset, extents.y + Offset, extents.z + Offset)) * offset;   // 1 top, left, front
            corners[2] = (center + new Vector3(-extents.x - Offset, extents.y + Offset, -extents.z - Offset)) * offset;  // 2 top, left, back
            corners[3] = (center + new Vector3(extents.x + Offset, extents.y + Offset, -extents.z - Offset)) * offset;   // 3 top, right, back
            corners[4] = (center + new Vector3(extents.x + Offset, -extents.y - Offset, extents.z + Offset)) * offset;   // 4 bottom, right, front
            corners[5] = (center + new Vector3(-extents.x - Offset, -extents.y - Offset, extents.z + Offset)) * offset;  // 5 bottom, left, front
            corners[6] = (center + new Vector3(-extents.x - Offset, -extents.y - Offset, -extents.z - Offset)) * offset; // 6 bottom, left, back
            corners[7] = (center + new Vector3(extents.x + Offset, -extents.y - Offset, -extents.z - Offset)) * offset;  // 7 bottom, right, back


            _lineRenderer.SetPositions(new Vector3[]
            {
                corners[0], corners[1], // Top face
                corners[1], corners[2],
                corners[2], corners[3],
                corners[3], corners[0],

                corners[4], corners[5], // Start of the bottom and edges
                corners[1],
                corners[5], corners[6],
                corners[2],
                corners[6], corners[7],
                corners[3],
                corners[7], corners[4]
            });
        }

        Bounds GetBounds()
        {
            Bounds bounds = new Bounds(_parent.position, Vector3.zero);

            if (EncompassChildren)
            {
                Renderer[] renderers = _parent.GetComponentsInChildren<Renderer>();

                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i].GetComponent<LineRenderer>() == null)
                        bounds.Encapsulate(renderers[i].bounds);
                }

                return bounds;
            }

            Renderer renderer = _parent.GetComponent<Renderer>();

            if (renderer != null)
                bounds.Encapsulate(renderer.bounds);

            return bounds;
        }

    }
}