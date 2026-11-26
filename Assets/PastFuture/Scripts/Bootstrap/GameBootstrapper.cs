using System;
using UnityEngine;

namespace PastFuture.Bootstrap
{
    /// <summary>
    /// Unity entry point placed in the Bootstrap scene (Handover 16-17, ADR-569).
    /// Creates the <see cref="CompositionRoot"/> once, keeps it across scene loads and
    /// disposes it when destroyed (including application quit). It owns no game rules
    /// and never writes WorldState (ADR-484).
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10000)]
    public sealed class GameBootstrapper : MonoBehaviour
    {
        private const string LogPrefix = "[PastFuture.Bootstrap]";

        // Only static state in the bootstrap. Reset explicitly so repeated Editor Play
        // sessions never see a stale instance even if domain reload is disabled (UNITY_T008).
        private static GameBootstrapper s_active;

        private CompositionRoot _root;

        /// <summary>The composition root this bootstrapper owns; null on a destroyed duplicate. Test access only.</summary>
        internal CompositionRoot Root => _root;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_active = null;
        }

        private void Awake()
        {
            if (s_active != null && s_active != this)
            {
                // A second Bootstrap scene/object was loaded: keep the first one (UNITY_T003).
                Debug.LogWarning($"{LogPrefix} Duplicate GameBootstrapper destroyed; composition root already exists.", this);
                Destroy(gameObject);
                return;
            }

            s_active = this;
            DontDestroyOnLoad(gameObject);
            _root = new CompositionRoot();
            _root.StartSceneBindings();
            Debug.Log($"{LogPrefix} Composition root created ({_root.ApplicationScope.Kind} scope).", this);
        }

        private void OnDestroy()
        {
            if (s_active != this)
            {
                return;
            }

            s_active = null;
            try
            {
                _root?.Dispose();
                Debug.Log($"{LogPrefix} Application scope disposed.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                _root = null;
            }
        }
    }
}
