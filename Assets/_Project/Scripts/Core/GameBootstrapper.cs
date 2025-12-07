using UnityEngine;

namespace Core
{
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            Debug.Log("[GameBootstrapper] Initializing Core Systems...");

            RegisterInputService();
            RegisterExperienceService();
            RegisterNPCInteractionService();
            CreateCoroutineRunner();

            Debug.Log("[GameBootstrapper] Initialization Complete.");
        }

        private static void RegisterInputService()
        {
            ServiceLocator.Register<Core.Input.IInputService>(new Core.Input.PlayerInputService());
        }

        private static void RegisterExperienceService()
        {
            ServiceLocator.Register<Features.Experience.IExperienceService>(
                new Features.Experience.ExperienceService());
        }

        private static void RegisterNPCInteractionService()
        {
            ServiceLocator.Register<Features.NPC.INPCInteractionService>(
                new Features.NPC.NPCInteractionService());
        }

        private static void CreateCoroutineRunner()
        {
            var coroutineRunner = new GameObject("[CoroutineRunner]").AddComponent<CoroutineRunner>();
            GameObject.DontDestroyOnLoad(coroutineRunner.gameObject);
            ServiceLocator.Register<ICoroutineRunner>(coroutineRunner);
        }
    }

    public interface ICoroutineRunner
    {
        UnityEngine.Coroutine StartCoroutine(System.Collections.IEnumerator routine);
        void StopCoroutine(UnityEngine.Coroutine routine);
    }

    public class CoroutineRunner : MonoBehaviour, ICoroutineRunner
    {
        public new UnityEngine.Coroutine StartCoroutine(System.Collections.IEnumerator routine)
        {
            return base.StartCoroutine(routine);
        }

        public new void StopCoroutine(UnityEngine.Coroutine routine)
        {
            if (routine != null)
            {
                base.StopCoroutine(routine);
            }
        }
    }
}

