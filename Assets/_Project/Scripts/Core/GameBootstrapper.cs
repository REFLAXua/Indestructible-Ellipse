using UnityEngine;

namespace Core
{
    /// <summary>
    /// Entry point for the game. Initializes core services before any scene logic runs.
    /// </summary>
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            Debug.Log("[GameBootstrapper] Initializing Core Systems...");

            // Initialize Services here
            ServiceLocator.Register<Core.Input.IInputService>(new Core.Input.PlayerInputService());
            
            // Note: MonoBehaviours that need to persist can be created here
            var coroutineRunner = new GameObject("[CoroutineRunner]").AddComponent<CoroutineRunner>();
            GameObject.DontDestroyOnLoad(coroutineRunner.gameObject);
            ServiceLocator.Register<ICoroutineRunner>(coroutineRunner);

            Debug.Log("[GameBootstrapper] Initialization Complete.");
        }
    }

    public interface ICoroutineRunner { }
    
    public class CoroutineRunner : MonoBehaviour, ICoroutineRunner { }
}
