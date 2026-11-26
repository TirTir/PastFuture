using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PastFuture.Bootstrap;
using UnityEngine.TestTools;

namespace PastFuture.Tests.PlayMode.Bootstrap
{
    /// <summary>
    /// PlayMode checks for the Bootstrap scene and <see cref="GameBootstrapper"/> (ADR-565, ADR-569).
    /// UNITY_T003: Bootstrap creates its services only once.
    /// UNITY_T008: repeated Play sessions do not leak static state. Covered here as two
    /// bootstrap sessions in one domain, which is what Editor Play repeats look like when
    /// domain reload is disabled. Pressing Play/Stop in the Editor itself is not automated.
    /// </summary>
    [Category("Bootstrap")]
    public sealed class BootstrapPlayModeTests
    {
        private const string BootstrapSceneName = "Bootstrap";
        private const string DuplicateWarning = "Duplicate GameBootstrapper destroyed";

        [UnityTearDown]
        public IEnumerator DestroyBootstrappers()
        {
            // GameBootstrapper lives in DontDestroyOnLoad, so it would outlive the test otherwise.
            foreach (var bootstrapper in FindBootstrappers())
            {
                UnityEngine.Object.Destroy(bootstrapper.gameObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator UNITY_T003_BootstrapSceneLoadedTwice_KeepsSingleCompositionRoot()
        {
            yield return LoadBootstrapScene();
            var first = SingleBootstrapper();
            var root = first.Root;
            Assert.That(root, Is.Not.Null, "First bootstrapper did not create a composition root.");

            LogAssert.Expect(UnityEngine.LogType.Warning, new Regex(DuplicateWarning));
            yield return LoadBootstrapScene();
            yield return null; // Destroy of the duplicate happens at the end of the frame.

            var survivor = SingleBootstrapper();
            Assert.That(survivor, Is.SameAs(first), "The original bootstrapper must survive the reload.");
            Assert.That(survivor.Root, Is.SameAs(root), "The composition root must not be recreated.");
            Assert.That(root.ApplicationScope.IsDisposed, Is.False, "The surviving root must stay alive.");
        }

        [UnityTest]
        public IEnumerator UNITY_T008_BootstrapRestartedInSameDomain_CreatesFreshRootAndDisposesPrevious()
        {
            var warnings = new List<string>();
            void Collect(string message, string stackTrace, UnityEngine.LogType type)
            {
                if (type == UnityEngine.LogType.Warning && message.Contains(DuplicateWarning))
                {
                    warnings.Add(message);
                }
            }

            UnityEngine.Application.logMessageReceived += Collect;
            try
            {
                yield return LoadBootstrapScene();
                var firstSession = SingleBootstrapper();
                var firstRoot = firstSession.Root;
                Assert.That(firstRoot, Is.Not.Null);

                // Leaving Play Mode destroys every object, including DontDestroyOnLoad ones.
                UnityEngine.Object.Destroy(firstSession.gameObject);
                yield return null;
                Assert.That(firstRoot.ApplicationScope.IsDisposed, Is.True, "Ending a session must dispose its root.");
                Assert.That(FindBootstrappers(), Is.Empty);

                yield return LoadBootstrapScene();
                var secondSession = SingleBootstrapper();
                Assert.That(secondSession, Is.Not.SameAs(firstSession));
                Assert.That(secondSession.Root, Is.Not.Null.And.Not.SameAs(firstRoot),
                    "The second session must build a fresh composition root.");
                Assert.That(secondSession.Root.ApplicationScope.IsDisposed, Is.False);
                Assert.That(warnings, Is.Empty, "The second session was treated as a duplicate of the first.");
            }
            finally
            {
                UnityEngine.Application.logMessageReceived -= Collect;
            }
        }

        private static IEnumerator LoadBootstrapScene()
        {
            // Bootstrap is in the build scene list (ADR-569), so it can be loaded by name.
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                BootstrapSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        private static GameBootstrapper[] FindBootstrappers()
        {
            return UnityEngine.Object.FindObjectsByType<GameBootstrapper>(UnityEngine.FindObjectsSortMode.None);
        }

        private static GameBootstrapper SingleBootstrapper()
        {
            var all = FindBootstrappers();
            Assert.That(all, Has.Length.EqualTo(1), "Expected exactly one GameBootstrapper.");
            return all[0];
        }
    }
}
