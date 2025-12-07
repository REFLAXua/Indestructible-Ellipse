using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Features.NPC.Handlers
{
    public class CompositeHandler : NPCInteractionHandlerBase
    {
        [Header("Composite Settings")]
        [SerializeField] private List<NPCInteractionHandlerBase> _handlers = new List<NPCInteractionHandlerBase>();
        [SerializeField] private CompositeMode _mode = CompositeMode.Sequential;
        
        [Header("Events")]
        [SerializeField] private UnityEvent _onAllHandlersExecuted;

        private int _currentHandlerIndex;

        public override void Execute(NPCController npc)
        {
            if (_handlers == null || _handlers.Count == 0)
            {
                Debug.LogWarning("[CompositeHandler] Немає обробників для виконання");
                return;
            }

            _currentHandlerIndex = 0;

            switch (_mode)
            {
                case CompositeMode.Sequential:
                    ExecuteSequential(npc);
                    break;
                case CompositeMode.All:
                    ExecuteAll(npc);
                    break;
                case CompositeMode.FirstValid:
                    ExecuteFirstValid(npc);
                    break;
            }
        }

        private void ExecuteSequential(NPCController npc)
        {
            if (_currentHandlerIndex < _handlers.Count)
            {
                var handler = _handlers[_currentHandlerIndex];
                if (handler != null)
                {
                    handler.Execute(npc);
                }
                _currentHandlerIndex++;
            }

            if (_currentHandlerIndex >= _handlers.Count)
            {
                _onAllHandlersExecuted?.Invoke();
            }
        }

        private void ExecuteAll(NPCController npc)
        {
            foreach (var handler in _handlers)
            {
                if (handler != null)
                {
                    handler.Execute(npc);
                }
            }
            _onAllHandlersExecuted?.Invoke();
        }

        private void ExecuteFirstValid(NPCController npc)
        {
            foreach (var handler in _handlers)
            {
                if (handler != null && handler.enabled)
                {
                    handler.Execute(npc);
                    break;
                }
            }
        }

        public void AdvanceToNextHandler(NPCController npc)
        {
            if (_mode == CompositeMode.Sequential)
            {
                ExecuteSequential(npc);
            }
        }

        public override void OnInteractionStart(NPCController npc)
        {
            base.OnInteractionStart(npc);
            
            foreach (var handler in _handlers)
            {
                if (handler != null)
                {
                    handler.OnInteractionStart(npc);
                }
            }
        }

        public override void OnInteractionEnd(NPCController npc)
        {
            base.OnInteractionEnd(npc);
            
            foreach (var handler in _handlers)
            {
                if (handler != null)
                {
                    handler.OnInteractionEnd(npc);
                }
            }
        }
    }

    public enum CompositeMode
    {
        Sequential,
        All,
        FirstValid
    }
}
