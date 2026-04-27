using System.Collections.Generic;
using UnityEngine;

namespace GuJian.Rendering {
    /// <summary>
    /// 挂在玩家 GameObject 上。
    /// 每帧从摄像机射向玩家，命中的 Renderer 自动淡化为半透明，
    /// 不再遮挡时恢复原始材质。
    /// 适配：Unity 内置 Standard 标准材质
    /// </summary>
    public class OcclusionFader : MonoBehaviour {
        [Header("检测")]
        [Tooltip("遮挡检测用的层，只检测建筑/场景几何，排除角色自身")]
        [SerializeField] LayerMask occlusionLayers = ~0;
        [Tooltip("射线数量（1=单根，3=三角形采样更稳定）")]
        [SerializeField] int rayCount = 3;
        [Tooltip("多根射线时的采样半径")]
        [SerializeField] float sampleRadius = 0.3f;

        [Header("淡化")]
        [SerializeField, Range(0f, 1f)] float fadedAlpha = 0.25f;
        [SerializeField] float fadeSpeed = 8f;

        // 每个被追踪的 Renderer 存一条记录
        class RendererRecord {
            public Renderer renderer;
            public Material[] originalMats;   // 原始材质
            public Material[] fadeMats;       // 实例化的半透明版本
            public float currentAlpha = 1f;
            public bool targeted;             // 本帧是否仍被遮挡
        }

        readonly Dictionary<Renderer, RendererRecord> _tracked = new();
        Camera _cam;

        // Standard 材质 属性
        static readonly int PropColor = Shader.PropertyToID("_Color");
        static readonly int PropMode = Shader.PropertyToID("_Mode");
        static readonly int PropSrcBlend = Shader.PropertyToID("_SrcBlend");
        static readonly int PropDstBlend = Shader.PropertyToID("_DstBlend");
        static readonly int PropZWrite = Shader.PropertyToID("_ZWrite");

        void Start() {
            _cam = Camera.main;
        }

        void Update() {
            if (_cam == null) { _cam = Camera.main; return; }

            // 标记所有记录为"本帧未命中"
            foreach (var r in _tracked.Values) r.targeted = false;

            // 射线采样点
            Vector3 playerPos = transform.position + Vector3.up * 1f;
            Vector3 camPos = _cam.transform.position;

            var sampleOffsets = GetSampleOffsets();
            foreach (var offset in sampleOffsets) {
                Vector3 origin = camPos + offset;
                Vector3 dir = playerPos - origin;
                float dist = dir.magnitude;
                if (Physics.Raycast(origin, dir.normalized, out _, dist, occlusionLayers)) {
                    var hits = Physics.RaycastAll(origin, dir.normalized, dist, occlusionLayers);
                    foreach (var hit in hits) {
                        var rend = hit.collider.GetComponentInChildren<Renderer>()
                                   ?? hit.collider.GetComponent<Renderer>();
                        if (rend == null) continue;
                        if (!_tracked.TryGetValue(rend, out var rec)) {
                            rec = CreateRecord(rend);
                            if (rec == null) continue;
                            _tracked[rend] = rec;
                        }
                        rec.targeted = true;
                    }
                }
            }

            // 更新 alpha，清理不再遮挡的记录
            var toRemove = new List<Renderer>();
            foreach (var kv in _tracked) {
                var rec = kv.Value;
                float target = rec.targeted ? fadedAlpha : 1f;
                rec.currentAlpha = Mathf.MoveTowards(rec.currentAlpha, target, fadeSpeed * Time.deltaTime);
                ApplyAlpha(rec);
                if (!rec.targeted && Mathf.Approximately(rec.currentAlpha, 1f)) {
                    RestoreOriginal(rec);
                    toRemove.Add(kv.Key);
                }
            }
            foreach (var r in toRemove) _tracked.Remove(r);
        }

        Vector3[] GetSampleOffsets() {
            if (rayCount <= 1) return new[] { Vector3.zero };
            var offsets = new Vector3[rayCount];
            offsets[0] = Vector3.zero;
            for (int i = 1; i < rayCount; i++) {
                float angle = (360f / (rayCount - 1)) * (i - 1) * Mathf.Deg2Rad;
                offsets[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * sampleRadius;
            }
            return offsets;
        }

        RendererRecord CreateRecord(Renderer rend) {
            var orig = rend.sharedMaterials;
            var fades = new Material[orig.Length];
            for (int i = 0; i < orig.Length; i++) {
                if (orig[i] == null) { fades[i] = null; continue; }
                var m = new Material(orig[i]);
                MakeTransparentStandard(m);
                fades[i] = m;
            }
            var rec = new RendererRecord {
                renderer = rend,
                originalMats = orig,
                fadeMats = fades,
                currentAlpha = 1f,
            };
            return rec;
        }

        /// <summary>
        /// 专门给 Standard 材质用的透明设置
        /// </summary>
        static void MakeTransparentStandard(Material m) {
            m.SetFloat(PropMode, 3); // 3 = Transparent 模式
            m.SetFloat(PropSrcBlend, 5); // SrcAlpha
            m.SetFloat(PropDstBlend, 10); // OneMinusSrcAlpha
            m.SetFloat(PropZWrite, 0); // 关闭深度写入
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        void ApplyAlpha(RendererRecord rec) {
            if (!ReferenceEquals(rec.renderer.sharedMaterials, rec.fadeMats))
                rec.renderer.sharedMaterials = rec.fadeMats;

            for (int i = 0; i < rec.fadeMats.Length; i++) {
                if (rec.fadeMats[i] == null) continue;
                var c = rec.fadeMats[i].GetColor(PropColor);
                c.a = rec.currentAlpha;
                rec.fadeMats[i].SetColor(PropColor, c);
            }
        }

        void RestoreOriginal(RendererRecord rec) {
            rec.renderer.sharedMaterials = rec.originalMats;
            foreach (var m in rec.fadeMats)
                if (m != null) Destroy(m);
        }

        void OnDestroy() {
            foreach (var rec in _tracked.Values) RestoreOriginal(rec);
            _tracked.Clear();
        }
    }
}
