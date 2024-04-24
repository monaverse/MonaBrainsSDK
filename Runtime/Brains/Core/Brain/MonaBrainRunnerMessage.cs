using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using Mona.SDK.Brains.Core.Brain;

public class MonaBrainRunnerMessage : MonoBehaviour
{
    [Serializable]
    public class MonaBrainUnityEvent
    {
        public string Message;
        public UnityEvent Event;
    }

    [SerializeField]
    public List<MonaBrainUnityEvent> Events;

    private IMonaBrainRunner _runner;

    public void Awake()
    {
        _runner = GetComponent<IMonaBrainRunner>();
        _runner.OnMessage += HandleMessage;
    }

    private void HandleMessage(string message)
    {
        for(var i = 0;i < Events.Count; i++)
        {
            var evt = Events[i];
            if (evt.Message == message)
                evt.Event?.Invoke();
        }
    }
}
