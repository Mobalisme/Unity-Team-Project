// GameManager.cs
// 주석: 한글 / 게임에 표시되는 문자열: 영어

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ===================== UI 연결 =====================
    [Header("UI")]
    public BattleUI battleUI;
    public TMP_Text playerHPText;
    public TMP_Text enemyHPText;

    [Header("Party Icons (Canvas: PlayerDino / PlayerDino2 / PlayerDino3)")]
    public Image playerDinoIcon1;
    public Image playerDinoIcon2;
    public Image playerDinoIcon3;

    [Header("Hit Shake (Optional)")]
    public BattleShaker playerShaker;
    public BattleShaker enemyShaker;

    // ===================== 티어 설정 =====================
    // 주의: PlayerDino1(Starter)는 고정 스탯이므로 티어 적용 안 함
    public enum Tier123 { Low_1 = 1, Mid_2 = 2, High_3 = 3 }

    [Header("Player Dino Tiers (Inspector)")]
    [Tooltip("PlayerDino1 = Starter (fixed stats). Tier is not applied.")]
    public Tier123 playerDino1Tier = Tier123.High_3;

    [Tooltip("PlayerDino2 = Attacker tier (Field mini game result)")]
    public Tier123 playerDino2Tier = Tier123.Mid_2;

    [Tooltip("PlayerDino3 = Tank tier (Lake mini game result)")]
    public Tier123 playerDino3Tier = Tier123.Mid_2;

    // ===================== 보스(고정 스펙) =====================
    [Header("Boss (Fixed Spec)")]
    public int bossMaxHP = 450;

    // 페이즈 기준(HP 비율): 1페 > 66.7%, 2페 > 33.3%, 그 외 3페
    private const float PHASE1_MIN = 2f / 3f;
    private const float PHASE2_MIN = 1f / 3f;

    // ===================== 데미지/회복 밸런스 =====================
    [Header("Damage Formula")]
    [Tooltip("damage = atk - def * defenseWeight (minDamage+)")]
    [Range(0f, 1.5f)] public float defenseWeight = 0.5f;
    public int minDamage = 1;

    [Header("Player Recover")]
    [Tooltip("Recover heals only dinos who selected Recover: maxHP * ratio")]
    [Range(0.05f, 0.25f)] public float playerRecoverRatio = 0.10f;
    [Tooltip("Recover heal capped by missingHP * capRatio (prevents full-heal abuse)")]
    [Range(0.2f, 1.0f)] public float playerRecoverCapMissingRatio = 0.60f;

    [Header("Boss AoE (Phase2+)")]
    [Tooltip("Phase2+: chance to hit all party members (scaled by aoeMultiplier)")]
    [Range(0f, 1f)] public float aoeChancePhase2 = 0.30f;
    [Range(0f, 1f)] public float aoeChancePhase3 = 0.45f;
    [Range(0.30f, 1.00f)] public float aoeMultiplier = 0.65f;
    public int aoeMinDamage = 1;

    // ===================== 내부 모델 =====================
    private class Dino
    {
        public string name;
        public int maxHP, atk, def, hp;
        public bool Dead => hp <= 0;

        public Dino(string n, int mhp, int a, int d)
        {
            name = n;
            maxHP = Mathf.Max(1, mhp);
            atk = Mathf.Max(0, a);
            def = Mathf.Max(0, d);
            hp = maxHP;
        }
    }

    private readonly List<Dino> party = new List<Dino>(3);
    private Dino boss;

    private int bossPhase = 1;

    private enum TurnState { Planning, Resolving, BossActing, Ended }
    private TurnState state = TurnState.Planning;

    private enum ActionType { Attack, Recover }
    private readonly ActionType[] plannedActions = new ActionType[3];
    private readonly bool[] actionLocked = new bool[3];
    private int planningIndex = 0;

    private Image[] partyIcons;
    private Vector3[] iconBaseScale;
    private int focusIndex = -1;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 두 미니게임(Field/Lake) 결과를 로드하고, 있으면 티어를 덮어씀
        MiniGameProgress.Load();

        if (MiniGameProgress.TryGetFieldTier(out int fieldTier))
            playerDino2Tier = (Tier123)fieldTier;   // Field -> Dino2(Attacker)

        if (MiniGameProgress.TryGetLakeTier(out int lakeTier))
            playerDino3Tier = (Tier123)lakeTier;    // Lake  -> Dino3(Tank)

        BuildParty();
        StartPlayerPlanning();
    }

    // ===================== 파티 구성(요구 스탯 그대로) =====================
    private void BuildParty()
    {
        party.Clear();

        // PlayerDino1: 스타터 고정
        party.Add(new Dino("Starter", 150, 24, 22));

        // PlayerDino2: 공격형(딜러) 티어 (Field 결과)
        party.Add(CreateAttacker((int)playerDino2Tier));

        // PlayerDino3: 방어형(탱커) 티어 (Lake 결과)
        party.Add(CreateTank((int)playerDino3Tier));

        // 보스
        boss = new Dino("Boss", bossMaxHP, 0, 0);

        bossPhase = 1;
        RefreshBossPhaseUpOnly();

        partyIcons = new Image[3] { playerDinoIcon1, playerDinoIcon2, playerDinoIcon3 };
        iconBaseScale = new Vector3[3];
        for (int i = 0; i < 3; i++)
        {
            if (partyIcons[i] != null)
                iconBaseScale[i] = partyIcons[i].transform.localScale;
        }

        UpdateHUD();
        UpdatePartyIcons();
    }

    // Field 스탯(요구치): High 155/32/18, Mid 145/28/16, Low 135/24/14
    private Dino CreateAttacker(int tier)
    {
        tier = Mathf.Clamp(tier, 1, 3);
        if (tier == 3) return new Dino("Attacker(3)", 155, 32, 18);
        if (tier == 2) return new Dino("Attacker(2)", 145, 28, 16);
        return new Dino("Attacker(1)", 135, 24, 14);
    }

    // Lake 스탯(요구치): High 210/22/32, Mid 195/20/28, Low 180/18/25
    private Dino CreateTank(int tier)
    {
        tier = Mathf.Clamp(tier, 1, 3);
        if (tier == 3) return new Dino("Tank(3)", 210, 22, 32);
        if (tier == 2) return new Dino("Tank(2)", 195, 20, 28);
        return new Dino("Tank(1)", 180, 18, 25);
    }

    // ===================== 턴 시작(플래닝) =====================
    private void StartPlayerPlanning()
    {
        if (boss.Dead) { EndBattle(true); return; }
        if (AllPartyDead()) { EndBattle(false); return; }

        state = TurnState.Planning;
        planningIndex = 0;
        focusIndex = -1;

        for (int i = 0; i < 3; i++)
        {
            plannedActions[i] = ActionType.Attack;
            actionLocked[i] = party[i].Dead;
        }

        if (battleUI != null)
        {
            battleUI.Show(true);
            battleUI.SetButtonsInteractable(true);
        }

        AdvanceToNextSelectable();
        UpdateHUD();
        UpdatePartyIcons();
    }

    private void AdvanceToNextSelectable()
    {
        while (planningIndex < 3 && actionLocked[planningIndex])
            planningIndex++;

        if (planningIndex >= 3)
        {
            StartCoroutine(ResolvePlayerTurnFlow());
            return;
        }

        focusIndex = planningIndex;
        UpdatePartyIcons();

        if (battleUI != null)
            battleUI.SetMessage(GetPlanningMessageEnglish());
    }

    private string GetPlanningMessageEnglish()
    {
        return $"{party[planningIndex].name}: choose action (Attack / Recover)\n" +
               $"Plan: 1[{plannedActions[0]}] 2[{plannedActions[1]}] 3[{plannedActions[2]}]";
    }

    // ===================== 버튼 입력 =====================
    public void PlayerAttack()
    {
        if (state != TurnState.Planning) return;
        if (planningIndex < 0 || planningIndex >= 3) return;

        plannedActions[planningIndex] = ActionType.Attack;
        planningIndex++;
        AdvanceToNextSelectable();
    }

    public void PlayerDefend()
    {
        if (state != TurnState.Planning) return;
        if (planningIndex < 0 || planningIndex >= 3) return;

        plannedActions[planningIndex] = ActionType.Recover;
        planningIndex++;
        AdvanceToNextSelectable();
    }

    // ===================== 플레이어 턴 처리 =====================
    private IEnumerator ResolvePlayerTurnFlow()
    {
        state = TurnState.Resolving;

        if (battleUI != null)
        {
            battleUI.SetButtonsInteractable(false);
            battleUI.SetMessage("Resolving turn...");
        }

        yield return new WaitForSeconds(0.25f);

        // Recover 먼저 처리
        for (int i = 0; i < 3; i++)
        {
            if (party[i].Dead) continue;
            if (plannedActions[i] != ActionType.Recover) continue;

            focusIndex = i;
            UpdatePartyIcons();

            int heal = ComputePlayerHeal(party[i]);
            int before = party[i].hp;
            party[i].hp = Mathf.Min(party[i].maxHP, party[i].hp + heal);
            int realHeal = party[i].hp - before;

            if (battleUI != null)
                battleUI.SetMessage($"{party[i].name} Recover (+{realHeal})");

            UpdateHUD();
            yield return new WaitForSeconds(0.45f);
        }

        // Attack 처리
        for (int i = 0; i < 3; i++)
        {
            if (party[i].Dead) continue;
            if (plannedActions[i] != ActionType.Attack) continue;

            focusIndex = i;
            UpdatePartyIcons();

            int dmg = ComputeDamage(party[i].atk, GetBossDefense(), minDamage);
            ApplyDamage(boss, dmg);

            if (enemyShaker != null) enemyShaker.Shake();

            RefreshBossPhaseUpOnly();

            if (battleUI != null)
                battleUI.SetMessage($"{party[i].name} attacks (-{dmg})");

            UpdateHUD();
            yield return new WaitForSeconds(0.45f);

            if (boss.Dead)
            {
                focusIndex = -1;
                UpdatePartyIcons();
                EndBattle(true);
                yield break;
            }
        }

        focusIndex = -1;
        UpdatePartyIcons();

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(BossTurnFlow());
    }

    private int ComputePlayerHeal(Dino d)
    {
        int heal = Mathf.CeilToInt(d.maxHP * playerRecoverRatio);

        int missing = d.maxHP - d.hp;
        int cap = Mathf.CeilToInt(missing * playerRecoverCapMissingRatio);

        heal = Mathf.Min(heal, Mathf.Max(1, cap));
        return Mathf.Max(1, heal);
    }

    // ===================== 보스 턴 처리 =====================
    private IEnumerator BossTurnFlow()
    {
        state = TurnState.BossActing;

        if (battleUI != null)
            battleUI.ShowEnemyTurn();

        yield return new WaitForSeconds(0.5f);

        if (boss.Dead) { EndBattle(true); yield break; }
        if (AllPartyDead()) { EndBattle(false); yield break; }

        bool doAoe = ShouldBossUseAoe();

        if (doAoe)
        {
            if (battleUI != null)
                battleUI.SetMessage("Boss uses AoE!");

            yield return new WaitForSeconds(0.2f);

            int bossAtk = GetBossAttack();

            for (int i = 0; i < 3; i++)
            {
                if (party[i].Dead) continue;

                focusIndex = i;
                UpdatePartyIcons();

                int baseDmg = ComputeDamage(bossAtk, party[i].def, aoeMinDamage);
                int dmg = Mathf.Max(aoeMinDamage, Mathf.RoundToInt(baseDmg * aoeMultiplier));

                ApplyDamage(party[i], dmg);

                if (playerShaker != null) playerShaker.Shake();

                UpdateHUD();
                yield return new WaitForSeconds(0.25f);
            }

            focusIndex = -1;
            UpdatePartyIcons();

            yield return new WaitForSeconds(0.4f);

            if (AllPartyDead()) { EndBattle(false); yield break; }
        }
        else
        {
            int target = FindRandomAliveIndex();
            if (target == -1) { EndBattle(false); yield break; }

            focusIndex = target;
            UpdatePartyIcons();

            int dmg = ComputeDamage(GetBossAttack(), party[target].def, minDamage);
            ApplyDamage(party[target], dmg);

            if (playerShaker != null) playerShaker.Shake();

            if (battleUI != null)
                battleUI.SetMessage($"Boss attacks {party[target].name} (-{dmg})");

            UpdateHUD();
            yield return new WaitForSeconds(0.9f);

            focusIndex = -1;
            UpdatePartyIcons();

            if (AllPartyDead()) { EndBattle(false); yield break; }
        }

        if (battleUI != null)
        {
            battleUI.Show(true);
            battleUI.SetButtonsInteractable(true);
        }

        StartPlayerPlanning();
    }

    private bool ShouldBossUseAoe()
    {
        if (bossPhase < 2) return false;

        float chance = (bossPhase == 2) ? aoeChancePhase2 : aoeChancePhase3;

        int alive = CountAlive();
        if (alive <= 1) chance *= 0.10f;

        return Random.value < chance;
    }

    // ===================== 보스 페이즈/스탯 =====================
    private void RefreshBossPhaseUpOnly()
    {
        int computed = ComputeBossPhaseByRatio();
        if (computed > bossPhase) bossPhase = computed;
    }

    private int ComputeBossPhaseByRatio()
    {
        float ratio = boss.hp / (float)boss.maxHP;
        if (ratio > PHASE1_MIN) return 1;
        if (ratio > PHASE2_MIN) return 2;
        return 3;
    }

    private int GetBossAttack()
    {
        if (bossPhase == 1) return 28;
        if (bossPhase == 2) return 31;
        return 34;
    }

    private int GetBossDefense()
    {
        if (bossPhase == 1) return 16;
        if (bossPhase == 2) return 18;
        return 20;
    }

    // ===================== 데미지 계산 =====================
    private int ComputeDamage(int atk, int def, int min)
    {
        float raw = atk - def * defenseWeight;
        raw = Mathf.Max(min, raw);
        return Mathf.Max(min, Mathf.RoundToInt(raw));
    }

    private void ApplyDamage(Dino target, int dmg)
    {
        target.hp -= dmg;
        if (target.hp < 0) target.hp = 0;
    }

    // ===================== 유틸 =====================
    private bool AllPartyDead()
    {
        for (int i = 0; i < 3; i++)
            if (!party[i].Dead) return false;
        return true;
    }

    private int CountAlive()
    {
        int c = 0;
        for (int i = 0; i < 3; i++)
            if (!party[i].Dead) c++;
        return c;
    }

    private int FindRandomAliveIndex()
    {
        List<int> alive = new List<int>(3);
        for (int i = 0; i < 3; i++)
            if (!party[i].Dead) alive.Add(i);

        if (alive.Count == 0) return -1;
        return alive[Random.Range(0, alive.Count)];
    }

    // ===================== HUD/아이콘 갱신 =====================
    private void UpdateHUD()
    {
        if (playerHPText != null)
        {
            playerHPText.text =
                $"{party[0].name}: {party[0].hp}/{party[0].maxHP}   " +
                $"{party[1].name}: {party[1].hp}/{party[1].maxHP}   " +
                $"{party[2].name}: {party[2].hp}/{party[2].maxHP}";
        }

        if (enemyHPText != null)
        {
            enemyHPText.text =
                $"Enemy HP: {boss.hp}/{boss.maxHP}\n" +
                $"Phase {bossPhase}  ATK {GetBossAttack()} / DEF {GetBossDefense()}";
        }
    }

    private void UpdatePartyIcons()
    {
        if (partyIcons == null || partyIcons.Length != 3) return;

        for (int i = 0; i < 3; i++)
        {
            Image img = partyIcons[i];
            if (img == null) continue;

            // 죽은 공룡은 반투명 처리
            Color c = img.color;
            c.a = party[i].Dead ? 0.25f : 1f;
            img.color = c;

            // 현재 선택/실행/피격 대상 강조
            if (iconBaseScale != null && iconBaseScale.Length == 3)
            {
                if (!party[i].Dead && i == focusIndex)
                    img.transform.localScale = iconBaseScale[i] * 1.15f;
                else
                    img.transform.localScale = iconBaseScale[i];
            }
        }
    }

    // ===================== 종료 =====================
    private void EndBattle(bool playerWon)
    {
        state = TurnState.Ended;
        focusIndex = -1;
        UpdatePartyIcons();

        if (battleUI != null)
            battleUI.ShowResult(playerWon);
    }
}
