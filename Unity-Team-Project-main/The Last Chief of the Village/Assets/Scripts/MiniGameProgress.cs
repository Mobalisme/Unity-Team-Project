// MiniGameProgress.cs
// Field/Lake 미니게임 점수를 저장하고(0~100), 티어(1~3)로 변환해서 배틀에서 읽어오는 용도

using UnityEngine;

public static class MiniGameProgress
{
    public enum Tier123
    {
        Low_1 = 1,
        Mid_2 = 2,
        High_3 = 3
    }

    private const string KEY_FIELD_SCORE = "MINIGAME_FIELD_SCORE";
    private const string KEY_FIELD_TIER  = "MINIGAME_FIELD_TIER";
    private const string KEY_LAKE_SCORE  = "MINIGAME_LAKE_SCORE";
    private const string KEY_LAKE_TIER   = "MINIGAME_LAKE_TIER";

    // 배틀에서 디버그 로그용으로 바로 볼 수 있게 공개
    public static int FieldScore { get; private set; } = -1;
    public static int FieldTier  { get; private set; } = 2;

    public static int LakeScore  { get; private set; } = -1;
    public static int LakeTier   { get; private set; } = 2;

    // ===== 공통 유틸 =====
    public static int ClampScore100(int score) => Mathf.Clamp(score, 0, 100);

    public static Tier123 ScoreToTier(int score01_100)
    {
        int s = ClampScore100(score01_100);
        if (s <= 30) return Tier123.Low_1;
        if (s <= 65) return Tier123.Mid_2;
        return Tier123.High_3;
    }

    // ===== 저장/로드 =====
    public static void SetFieldResult(int score01_100)
    {
        int s = ClampScore100(score01_100);
        Tier123 t = ScoreToTier(s);

        FieldScore = s;
        FieldTier  = (int)t;

        PlayerPrefs.SetInt(KEY_FIELD_SCORE, FieldScore);
        PlayerPrefs.SetInt(KEY_FIELD_TIER,  FieldTier);
        PlayerPrefs.Save();
    }

    public static void SetLakeResult(int score01_100)
    {
        int s = ClampScore100(score01_100);
        Tier123 t = ScoreToTier(s);

        LakeScore = s;
        LakeTier  = (int)t;

        PlayerPrefs.SetInt(KEY_LAKE_SCORE, LakeScore);
        PlayerPrefs.SetInt(KEY_LAKE_TIER,  LakeTier);
        PlayerPrefs.Save();
    }

    public static void Load()
    {
        if (PlayerPrefs.HasKey(KEY_FIELD_SCORE)) FieldScore = PlayerPrefs.GetInt(KEY_FIELD_SCORE);
        if (PlayerPrefs.HasKey(KEY_FIELD_TIER))  FieldTier  = PlayerPrefs.GetInt(KEY_FIELD_TIER);

        if (PlayerPrefs.HasKey(KEY_LAKE_SCORE))  LakeScore = PlayerPrefs.GetInt(KEY_LAKE_SCORE);
        if (PlayerPrefs.HasKey(KEY_LAKE_TIER))   LakeTier  = PlayerPrefs.GetInt(KEY_LAKE_TIER);

        // 티어 범위 보정
        FieldTier = Mathf.Clamp(FieldTier, 1, 3);
        LakeTier  = Mathf.Clamp(LakeTier,  1, 3);
    }

    public static bool TryGetFieldTier(out int tier123)
    {
        if (PlayerPrefs.HasKey(KEY_FIELD_TIER))
        {
            tier123 = Mathf.Clamp(PlayerPrefs.GetInt(KEY_FIELD_TIER), 1, 3);
            return true;
        }

        tier123 = 2; // 기본 Mid
        return false;
    }

    public static bool TryGetLakeTier(out int tier123)
    {
        if (PlayerPrefs.HasKey(KEY_LAKE_TIER))
        {
            tier123 = Mathf.Clamp(PlayerPrefs.GetInt(KEY_LAKE_TIER), 1, 3);
            return true;
        }

        tier123 = 2; // 기본 Mid
        return false;
    }
}
