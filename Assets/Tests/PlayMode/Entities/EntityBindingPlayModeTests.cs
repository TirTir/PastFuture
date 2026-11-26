using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PastFuture.Application.Entities;
using PastFuture.Bootstrap;
using PastFuture.Domain.Entities;
using PastFuture.UnityAdapters.Entities;
using UnityEngine.TestTools;

namespace PastFuture.Tests.PlayMode.Entities
{
    /// <summary>
    /// Entity ID ↔ GameObject binding in the M1 test room (ADR-573).
    /// UNITY_T024: a duplicate binding gets no authority. UNITY_T023: after a scene reload, tokens from
    /// the previous scene are rejected by generation (checked at the binding-token level; physics and
    /// interaction callbacks that will carry these tokens arrive in M1 steps 4-5).
    /// Other tests: Handover ID 없음.
    /// </summary>
    [Category("Entities")]
    public sealed class EntityBindingPlayModeTests
    {
        private const string TestRoomPath = "Assets/PastFuture/Scenes/Test/M1_TestRoom.unity";
        private static readonly EntityId Player = new EntityId(900001);
        private static readonly EntityId Table = new EntityId(900101);

        private SceneEntityBinder _binder;

        [UnityTearDown]
        public IEnumerator CleanUp()
        {
            _binder?.Dispose();
            _binder = null;
            foreach (var bootstrapper in UnityEngine.Object.FindObjectsByType<GameBootstrapper>(UnityEngine.FindObjectsSortMode.None))
            {
                UnityEngine.Object.Destroy(bootstrapper.gameObject);
            }

            yield return null;

            // Leave an empty scene so the next test does not start inside this test's room.
            var empty = UnityEngine.SceneManagement.SceneManager.CreateScene("EntityBindingTestsEmpty");
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(empty);
            for (var i = UnityEngine.SceneManagement.SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene != empty && scene.isLoaded)
                {
                    yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        [UnityTest]
        public IEnumerator M1TestRoom_EntityViews_AreBoundToTheirIds()
        {
            yield return LoadTestRoom();
            var registry = new EntityPresentationRegistry();
            _binder = new SceneEntityBinder(registry);

            var views = AllViews();
            Assert.That(views.Select(v => v.EntityId), Is.EquivalentTo(new[] { Player, Table }));
            Assert.That(views.All(v => v.IsBound), Is.True);
            Assert.That(registry.Count, Is.EqualTo(2));
            foreach (var view in views)
            {
                Assert.That(registry.TryGetPresentation(view.EntityId, out var found), Is.True);
                Assert.That(found, Is.SameAs(view), $"{view.EntityId} must map back to its own GameObject.");
            }

            Assert.That(((EntityView)Presentation(registry, Player)).GetComponent<UnityEngine.CharacterController>(), Is.Not.Null);
            Assert.That(((EntityView)Presentation(registry, Table)).name, Is.EqualTo("Prop_Table"));
        }

        [UnityTest]
        public IEnumerator UNITY_T024_DuplicateEntityView_IsRejected_AndOriginalKeepsAuthority()
        {
            yield return LoadTestRoom();
            var registry = new EntityPresentationRegistry();
            _binder = new SceneEntityBinder(registry);
            var original = (EntityView)Presentation(registry, Table);
            var originalBinding = original.Binding;

            var clone = UnityEngine.Object.Instantiate(original.gameObject).GetComponent<EntityView>();
            LogAssert.Expect(UnityEngine.LogType.Error, new Regex("Entity bind rejected \\(RejectedDuplicateEntity\\)"));
            var result = clone.BindTo(registry);

            Assert.That(result, Is.EqualTo(BindResult.RejectedDuplicateEntity));
            Assert.That(clone.IsBound, Is.False);
            Assert.That(clone.Binding.IsNone, Is.True);
            Assert.That(original.IsBound, Is.True);
            Assert.That(registry.IsCurrent(originalBinding), Is.True);
            Assert.That(Presentation(registry, Table), Is.SameAs(original));

            // Destroying the rejected duplicate must not unbind the original.
            UnityEngine.Object.Destroy(clone.gameObject);
            yield return null;
            Assert.That(original.IsBound, Is.True);
            Assert.That(registry.Count, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator EntityView_Destroyed_UnbindsOnlyThatEntity()
        {
            yield return LoadTestRoom();
            var registry = new EntityPresentationRegistry();
            _binder = new SceneEntityBinder(registry);
            var table = (EntityView)Presentation(registry, Table);
            var tableBinding = table.Binding;

            UnityEngine.Object.Destroy(table.gameObject);
            yield return null;

            Assert.That(registry.IsCurrent(tableBinding), Is.False);
            Assert.That(registry.TryGetPresentation(Table, out _), Is.False);
            Assert.That(registry.TryGetPresentation(Player, out _), Is.True);
        }

        [UnityTest]
        public IEnumerator UNITY_T023_SceneReload_OldBindingTokens_AreRejected()
        {
            yield return LoadTestRoom();
            var registry = new EntityPresentationRegistry();
            _binder = new SceneEntityBinder(registry);
            var oldTokens = AllViews().ToDictionary(v => v.EntityId, v => v.Binding);

            // The binder stays subscribed, so the reloaded scene binds its new views.
            yield return LoadTestRoom();
            yield return null;

            foreach (var pair in oldTokens)
            {
                Assert.That(registry.IsCurrent(pair.Value), Is.False, $"Old token {pair.Value} is still current.");
                Assert.That(registry.Unbind(pair.Value), Is.False, $"Old token {pair.Value} removed a new binding.");

                var current = (EntityView)Presentation(registry, pair.Key);
                Assert.That(current.IsBound, Is.True);
                Assert.That(current.Binding.BindingGeneration, Is.GreaterThan(pair.Value.BindingGeneration));
            }

            Assert.That(registry.Count, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator Bootstrap_ThenTestRoom_BindsViewsThroughCompositionRoot()
        {
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                "Bootstrap", UnityEngine.SceneManagement.LoadSceneMode.Single);
            var bootstrappers = UnityEngine.Object.FindObjectsByType<GameBootstrapper>(UnityEngine.FindObjectsSortMode.None);
            Assert.That(bootstrappers, Has.Length.EqualTo(1));
            var registry = bootstrappers[0].Root.EntityPresentations;
            Assert.That(registry.Count, Is.EqualTo(0));

            yield return LoadTestRoom();

            Assert.That(registry.Count, Is.EqualTo(2));
            Assert.That(Presentation(registry, Player), Is.SameAs(AllViews().Single(v => v.EntityId == Player)));
        }

        private static IEnumerator LoadTestRoom()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                TestRoomPath,
                new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
#else
            Assert.Ignore("The M1 test room is not in the build scene list; Editor only.");
            yield break;
#endif
        }

        private static List<EntityView> AllViews()
        {
            return UnityEngine.Object.FindObjectsByType<EntityView>(UnityEngine.FindObjectsSortMode.None).ToList();
        }

        private static IEntityPresentation Presentation(IEntityPresentationRegistry registry, EntityId id)
        {
            Assert.That(registry.TryGetPresentation(id, out var presentation), Is.True, $"{id} is not bound.");
            return presentation;
        }
    }
}
