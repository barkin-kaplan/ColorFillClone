using System.Collections;
using ColorFill.helper;
using UnityEngine;

namespace ColorFill.game.camera
{
    public class GameCamera : MonoBehaviour
    {
        private Vector3 Stage1Position = new Vector3(0, -11.4f, -50f);
        private Vector3 Stage2Position = new Vector3(0, 21.2f, 72);
        public static GameCamera Instance { get; private set; }

        void Awake()
        {
            if (!Util.DontDestroyOnLoad<GameCamera>(gameObject))
            {
                return;
            }
            Instance = this;
        }

        public void AdjustStage1()
        {
            transform.position = Stage1Position;
        }

        public void AdjustStage2()
        {
            StartCoroutine(AdjustStage2CoRoutine());
        }

        IEnumerator AdjustStage2CoRoutine()
        {
            int frameCount = 60;
            for (int i = 1; i <= frameCount; i++)
            {
                transform.position = Vector3.Lerp(Stage1Position, Stage2Position, (float) i / frameCount);
                yield return null;
            }
        }
    }
}
