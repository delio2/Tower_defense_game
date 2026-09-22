using System;
using TowerDefense.Simulation;
using UnityEngine;

namespace TowerDefense.Presentation.Settings
{
    /// <summary>
    /// Local save of the run in progress (GDD v0.2 §2, §16): the save is the replay itself, written at every shop.
    /// Resuming re-simulates it, so a save can never disagree with the rules (D19). PlayerPrefs for the prototype.
    /// </summary>
    public static class RunSave
    {
        private const string Key = "save.run";

        public static bool Exists => PlayerPrefs.HasKey(Key);

        public static void Save(GameSimulation sim)
        {
            PlayerPrefs.SetString(Key, Replay.Record(sim).Serialize());
            PlayerPrefs.Save();
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Rebuilds the saved run up to its last shop. Returns null (and clears the save) when the text is unreadable,
        /// from another balance version, or not in a shop any more.
        /// </summary>
        public static GameSimulation TryResume(ContentDatabase content)
        {
            if (!Exists)
            {
                return null;
            }

            try
            {
                Replay replay = Replay.Deserialize(PlayerPrefs.GetString(Key));
                if (replay.BalanceVersion != RunConfig.BalanceVersion)
                {
                    Clear();
                    return null;
                }

                RunConfig config = RunSetup.Create(replay.Seed, replay.Core, replay.Grade, replay.Mode);
                GameSimulation sim = ReplayVerifier.Resimulate(replay, new GameSimulation(config, content));
                if (sim.IsOver || sim.Phase != GamePhase.Shop)
                {
                    Clear();
                    return null;
                }

                return sim;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Saved run discarded: {e.Message}");
                Clear();
                return null;
            }
        }
    }
}
