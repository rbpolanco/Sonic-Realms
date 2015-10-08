﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hedgehog.Core.Actors;
using Hedgehog.Core.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Hedgehog.Core.Triggers
{
    /// <summary>
    /// An event for when a controller lands on a platform, invoked with the offending controller,
    /// the offending platform, and the priority the surface holds.
    /// </summary>
    [Serializable]
    public class PlatformSurfaceEvent : UnityEvent<HedgehogController, TerrainCastHit> { }

    /// <summary>
    /// Can be added to a collider to receive events when a controller stands on it.
    /// </summary>
    [AddComponentMenu("Hedgehog/Platforms/Platform Trigger")]
    public class PlatformTrigger : BaseTrigger
    {
        /// <summary>
        /// Called when a controller lands on the surface of the platform.
        /// </summary>
        [SerializeField]
        public PlatformSurfaceEvent OnSurfaceEnter;

        /// <summary>
        /// Called while a controller is on the surface of the platform.
        /// </summary>
        [SerializeField]
        public PlatformSurfaceEvent OnSurfaceStay;

        /// <summary>
        /// Called when a controller exits the surface of the platform.
        /// </summary>
        [SerializeField]
        public PlatformSurfaceEvent OnSurfaceExit;

        /// <summary>
        /// Returns whether the platform should be collided with based on the result of the specified
        /// terrain cast.
        /// </summary>
        /// <param name="hit">The specified terrain cast.</param>
        /// <returns></returns>
        public delegate bool CollisionPredicate(TerrainCastHit hit);

        /// <summary>
        /// A list of predicates which, if empty or all return true, allow collision with the
        /// platform.
        /// </summary>
        public ICollection<CollisionPredicate> CollisionRules;

        /// <summary>
        /// Returns whether the controller is considered to be on the surface.
        /// </summary>
        /// <returns></returns>
        public delegate bool SurfacePredicate(HedgehogController controller, TerrainCastHit hit);

        /// <summary>
        /// A list of predicates which invokes surface events based on whether it is empty or all
        /// return true.
        /// </summary>
        public ICollection<SurfacePredicate> SurfaceRules;

        /// <summary>
        /// A list of current surface collisions.
        /// </summary>
        public List<TerrainCastHit> Collisions;

        public override void Reset()
        {
            base.Reset();

            OnSurfaceEnter = new PlatformSurfaceEvent();
            OnSurfaceStay = new PlatformSurfaceEvent();
            OnSurfaceExit = new PlatformSurfaceEvent();
        }

        public override void Awake()
        {
            base.Awake();

            OnSurfaceEnter = OnSurfaceEnter ?? new PlatformSurfaceEvent();
            OnSurfaceStay = OnSurfaceStay ?? new PlatformSurfaceEvent();
            OnSurfaceExit = OnSurfaceExit ?? new PlatformSurfaceEvent();
            CollisionRules = new List<CollisionPredicate>();
            SurfaceRules = new List<SurfacePredicate>();
            Collisions = new List<TerrainCastHit>();
        }

        public virtual void OnEnable()
        {
            if (!TriggerFromChildren) return;
            foreach (var childCollider in transform.GetComponentsInChildren<Collider2D>())
            {
                if (childCollider.transform == transform) continue;
                if(childCollider.gameObject.GetComponent<PlatformTrigger>() == null)
                    childCollider.gameObject.AddComponent<PlatformTrigger>();
            }
        }

        public virtual void FixedUpdate()
        {
            foreach (var hit in Collisions)
            {
                OnSurfaceStay.Invoke(hit.Source, hit);
                OnStay.Invoke(hit.Source);
            }
        }

        public override bool HasController(HedgehogController controller)
        {
            return Collisions.Any(hit => hit.Source == controller);
        }

        private void CheckSurface(HedgehogController controller, TerrainCastHit hit, bool isExit = false)
        {
            if (!enabled || controller == null) return;

            var found = Collisions.FirstOrDefault(castHit => castHit.Source == controller);
            if (isExit)
            {
                if (found == null || found.Hit.transform != hit.Hit.transform) return;
                Collisions.Remove(found);
                OnSurfaceExit.Invoke(controller, hit);
                OnExit.Invoke(controller);
            } else if (found == null)
            {
                Collisions.Add(hit);
                OnSurfaceEnter.Invoke(controller, hit);
                OnEnter.Invoke(controller);
            }
            else
            {
                Collisions[Collisions.FindIndex(castHit => castHit.Source == controller)] = hit;
            }
        }
        
        private void BubbleEvent(HedgehogController controller, TerrainCastHit hit, bool isExit = false)
        {
            foreach (var trigger in GetComponentsInParent<PlatformTrigger>().Where(
                trigger => trigger != this && trigger.TriggerFromChildren))
            {
                trigger.CheckSurface(controller, hit, isExit);
            }
        }

        public void UpdateController(HedgehogController controller, TerrainCastHit hit, bool isExit = false)
        {
            CheckSurface(controller, hit, isExit);
            BubbleEvent(controller, hit, isExit);
        }

        public bool IsOnSurface(HedgehogController controller, TerrainCastHit hit)
        {
            return (!SurfaceRules.Any() && DefaultSurfaceRule(controller, hit))
                || SurfaceRules.All(predicate => predicate(controller, hit));
        }

        /// <summary>
        /// Returns whether the platform can be collided with based on its list of collision predicates
        /// and the specified results of a terrain cast.
        /// </summary>
        /// <param name="hit">The specified results of a terrain cast.</param>
        /// <returns></returns>
        public bool CollidesWith(TerrainCastHit hit)
        {
            return (!CollisionRules.Any() && DefaultCollisionRule(hit))
                || CollisionRules.All(predicate => predicate(hit));
        }

        public bool DefaultSurfaceRule(HedgehogController controller, TerrainCastHit hit)
        {
            return controller.PrimarySurface == hit.Hit.transform ||
                   controller.SecondarySurface == hit.Hit.transform;
        }

        public bool DefaultCollisionRule(TerrainCastHit hit)
        {
            return true;
        }
    }
}
