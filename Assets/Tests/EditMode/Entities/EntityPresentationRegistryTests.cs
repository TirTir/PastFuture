using System;
using NUnit.Framework;
using PastFuture.Application.Entities;
using PastFuture.Domain.Entities;

namespace PastFuture.Tests.EditMode.Entities
{
    // EntityId and registry contract (ADR-573). UNITY_T024 is the Handover ID for duplicate bindings;
    // the other tests have no Handover ID.
    [Category("Entities")]
    public sealed class EntityPresentationRegistryTests
    {
        private static readonly EntityId A = new EntityId(1);
        private static readonly EntityId B = new EntityId(2);

        private sealed class FakePresentation : IEntityPresentation
        {
            public FakePresentation(EntityId id, string name)
            {
                EntityId = id;
                DebugName = name;
            }

            public EntityId EntityId { get; }

            public string DebugName { get; }
        }

        [Test]
        public void EntityId_ZeroOrNegative_Throws_AndNoneIsInvalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EntityId(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EntityId(-5));
            Assert.That(EntityId.None.IsValid, Is.False);
            Assert.That(new EntityId(7), Is.EqualTo(new EntityId(7)));
            Assert.That(new EntityId(7) != new EntityId(8), Is.True);
        }

        [Test]
        public void Registry_Bind_LooksUpBothWays()
        {
            var registry = new EntityPresentationRegistry();
            var view = new FakePresentation(A, "a");

            Assert.That(registry.TryBind(A, view, out var binding), Is.EqualTo(BindResult.Bound));

            Assert.That(registry.Count, Is.EqualTo(1));
            Assert.That(registry.IsCurrent(binding), Is.True);
            Assert.That(registry.TryGetPresentation(A, out var found), Is.True);
            Assert.That(found, Is.SameAs(view));
            Assert.That(registry.TryGetBinding(view, out var back), Is.True);
            Assert.That(back, Is.EqualTo(binding));
        }

        [Test]
        public void UNITY_T024_DuplicateEntityBinding_IsRejected_AndFirstKeepsAuthority()
        {
            var registry = new EntityPresentationRegistry();
            var first = new FakePresentation(A, "first");
            var second = new FakePresentation(A, "second");
            registry.TryBind(A, first, out var firstBinding);

            var result = registry.TryBind(A, second, out var secondBinding);

            Assert.That(result, Is.EqualTo(BindResult.RejectedDuplicateEntity));
            Assert.That(secondBinding.IsNone, Is.True);
            Assert.That(registry.IsCurrent(secondBinding), Is.False);
            Assert.That(registry.TryGetBinding(second, out _), Is.False);
            Assert.That(registry.TryGetPresentation(A, out var holder), Is.True);
            Assert.That(holder, Is.SameAs(first));
            Assert.That(registry.IsCurrent(firstBinding), Is.True);
        }

        [Test]
        public void Registry_SamePresentationForTwoEntities_IsRejected()
        {
            var registry = new EntityPresentationRegistry();
            var view = new FakePresentation(A, "a");
            registry.TryBind(A, view, out _);

            Assert.That(registry.TryBind(B, view, out _), Is.EqualTo(BindResult.RejectedPresentationAlreadyBound));
            Assert.That(registry.TryGetPresentation(B, out _), Is.False);
        }

        [Test]
        public void Registry_InvalidEntityId_IsRejected_AndNullPresentationThrows()
        {
            var registry = new EntityPresentationRegistry();

            Assert.That(registry.TryBind(EntityId.None, new FakePresentation(EntityId.None, "none"), out _),
                Is.EqualTo(BindResult.RejectedInvalidEntityId));
            Assert.Throws<ArgumentNullException>(() => registry.TryBind(A, null, out _));
            Assert.That(registry.Count, Is.EqualTo(0));
        }

        [Test]
        public void Registry_StaleUnbind_AfterRebind_KeepsNewBinding()
        {
            var registry = new EntityPresentationRegistry();
            var oldView = new FakePresentation(A, "old");
            var newView = new FakePresentation(A, "new");
            registry.TryBind(A, oldView, out var oldBinding);
            Assert.That(registry.Unbind(oldBinding), Is.True);
            registry.TryBind(A, newView, out var newBinding);

            // A late callback from the old view (e.g. OnDestroy after a reload) carries the old token.
            Assert.That(registry.Unbind(oldBinding), Is.False);
            Assert.That(registry.IsCurrent(oldBinding), Is.False);
            Assert.That(registry.IsCurrent(newBinding), Is.True);
            Assert.That(newBinding.BindingGeneration, Is.GreaterThan(oldBinding.BindingGeneration));
            Assert.That(registry.TryGetPresentation(A, out var holder), Is.True);
            Assert.That(holder, Is.SameAs(newView));
        }

        [Test]
        public void Registry_UnbindAll_MakesEveryTokenStale_AndAllowsRebind()
        {
            var registry = new EntityPresentationRegistry();
            var a = new FakePresentation(A, "a");
            var b = new FakePresentation(B, "b");
            registry.TryBind(A, a, out var bindingA);
            registry.TryBind(B, b, out var bindingB);

            registry.UnbindAll();

            Assert.That(registry.Count, Is.EqualTo(0));
            Assert.That(registry.IsCurrent(bindingA), Is.False);
            Assert.That(registry.IsCurrent(bindingB), Is.False);
            Assert.That(registry.TryBind(A, a, out var again), Is.EqualTo(BindResult.Bound));
            Assert.That(again.BindingGeneration, Is.GreaterThan(bindingB.BindingGeneration));
        }

        [Test]
        public void Registry_DefaultToken_IsNeverCurrent()
        {
            var registry = new EntityPresentationRegistry();
            registry.TryBind(A, new FakePresentation(A, "a"), out _);

            Assert.That(registry.IsCurrent(default), Is.False);
            Assert.That(registry.Unbind(default), Is.False);
            Assert.That(registry.Count, Is.EqualTo(1));
        }
    }
}
