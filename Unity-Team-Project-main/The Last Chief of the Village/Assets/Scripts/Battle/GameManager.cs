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
    public TMP_Text playerHPText;   // PlayerHPText
    public TMP_Text enemyHPText;    // EnemyHPText

    [Header("Party Icons (Canvas: PlayerDino / PlayerDino2 / PlayerDino3)")]
    public Image playerDinoIcon1;   // PlayerDino
    public Image playerDinoIcon2;   // PlayerDino2
    public Image playerDinoIcon3;   // PlayerDino3

    [Header("Hit Shake (Optional)")]
    public BattleShaker playerShaker;
    public BattleShaker enemyShaker;

    // ===================== 티어(하=1, 중=2, 상=3) =====================
    // 주의: PlayerDino1(Starter)는 고정 스탯이므로 티어 적용 안 함
    public enum Tier123 { Low_1 = 1, Mid_2 = 2, High_3 = 3 }

    [Header("Player Dino Tiers (Inspector)")]
    [Tooltip("PlayerDino1 = Starter (fixed stats). Tier is not applied.")]
    public Tier123 playerDino1Tier = Tier123.High_3;

    [Tooltip("PlayerDino2 = Attacker tier")]
    public Tier123 playerDino2Tier = Tier123.Mid_2;

    [Tooltip("PlayerDino3 = Tank tier")]
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

    [Tooltip("Recover heal is capped by missingHP * capRatio to prevent balance break")]
    [Range(0.2f, 1.0f)] public float playerRecoverCapMissingRatio = 0.60f;

    [Header("Boss AoE (Phase2+)")]
    [Tooltip("AoE chance in Phase 2")]
    [Range(0f, 1f)] public float aoeChancePhase2 = 0.30f;

    [Tooltip("AoE chance in Phase 3")]
    [Range(0f, 1f)] public float aoeChancePhase3 = 0.45f;

    [Tooltip("AoE damage multiplier vs single-hit damage (recommended 0.55~0.75)")]
    [Range(0.30f, 1.00f)] public float aoeMultiplier = 0.65f;

    public int aoeMinDamage = 1;

    // ===================== 런타임 데이터 =====================
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

    // 보스 페이즈는 “올라가기만”(회복/특수 상황에도 페이즈가 내려가지 않게)
    private int bossPhase = 1;

    // ===================== 플레이어 턴: 3마리 각각 행동 선택 후 일괄 실행 =====================
    private enum TurnState { Planning, Resolving, BossActing, Ended }
    private TurnState state = TurnState.Planning;

    private enum ActionType { Attack, Recover }
    private readonly ActionType[] plannedActions = new ActionType[3];
    private readonly bool[] actionLocked = new bool[3]; // 죽은 공룡은 스킵용
    private int planningIndex = 0;

    // ===================== 아이콘 강조(선택/실행/피격 표시) =====================
    private Image[] partyIcons;
    private Vector3[] iconBaseScale;
    private int focusIndex = -1;

    // ===================== Unity =====================
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BuildParty();
        StartPlayerPlanning();
    }

    // ===================== 파티 구성(요구 스탯 그대로) =====================
    private void BuildParty()
    {
        party.Clear();

        // PlayerDino1: 스타터 고정
        party.Add(new Dino("Starter", 150, 24, 22));

        // PlayerDino2: 공격형(딜러) 티어
        party.Add(CreateAttacker((int)playerDino2Tier));

        // PlayerDino3: 방어형(탱커) 티어
        party.Add(CreateTank((int)playerDino3Tier));

        // 보스
        boss = new Dino("Boss", bossMaxHP, 0, 0);

        bossPhase = 1;
        RefreshBossPhaseUpOnly();

        // 아이콘 배열
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

    private Dino CreateAttacker(int tier) // 1=하,2=중,3=상
    {
        tier = Mathf.Clamp(tier, 1, 3);
        if (tier == 3) return new Dino("Attacker(3)", 155, 32, 18);
        if (tier == 2) return new Dino("Attacker(2)", 145, 28, 16);
        return new Dino("Attacker(1)", 135, 24, 14);
    }

    private Dino CreateTank(int tier) // 1=하,2=중,3=상
    {
        tier = Mathf.Clamp(tier, 1, 3);
        if (tier == 3) return new Dino("Tank(3)", 210, 22, 32);
        if (tier == 2) return new Dino("Tank(2)", 195, 20, 28);
        return new Dino("Tank(1)", 180, 18, 25);
    }

    // ===================== 플레이어 턴 시작: 1→2→3 행동 선택 =====================
    private void StartPlayerPlanning()
    {
        if (boss.Dead) { EndBattle(true); return; }
        if (AllPartyDead()) { EndBattle(false); return; }

        state = TurnState.Planning;
        planningIndex = 0;
        focusIndex = -1;

        // 기본값: 전부 Attack
        for (int i = 0; i < 3; i++)
        {
            plannedActions[i] = ActionType.Attack;
            actionLocked[i] = party[i].Dead; // 죽은 공룡은 선택 불가(자동 스킵)
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

    // 다음 선택 가능한 공룡으로 이동(죽은 공룡 스킵)
    private void AdvanceToNextSelectable()
    {
        while (planningIndex < 3 && actionLocked[planningIndex])
            planningIndex++;

        if (planningIndex >= 3)
        {
            // 3마리 모두 선택 완료 → 턴 실행
            StartCoroutine(ResolvePlayerTurnFlow());
            return;
        }

        focusIndex = planningIndex;
        UpdatePartyIcons();

        if (battleUI != null)
            battleUI.SetMessage(GetPlanningMessageEnglish());
    }

    // 게임에 표시되는 문구(영어)
    private string GetPlanningMessageEnglish()
    {
        return $"{party[planningIndex].name}: choose action (Attack / Recover)\n" +
               $"Plan: 1[{plannedActions[0]}] 2[{plannedActions[1]}] 3[{plannedActions[2]}]";
    }

    // ===================== 버튼 입력(선택 단계에서만 동작) =====================
    public void PlayerAttack()
    {
        if (state != TurnState.Planning) return;
        if (planningIndex < 0 || planningIndex >= 3) return;

        plannedActions[planningIndex] = ActionType.Attack;
        planningIndex++;
        AdvanceToNextSelectable();
    }

    public void PlayerDefend() // Recover
    {
        if (state != TurnState.Planning) return;
        if (planningIndex < 0 || planningIndex >= 3) return;

        plannedActions[planningIndex] = ActionType.Recover;
        planningIndex++;
        AdvanceToNextSelectable();
    }

    // ===================== 플레이어 턴 실행(선택 후 일괄 실행) =====================
    private IEnumerator ResolvePlayerTurnFlow()
    {
        state = TurnState.Resolving;

        if (battleUI != null)
        {
            battleUI.SetButtonsInteractable(false);
            battleUI.SetMessage("Resolving turn...");
        }

        yield return new WaitForSeconds(0.25f);

        // 1) Recover 먼저 처리(회복 선택한 공룡만)
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

        // 2) Attack 처리(공격 선택한 공룡만)
        for (int i = 0; i < 3; i++)
        {
            if (party[i].Dead) continue;
            if (plannedActions[i] != ActionType.Attack) continue;

            focusIndex = i;
            UpdatePartyIcons();

            int dmg = ComputeDamage(party[i].atk, GetBossDefense(), minDamage);
            ApplyDamage(boss, dmg);

            if (enemyShaker != null) enemyShaker.Shake();

            // HP가 내려가면 페이즈가 올라갈 수 있음
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

        // 3) 보스 행동
        StartCoroutine(BossTurnFlow());
    }

    // 플레이어 회복량 계산(밸런스가 깨지지 않도록 제한 포함)
    private int ComputePlayerHeal(Dino d)
    {
        // 기본 회복: maxHP * playerRecoverRatio
        int heal = Mathf.CeilToInt(d.maxHP * playerRecoverRatio);

        // 과도 회복 제한: missingHP * capRatio
        int missing = d.maxHP - d.hp;
        int cap = Mathf.CeilToInt(missing * playerRecoverCapMissingRatio);

        heal = Mathf.Min(heal, Mathf.Max(1, cap));
        return Mathf.Max(1, heal);
    }

    // ===================== 보스 턴(2페부터 확률적으로 단체공격) =====================
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

            // 단체 공격: 살아있는 3마리 모두에게 감소된 데미지
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
            // 단일 공격: 랜덤 생존 대상 1명
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

        // 다음 턴: 다시 1→2→3 행동 선택
        if (battleUI != null)
        {
            battleUI.Show(true);
            battleUI.SetButtonsInteractable(true);
        }

        StartPlayerPlanning();
    }

    // 보스 AoE 사용 여부(2페부터 확률)
    private bool ShouldBossUseAoe()
    {
        if (bossPhase < 2) return false;

        float chance = (bossPhase == 2) ? aoeChancePhase2 : aoeChancePhase3;

        // 생존 공룡이 1마리면 AoE 의미가 적으니 확률 낮춤
        int alive = CountAlive();
        if (alive <= 1) chance *= 0.10f;

        return Random.value < chance;
    }

    // ===================== 보스 페이즈(Up-only) =====================
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

    // ===================== 공통: 데미지/체크 =====================
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

    // ===================== HUD / 아이콘 표시 =====================
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
