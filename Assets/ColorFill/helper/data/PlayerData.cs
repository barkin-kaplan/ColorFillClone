using UnityEngine;
namespace ColorFill.helper.data
{
    public class PlayerData
    {
        public int CurrentLevel { get; set; }
        public int GemCount { get; set; }


        public PlayerData()
        {
            CurrentLevel = PlayerPrefs.GetInt(nameof(CurrentLevel),1);
            GemCount = PlayerPrefs.GetInt(nameof(GemCount), 0);
        }

        public void Save()
        {
            PlayerPrefs.SetInt(nameof(CurrentLevel),CurrentLevel);
            PlayerPrefs.SetInt(nameof(GemCount),GemCount);
        }
    }
}