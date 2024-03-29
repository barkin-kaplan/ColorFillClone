﻿using System.Collections;
using ColorFill.helper;
using UnityEngine;

namespace ColorFill.game.camera
{
    public class GameCamera : MonoBehaviour
    {
        private Vector3 Stage1Position = new Vector3(0, -8.9f, -45.5f);
        private Vector3 Stage2Position = new Vector3(0, 21.2f, -72);
        private const float TargetHorizontalFOV = 14.6f;
        public static GameCamera Instance { get; private set; }

        void Awake()
        {
            if (!Util.DontDestroyOnLoad<GameCamera>(gameObject))
            {
                return;
            }
            Instance = this;
            var verticalFov = TargetHorizontalFOV * Screen.height / Screen.width;
            GetComponent<Camera>().fieldOfView = verticalFov;
        }

        public void AdjustStage1()
        {
            transform.position = Stage1Position;
        }

        public void AdjustStage2(int frameCount)
        {
            StartCoroutine(AdjustStage2CoRoutine(frameCount));
        }

        IEnumerator AdjustStage2CoRoutine(int frameCount)
        {
            for (int i = 1; i <= frameCount; i++)
            {
                transform.position = Vector3.Lerp(Stage1Position, Stage2Position, (float) i / frameCount);
                yield return new WaitForSeconds(Util.FrameWaitAmount);
            }
        }
    }
}
