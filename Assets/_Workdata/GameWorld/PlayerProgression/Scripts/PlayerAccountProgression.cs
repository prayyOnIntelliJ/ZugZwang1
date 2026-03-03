using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class PlayerAccountProgression : MonoBehaviour
{
    [Separator("Rank Settings")] [SerializeField]
    private RanksSO rankSO;

    [HideInInspector] public List<RanksSO.Rank> ranks;

    private int levelUpCount;
    private ProgressBar progressBar;

    public int CurrentLevel
    {
        get => Prefs.GetKey<int>(Prefs.KEY_TYPES.LEVEL);
        set => Prefs.SetKey(Prefs.KEY_TYPES.LEVEL, value);
    }

    public int CurrentXp
    {
        get => Prefs.GetKey<int>(Prefs.KEY_TYPES.XP);
        set => Prefs.SetKey(Prefs.KEY_TYPES.XP, value);
    }

    private void Awake()
    {
        if (rankSO == null)
        {
            Debugger.LogError("RankSO missing.");
            return;
        }

        ranks = rankSO.ranks;
        progressBar = GetComponent<ProgressBar>();

        if (progressBar == null)
        {
            Debugger.LogWarning("ProgressBar missing on PlayerAccountProgression.");
        }
    }

    private void Start()
    {
        SetDefaultValues();
    }

    public float GetLevelProgress()
    {
        var rankIndex = GetCurrentRankIndex(CurrentLevel);
        if (rankIndex < 0 || rankIndex >= ranks.Count)
            return 0f;

        float xpNeeded = ranks[rankIndex].xpPerLevel;
        if (xpNeeded <= 0f)
            return 0f;

        return Mathf.Clamp01(CurrentXp / xpNeeded);
    }


    public int GetLevelUpCount() => levelUpCount;

    public int GetCurrentRankIndex() => GetCurrentRankIndex(CurrentLevel);

    public int GetCurrentRankIndex(int level)
    {
        if (ranks == null || ranks.Count == 0)
            return -1;

        if (level < ranks[0].levelToReach)
            return 0;

        for (var i = 0; i < ranks.Count - 1; i++)
        {
            if (level >= ranks[i].levelToReach &&
                level < ranks[i + 1].levelToReach)
                return i;
        }

        return ranks.Count - 1;
    }


    public void AddXp(int amount)
    {
        if (amount <= 0) return;

        levelUpCount = 0;

        int newXp = CurrentXp + amount;
        int newLevel = CurrentLevel;

        ProcessLevelUps(ref newXp, ref newLevel);

        CurrentXp = newXp;
        CurrentLevel = newLevel;

        if (progressBar != null)
        {
            int startingLevel = newLevel - levelUpCount;
            progressBar.StartFilling(startingLevel, levelUpCount);
        }
    }

    private void SetDefaultValues()
    {
        if (!Prefs.HasKey(Prefs.KEY_TYPES.LEVEL))
            CurrentLevel = 0;
        if (!Prefs.HasKey(Prefs.KEY_TYPES.XP))
            CurrentXp = 0;
    }

    private void ProcessLevelUps(ref int xp, ref int level)
    {
        int safety = 0;

        while (safety++ < 1000)
        {
            int rankIndex = GetCurrentRankIndex(level);
            if (rankIndex < 0 || rankIndex >= ranks.Count)
                break;

            int xpNeeded = ranks[rankIndex].xpPerLevel;
            if (xp < xpNeeded)
                break;

            xp -= xpNeeded;
            level++;
            levelUpCount++;
        }
    }
}