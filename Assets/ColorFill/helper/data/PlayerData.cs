using UnityEngine;
namespace ColorFill.helper.data
{
    public class PlayerData
    {
        private int currentLevel;

        public int CurrentLevel
        {
            get => currentLevel;
            set
            {
                currentLevel = value;
                Save();
            }
        }

        private int gemCount;

        public int GemCount
        {
            get => gemCount;
            set
            {
                gemCount = value;
                Save();
            }
        }


        public PlayerData()
        {
            currentLevel = PlayerPrefs.GetInt(nameof(currentLevel),1);
            gemCount = PlayerPrefs.GetInt(nameof(gemCount), 0);
        }

        public void Save()
        {
            PlayerPrefs.SetInt(nameof(currentLevel),currentLevel);
            PlayerPrefs.SetInt(nameof(gemCount),gemCount);
        }
    }
}