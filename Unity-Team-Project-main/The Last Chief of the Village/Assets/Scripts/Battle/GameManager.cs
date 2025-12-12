using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public BattleUI battleUI;
    public TMP_Text playerHPText;   // PlayerHPText
    public TMP_Text enemyHPText;    // EnemyHPText

    [Header("Party Icons (Canvas: PlayerDino / PlayerDino2 / PlayerDino3)")]
    public Image playerDinoIcon1;
    public Image playerDinoIcon2;
    public Image playerDinoIcon3;

    [Header("Hit Shake (Optional)")]
    public BattleShaker playerShaker;
    public BattleShaker enemyShaker;

    // ===================== Tier(하=1, 중=2, 상=3) =====================
    public enum Tier123
    {
        Low_1 = 1,
        Mid_2 = 2,
        High_3 = 3
    }

    [Header("Player Dino Tier (Inspector)")]
    [Tooltip("PlayerDino1 = Starter(고정 스탯). Tier는 표시만 되고 실제 스탯엔 영향 없음.")]
    public Tier123 playerDino1Tier = Tier123.High_3;

    [Tooltip("PlayerDino2 = 공격형(딜러) Tier")]
    public Tier123 playerDino2Tier = Tier123.Mid_2;

    [Tooltip("PlayerDino3 = 방어형(탱커) Tier")]
    public Tier123 playerDino3Tier = Tier123.Mid_2;

    // ===================== Boss =====================
    [Header("Boss (Fixed Spec)")]
    public int bossMaxHP = 450;

    // 페이즈 기준 (HP 비율)
    private const float PHASE1_MIN = 2f / 3f; // 66.7% 초과 -> 1페
    private const float PHASE2_MIN = 1f / 3f; // 33.3% 초과 -> 2페, 그 이하는 3페

    // ===================== Damage / Recover =====================
    [Header("Damage Formula")]
    [Tooltip("데미지 = atk - def * defenseWeight (최소 minDamage)")]
    [Range(0f, 1.5f)]
    public float defenseWeight = 0.5f;
    public int minDamage = 1;

    [Header("Player Recover (Team Heal)")]
    [Tooltip("Recover 시, 살아있는 각 공룡이 최대HP의 이 비율만큼 회복")]
    [Range(0.05f, 0.25f)]
    public float playerRecoverRatio = 0.12f; // 기본 12%

    [Header("Boss Recover (Random)")]
    [Tooltip("보스 턴에 Recover를 선택할 확률(기본)")]
    [Range(0f, 1f)]
    public float bossRecoverChance = 0.25f;

    [Tooltip("보스 HP가 33% 이하(3페 진입 구간)일 때 Recover 확률(기본보다 높게 추천)")]
    [Range(0f, 1f)]
    public float bossRecoverChanceLowHP = 0.40f;

    [Tooltip("보스 Recover 회복량: maxHP * 랜덤비율(min~max)")]
    [Range(0.01f, 0.30f)]
    public float bossRecoverMinRatio = 0.06f;

    [Range(0.01f, 0.30f)]
    public float bossRecoverMaxRatio = 0.10f;

    // ===================== Runtime =====================
    private class Dino
    {
        public string name;
        public int maxHP;
        public int atk;
        public int def;
        public int hp;
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

    private enum State { PlayerTurn, Busy, Ended }
    private State state = State.Busy;

    private int bossPhase = 1;          // 1->2->3 (올라가기만 함)
    private int focusIndex = -1;        // 아이콘 강조용(공격 중/피격 대상)

    private Image[] partyIcons;
    private Vector3[] iconBaseScale;

    // ===================== Unity =====================
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BuildParty();
        StartBattle();
    }

    // ===================== Party Setup =====================
    private void BuildParty()
    {
        party.Clear();

        // PlayerDino1: Starter 고정
        party.Add(new Dino("Starter", 150, 24, 22));

        // PlayerDino2: Attacker Tier
        party.Add(CreateAttacker((int)playerDino2Tier));

        // PlayerDino3: Tank Tier
        party.Add(CreateTank((int)playerDino3Tier));

        boss = new Dino("Boss", bossMaxHP, 0, 0);

        bossPhase = 1;
        RefreshBossPhaseByHP(); // 시작은 1페

        partyIcons = new Image[3] { playerDinoIcon1, playerDinoIcon2, playerDinoIcon3 };
        iconBaseScale = new Vector3[3];
        for (int i = 0; i < 3; i++)
        {
            if (partyIcons[i] != null) iconBaseScale[i] = partyIcons[i].transform.localScale;
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

    // ===================== Battle Start =====================
    private void StartBattle()
    {
        state = State.PlayerTurn;
        focusIndex = -1;

        UpdateHUD();
        UpdatePartyIcons();

        if (battleUI != null)
        {
            battleUI.ShowPlayerTurn();
            battleUI.SetMessage("플레이어는 무엇을 할 지 선택하세요!");
        }
    }

    // ===================== Button Events =====================
    public void PlayerAttack()
    {
        if (!CanPlayerAct()) return;
        StartCoroutine(PlayerAllAttackFlow()); // ★ 한 턴에 3마리 모두 공격
    }

    public void PlayerDefend() // Recover
    {
        if (!CanPlayerAct()) return;
        StartCoroutine(PlayerRecoverFlow());
    }

    private bool CanPlayerAct()
    {
        if (state != State.PlayerTurn) return false;
        if (boss == null || boss.Dead) return false;
        if (AllPartyDead()) return false;
        return true;
    }

    // ===================== Player Turn: 3 attacks =====================
    private IEnumerator PlayerAllAttackFlow()
    {
        state = State.Busy;

        if (battleUI != null)
        {
            battleUI.Show(true);
            battleUI.SetButtonsInteractable(false);
            battleUI.SetMessage("파티의 연속 공격!");
        }

        yield return new WaitForSeconds(0.25f);

        for (int i = 0; i < party.Count; i++)
        {
            if (party[i].Dead) continue;

            focusIndex = i;
            UpdatePartyIcons();

            if (battleUI != null)
                battleUI.SetMessage(party[i].name + " 공격!");

            yield return new WaitForSeconds(0.15f);

            int dmg = ComputeDamage(party[i].atk, GetBossDefense());
            ApplyDamage(boss, dmg);

            if (enemyShaker != null) enemyShaker.Shake();

            RefreshBossPhaseByHP(); // HP 내려가면 페이즈가 올라갈 수 있음
            UpdateHUD();

            yield return new WaitForSeconds(0.40f);

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

        // Boss turn
        yield return BossTurnFlow();
    }

    // ===================== Player Recover: Team Heal =====================
    private IEnumerator PlayerRecoverFlow()
    {
        state = State.Busy;

        if (battleUI != null)
        {
            battleUI.Show(true);
            battleUI.SetButtonsInteractable(false);
        }

        int totalHealed = 0;

        for (int i = 0; i < party.Count; i++)
        {
            if (party[i].Dead) continue;

            int heal = Mathf.Max(1, Mathf.CeilToInt(party[i].maxHP * playerRecoverRatio));
            int before = party[i].hp;
            party[i].hp = Mathf.Min(party[i].maxHP, party[i].hp + heal);
            totalHealed += (party[i].hp - before);
        }

        if (battleUI != null)
            battleUI.SetMessage("Recover! (파티 총 +" + totalHealed + ")");

        UpdateHUD();
        UpdatePartyIcons();

        yield return new WaitForSeconds(0.8f);

        // Boss turn
        yield return BossTurnFlow();
    }

    // ===================== Boss Turn: Random Attack or Recover =====================
    private IEnumerator BossTurnFlow()
    {
        if (boss.Dead)
        {
            EndBattle(true);
            yield break;
        }

        if (battleUI != null)
            battleUI.ShowEnemyTurn(); // 버튼 잠금

        yield return new WaitForSeconds(0.5f);

        bool doRecover = ShouldBossRecover();

        if (doRecover && boss.hp < boss.maxHP)
        {
            // Boss Recover
            float rMin = Mathf.Min(bossRecoverMinRatio, bossRecoverMaxRatio);
            float rMax = Mathf.Max(bossRecoverMinRatio, bossRecoverMaxRatio);
            float ratio = Random.Range(rMin, rMax);

            int heal = Mathf.Max(1, Mathf.CeilToInt(boss.maxHP * ratio));
            int before = boss.hp;
            boss.hp = Mathf.Min(boss.maxHP, boss.hp + heal);
            int realHeal = boss.hp - before;

            // 페이즈는 내려가지 않게 유지(요구: 페이즈 기반 3페, 전환 후 고정 느낌)
            // RefreshBossPhaseByHP()는 "올라가기만" 하므로 호출해도 문제 없음
            RefreshBossPhaseByHP();

            if (battleUI != null)
            {
                battleUI.Show(true);
                battleUI.SetMessage("보스 Recover! (+" + realHeal + ")");
            }

            UpdateHUD();
            yield return new WaitForSeconds(0.9f);
        }
        else
        {
            // Boss Attack (단일 공격 1회)
            int target = FindRandomAliveIndex();
            if (target == -1)
            {
                EndBattle(false);
                yield break;
            }

            focusIndex = target;
            UpdatePartyIcons();

            int dmg = ComputeDamage(GetBossAttack(), party[target].def);
            ApplyDamage(party[target], dmg);

            if (playerShaker != null) playerShaker.Shake();

            if (battleUI != null)
            {
                battleUI.Show(true);
                battleUI.SetMessage("보스의 공격! (" + party[target].name + " -" + dmg + ")");
            }

            UpdateHUD();
            UpdatePartyIcons();

            yield return new WaitForSeconds(0.9f);

            if (AllPartyDead())
            {
                focusIndex = -1;
                UpdatePartyIcons();
                EndBattle(false);
                yield break;
            }
        }

        // Next player turn
        focusIndex = -1;
        UpdatePartyIcons();

        state = State.PlayerTurn;
        if (battleUI != null)
            battleUI.ShowPlayerTurn();
    }

    private bool ShouldBossRecover()
    {
        float hpRatio = boss.hp / (float)boss.maxHP;

        // 3페 구간이면 Recover 확률을 더 높게
        float chance = (hpRatio <= PHASE2_MIN) ? bossRecoverChanceLowHP : bossRecoverChance;

        // 완전 풀피면 Recover 의미 없으니 거의 안 뜨게
        if (boss.hp >= boss.maxHP) chance *= 0.1f;

        return Random.value < chance;
    }

    // ===================== Boss Phase (Up-only) =====================
    private void RefreshBossPhaseByHP()
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

    // ===================== Damage =====================
    private int ComputeDamage(int atk, int def)
    {
        float raw = atk - def * defenseWeight;
        raw = Mathf.Max(minDamage, raw);
        return Mathf.Max(minDamage, Mathf.RoundToInt(raw));
    }

    private void ApplyDamage(Dino target, int dmg)
    {
        target.hp -= dmg;
        if (target.hp < 0) target.hp = 0;
    }

    // ===================== Target / Checks =====================
    private bool AllPartyDead()
    {
        for (int i = 0; i < party.Count; i++)
            if (!party[i].Dead) return false;
        return true;
    }

    private int FindRandomAliveIndex()
    {
        List<int> alive = new List<int>(3);
        for (int i = 0; i < party.Count; i++)
            if (!party[i].Dead) alive.Add(i);

        if (alive.Count == 0) return -1;
        return alive[Random.Range(0, alive.Count)];
    }

    // ===================== HUD / Icons =====================
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

            // 죽은 공룡 반투명
            Color c = img.color;
            c.a = party[i].Dead ? 0.25f : 1f;
            img.color = c;

            // focusIndex 강조(공격 중/피격 대상)
            if (iconBaseScale != null && iconBaseScale.Length == 3)
            {
                if (!party[i].Dead && i == focusIndex)
                    img.transform.localScale = iconBaseScale[i] * 1.15f;
                else
                    img.transform.localScale = iconBaseScale[i];
            }
        }
    }

    // ===================== End =====================
    private void EndBattle(bool playerWon)
    {
        state = State.Ended;
        if (battleUI != null)
            battleUI.ShowResult(playerWon);
    }
}
