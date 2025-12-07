using UnityEngine;
using Features.NPC.Interfaces;

namespace Features.NPC.Data
{
    [CreateAssetMenu(fileName = "NPCConfig", menuName = "LetsRoll/NPC/NPC Config")]
    public class NPCConfigSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _npcName = "NPC";
        [SerializeField] private NPCType _npcType = NPCType.Generic;
        [SerializeField, TextArea(2, 4)] private string _interactionPrompt = "Натисніть [E] для взаємодії";

        [Header("Detection")]
        [SerializeField] private float _interactionRadius = 3f;
        [SerializeField] private LayerMask _playerLayer;

        [Header("Visual Feedback")]
        [SerializeField] private bool _showFloatingPrompt = true;
        [SerializeField] private Vector3 _promptOffset = new Vector3(0f, 2.5f, 0f);
        [SerializeField] private float _promptFadeSpeed = 5f;

        [Header("Interaction Settings")]
        [SerializeField] private bool _facePlayerOnInteract = true;
        [SerializeField] private float _rotationSpeed = 5f;
        [SerializeField] private bool _canInteractDuringCombat = false;

        public string NPCName => _npcName;
        public NPCType NPCType => _npcType;
        public string InteractionPrompt => _interactionPrompt;
        public float InteractionRadius => _interactionRadius;
        public LayerMask PlayerLayer => _playerLayer;
        public bool ShowFloatingPrompt => _showFloatingPrompt;
        public Vector3 PromptOffset => _promptOffset;
        public float PromptFadeSpeed => _promptFadeSpeed;
        public bool FacePlayerOnInteract => _facePlayerOnInteract;
        public float RotationSpeed => _rotationSpeed;
        public bool CanInteractDuringCombat => _canInteractDuringCombat;
    }
}
