﻿using Hedgehog.Core.Actors;
using UnityEngine;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// Helper class for creating objects that react to activation.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(ObjectTrigger))]
    public class ReactiveObject : BaseReactive
    {
        protected bool RegisteredEvents;

        public override void Awake()
        {
            base.Awake();
            ObjectTrigger = GetComponent<ObjectTrigger>();
        }

        public virtual void OnEnable()
        {
            if (ObjectTrigger.OnActivateEnter != null) Start();
        }

        public override void Start()
        {
            base.Start();

            if (RegisteredEvents) return;

            ObjectTrigger.OnActivateEnter.AddListener(NotifyActivateEnter);
            ObjectTrigger.OnActivateStay.AddListener(NotifyActivateStay);
            ObjectTrigger.OnActivateExit.AddListener(NotifyActivateExit);

            RegisteredEvents = true;
        }

        public virtual void OnDisable()
        {
            if (!RegisteredEvents) return;

            ObjectTrigger.OnActivateEnter.RemoveListener(NotifyActivateEnter);
            ObjectTrigger.OnActivateStay.RemoveListener(NotifyActivateStay);
            ObjectTrigger.OnActivateExit.RemoveListener(NotifyActivateExit);

            RegisteredEvents = false;
        }

        public virtual void OnActivateEnter(HedgehogController controller)
        {
            
        }

        public virtual void OnActivateStay(HedgehogController controller)
        {
            
        }

        public virtual void OnActivateExit(HedgehogController controller)
        {
            
        }
        #region Notify Methods
        public void NotifyActivateEnter(HedgehogController controller)
        {
            controller.NotifyReactiveEnter(this);
            OnActivateEnter(controller);
        }

        public void NotifyActivateStay(HedgehogController controller)
        {
            // here for consistency, may add something later
            OnActivateStay(controller);
        }

        public void NotifyActivateExit(HedgehogController controller)
        {
            controller.NotifyReactiveExit(this);
            OnActivateExit(controller);
        }
        #endregion
    }
}