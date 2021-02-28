using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ColorFill.helper.data;
using ColorFill.helper.DI;
using ColorFill.helper.flow;
using ColorFill.helper.input_helper;
using UnityEngine;

namespace ColorFill.helper.context
{
    public class MainContext : MonoBehaviour
    {
        public static MainContext Instance { get; private set; }
        [SerializeField] private GameObject [] _monoBehaviours;   
        private ConcurrentQueue<Action> _invokeQueue = new ConcurrentQueue<Action>();
        private List<IManualUpdate> _manualUpdates = new List<IManualUpdate>();

        
        // Start is called before the first frame update
        void Awake()
        {
            Util.DontDestroyOnLoad<MainContext>(gameObject);
            Instance = this;
            CreateMonoBehaviours();
            RegisterContextElements();
        }

        private void Start()
        {
            Util.ThreadStart(() =>
            {
                Thread.Sleep(500);
                Invoke(() =>
                {
                    FlowManager.LoadScene(FlowManager.GameIndex);
                });
            });
        }

        void RegisterContextElements()
        {
            
            var touchHelper = DIContainer.RegisterSingle<TouchHelper>();
            _manualUpdates.Add(touchHelper);
            DIContainer.RegisterSingle<PlayerData>();
        }

        void CreateMonoBehaviours()
        {
            foreach (var monoBehaviour in _monoBehaviours)
            {
                Instantiate(monoBehaviour);
            }
        }

        // Update is called once per frame
        void Update()
        {
            while (_invokeQueue.TryDequeue(out var action))
            {
                action();
            }

            foreach (var manualUpdate in _manualUpdates)
            {
                manualUpdate.ManualUpdate();
            }
        }

        public void Invoke(Action action)
        {
            _invokeQueue.Enqueue(action);
        }


        public void Play()
        {
            
        }
        
    }
}