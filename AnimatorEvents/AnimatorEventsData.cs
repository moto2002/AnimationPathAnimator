﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ATP.SimplePathAnimator.PathEvents {

    public class AnimatorEventsData : ScriptableObject {

        [SerializeField]
        private List<NodeEvent> nodeEvents;

        public List<NodeEvent> NodeEvents {
            get { return nodeEvents; }
            set { nodeEvents = value; }
        }

        public void ResetEvents() {
            NodeEvents.Clear();
        }

        private void OnEnable() {
            NodeEvents = new List<NodeEvent>();
        }

    }

}
