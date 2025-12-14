// MiniGameProgress.cs
// 주석: 한글 / 게임에 표시되는 문자열: 영어

using UnityEngine;

public static class MiniGameProgress
{
    // 1=Low, 2=Mid, 3=High
    public enum Tier123 { Low_1 = 1, Mid_2 = 2, High_3 = 3 }

    // PlayerPrefs Key
    private const string LAKE_SCORE_KEY  = "MINI_LAKE_SCORE";
    private const string LAKE_TIER_KEY   = "MINI_LAKE_TIER";
    private const string FIELD_SCORE_KEY = "MINI_FIELD_SCORE";
    private const string FIELD_TIER_KEY  = "MINI_FIELD_TIER";

    private static bool _loaded = false;

    public static int LakeScore  { get; private set; } = -1;
    public static int FieldScore { get; private set; } = -1;

    public static int LakeTier   { get; private set; } = (int)Tier123.Mid_2;
    public static int FieldTier  { get; private set; } = (int)Tier123.Mid_2;

    public static void Load()
    {
        LakeScore  = PlayerPrefs.GetInt(LAKE_SCORE_KEY, -1);
        FieldScore = PlayerPrefs.GetInt(FIELD_SCORE_KEY, -1);

        LakeTier   = PlayerPrefs.GetInt(LAKE_TIER_KEY, (int)Tier123.Mid_2);
        FieldTier  = PlayerPrefs.GetInt(FIELD_TIER_KEY, (int)Tier123.Mid_2);

        LakeTier  = Mathf.Clamp(LakeTier, 1, 3);
        FieldTier = Mathf.Clamp(FieldTier, 1, 3);

        if (LakeScore >= 0)  LakeScore  = ClampScore100(LakeScore);
        if (FieldScore >= 0) FieldScore = ClampScore100(FieldScore);

        _loaded = true;
    }

    private static void EnsureLoaded()
    {
        if (!_loaded) Load();
    }

    public static int ClampScore100(int s) => Mathf.Clamp(s, 0, 100);

    // 점수 -> 티어 enum (Field/Lake 팝업에서 주로 사용)
    public static Tier123 ScoreToTier(int rawScore)
    {
        int s = ClampScore100(rawScore);
        if (s <= 30) return Tier123.Low_1;
        if (s <= 65) return Tier123.Mid_2;
        return Tier123.High_3;
    }

    // 점수 -> 티어 int (GameManager에서 캐스팅할 때 사용)
    public static int TierFromScore(int rawScore) => (int)ScoreToTier(rawScore);

    // 저장 API
    public static void SetLakeResult(int rawScore)  => SaveLakeScore(rawScore);
    public static void SetFieldResult(int rawScore) => SaveFieldScore(rawScore);

    public static void SaveLakeScore(int rawScore)
    {
        EnsureLoaded();

        int s = ClampScore100(rawScore);
        int t = TierFromScore(s);

        LakeScore = s;
        LakeTier  = t;

        PlayerPrefs.SetInt(LAKE_SCORE_KEY, LakeScore);
        PlayerPrefs.SetInt(LAKE_TIER_KEY, LakeTier);
        PlayerPrefs.Save();
    }

    public static void SaveFieldScore(int rawScore)
    {
        EnsureLoaded();

        int s = ClampScore100(rawScore);
        int t = TierFromScore(s);

        FieldScore = s;
        FieldTier  = t;

        PlayerPrefs.SetInt(FIELD_SCORE_KEY, FieldScore);
        PlayerPrefs.SetInt(FIELD_TIER_KEY, FieldTier);
        PlayerPrefs.Save();
    }

    // 호환용(이전 코드에서 쓰던 이름이 남아도 컴파일되게)
    public static int ComputeTier(int score) => TierFromScore(score);
    public static void SaveScore(int score) => SaveLakeScore(score);

    // 배틀에서 안전하게 읽기
    public static bool TryGetLakeTier(out int tier)
    {
        EnsureLoaded();
        if (LakeScore < 0) { tier = (int)Tier123.Mid_2; return false; }
        tier = LakeTier;
        return true;
    }

    public static bool TryGetFieldTier(out int tier)
    {
        EnsureLoaded();
        if (FieldScore < 0) { tier = (int)Tier123.Mid_2; return false; }
        tier = FieldTier;
        return true;
    }

    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(LAKE_SCORE_KEY);
        PlayerPrefs.DeleteKey(LAKE_TIER_KEY);
        PlayerPrefs.DeleteKey(FIELD_SCORE_KEY);
        PlayerPrefs.DeleteKey(FIELD_TIER_KEY);
        PlayerPrefs.Save();

        _loaded = false;

        LakeScore = FieldScore = -1;
        LakeTier = FieldTier = (int)Tier123.Mid_2;
    }
}
