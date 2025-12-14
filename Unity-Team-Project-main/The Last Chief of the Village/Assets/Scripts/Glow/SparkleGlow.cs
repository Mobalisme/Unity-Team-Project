using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class OverGlowFloat : MonoBehaviour
{
    [Header("Glow Look (no scale)")]
    public Color glowColor = new Color(1f, 1f, 0.75f, 1f);
    [Range(4, 24)] public int glowCopies = 16;   // 겹 수 (많을수록 형체 더 날아감)
    [Range(0f, 1f)] public float glowAlpha = 0.9f;

    [Header("Glow Spread (no scale)")]
    [Range(0f, 0.2f)] public float spread = 0.06f; // 퍼짐 정도 (월드 단위)
    public bool hideBaseSprite = true;             // 본체 거의 숨김

    [Header("Sparkle (alpha pulse only)")]
    public float pulseSpeed = 10f;
    [Range(0f, 1f)] public float pulseAmount = 0.9f;

    [Header("Bob (up-down floating)")]
    public float bobSpeed = 2.2f;     // 둥둥 속도
    public float bobAmount = 0.07f;   // 둥둥 높이 (월드 단위)

    SpriteRenderer baseSr;
    SpriteRenderer[] glowSrs;
    Transform glowRoot;

    Vector3 startLocalPos;

    void Awake()
    {
        baseSr = GetComponent<SpriteRenderer>();
        startLocalPos = transform.localPosition;
        BuildGlow();
    }

    void OnEnable()
    {
        if (baseSr == null) baseSr = GetComponent<SpriteRenderer>();
        if (glowSrs == null || glowSrs.Length == 0) BuildGlow();
    }

    void BuildGlow()
    {
        var old = transform.Find("__GLOW_ROOT__");
        if (old != null) Destroy(old.gameObject);

        glowRoot = new GameObject("__GLOW_ROOT__").transform;
        glowRoot.SetParent(transform, false);

        glowSrs = new SpriteRenderer[glowCopies];

        for (int i = 0; i < glowCopies; i++)
        {
            var g = new GameObject($"Glow_{i}");
            g.transform.SetParent(glowRoot, false);
            g.transform.localScale = Vector3.one; // ⭐ 크기 절대 변경 안 함

            var sr = g.AddComponent<SpriteRenderer>();
            sr.sprite = baseSr.sprite;
            sr.sortingLayerID = baseSr.sortingLayerID;
            sr.sortingOrder = baseSr.sortingOrder - (glowCopies - i);
            sr.color = new Color(glowColor.r, glowColor.g, glowColor.b, glowAlpha);

            glowSrs[i] = sr;
        }
    }

    void LateUpdate()
    {
        // 애니메이션 스프라이트 변경 따라가기
        for (int i = 0; i < glowCopies; i++)
        {
            glowSrs[i].sprite = baseSr.sprite;
            glowSrs[i].flipX = baseSr.flipX;
            glowSrs[i].flipY = baseSr.flipY;
            glowSrs[i].sortingLayerID = baseSr.sortingLayerID;
        }

        // 반짝 (알파만)
        float t = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
        float pulse = Mathf.Lerp(1f - pulseAmount, 1f + pulseAmount, t);

        // 퍼짐 (scale 없이 위치만 살짝 흩뿌림)
        for (int i = 0; i < glowCopies; i++)
        {
            float ratio = (i + 1f) / glowCopies;
            float ang = ratio * Mathf.PI * 2f;
            float r = spread * Mathf.Lerp(0.25f, 1f, ratio);

            glowSrs[i].transform.localPosition =
                new Vector3(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r, 0f);

            float a = Mathf.Clamp01(glowAlpha * pulse * Mathf.Lerp(0.25f, 1f, ratio));
            var c = glowColor; c.a = a;
            glowSrs[i].color = c;
        }

        // 본체 처리 (형체 날리기)
        if (hideBaseSprite)
            baseSr.color = new Color(1f, 1f, 1f, 0.03f);
        else
            baseSr.color = Color.white;

        // 둥둥 떠다니기 (위아래)
        float y = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.localPosition = startLocalPos + new Vector3(0f, y, 0f);
    }
}
