using System;
using System.Collections.Generic;
using NUnit.Framework;
using PastFuture.Application.Lifecycle;
using PastFuture.Bootstrap;

namespace PastFuture.Tests.EditMode.Lifecycle
{
    /// <summary>
    /// LifetimeScope / CompositionRoot contract (ADR-569). Handover has no Test ID for
    /// these (16-1 detailed test IDs were not recovered), so names carry no ID prefix.
    /// </summary>
    [Category("Lifecycle")]
    public sealed class LifetimeScopeTests
    {
        private sealed class RecordingDisposable : IDisposable
        {
            private readonly string _name;
            private readonly List<string> _log;
            private readonly bool _throwOnDispose;

            public RecordingDisposable(string name, List<string> log, bool throwOnDispose = false)
            {
                _name = name;
                _log = log;
                _throwOnDispose = throwOnDispose;
            }

            public int DisposeCount { get; private set; }

            public void Dispose()
            {
                DisposeCount++;
                _log.Add(_name);
                if (_throwOnDispose)
                {
                    throw new InvalidOperationException($"{_name} failed");
                }
            }
        }

        [Test]
        public void LifetimeScope_DisposesRegisteredItemsInReverseOrder()
        {
            var log = new List<string>();
            var scope = new LifetimeScope(ScopeKind.Application);
            scope.Register(new RecordingDisposable("first", log));
            scope.Register(new RecordingDisposable("second", log));
            scope.Register(new RecordingDisposable("third", log));

            scope.Dispose();

            Assert.That(log, Is.EqualTo(new[] { "third", "second", "first" }));
            Assert.That(scope.IsDisposed, Is.True);
        }

        [Test]
        public void LifetimeScope_DisposesChildrenBeforeOwnItems()
        {
            var log = new List<string>();
            var session = new LifetimeScope(ScopeKind.Session);
            session.Register(new RecordingDisposable("session-item", log));
            var loop = session.CreateChild(ScopeKind.Loop);
            loop.Register(new RecordingDisposable("loop-item", log));
            var scene = loop.CreateChild(ScopeKind.Scene);
            scene.Register(new RecordingDisposable("scene-item", log));

            session.Dispose();

            Assert.That(log, Is.EqualTo(new[] { "scene-item", "loop-item", "session-item" }));
            Assert.That(loop.IsDisposed, Is.True);
            Assert.That(scene.IsDisposed, Is.True);
            Assert.That(scene.Parent, Is.SameAs(loop));
            Assert.That(loop.Kind, Is.EqualTo(ScopeKind.Loop));
        }

        [Test]
        public void LifetimeScope_CancelsOwnAndChildTokensOnDispose()
        {
            var parent = new LifetimeScope(ScopeKind.Session);
            var child = parent.CreateChild(ScopeKind.Loop);
            var parentToken = parent.Cancellation;
            var childToken = child.Cancellation;
            var childCancelledBeforeItsItems = false;
            child.Register(new ActionDisposable(() => childCancelledBeforeItsItems = childToken.IsCancellationRequested));

            Assert.That(parentToken.IsCancellationRequested, Is.False);
            Assert.That(childToken.IsCancellationRequested, Is.False);

            parent.Dispose();

            Assert.That(parentToken.IsCancellationRequested, Is.True);
            Assert.That(childToken.IsCancellationRequested, Is.True);
            Assert.That(childCancelledBeforeItsItems, Is.True);
        }

        [Test]
        public void LifetimeScope_DisposingChild_DoesNotCancelParent_AndDetachesChild()
        {
            var log = new List<string>();
            var parent = new LifetimeScope(ScopeKind.Session);
            var child = parent.CreateChild(ScopeKind.Loop);
            var childItem = child.Register(new RecordingDisposable("loop-item", log));

            child.Dispose();
            Assert.That(parent.Cancellation.IsCancellationRequested, Is.False);
            Assert.That(parent.IsDisposed, Is.False);

            parent.Dispose();
            Assert.That(childItem.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void LifetimeScope_DisposeTwice_DisposesItemsOnce()
        {
            var log = new List<string>();
            var scope = new LifetimeScope(ScopeKind.Scene);
            var item = scope.Register(new RecordingDisposable("item", log));

            scope.Dispose();
            scope.Dispose();

            Assert.That(item.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void LifetimeScope_RegisterOrCreateChildAfterDispose_Throws()
        {
            var scope = new LifetimeScope(ScopeKind.Loop);
            scope.Dispose();

            Assert.Throws<ObjectDisposedException>(() => scope.Register(new RecordingDisposable("late", new List<string>())));
            Assert.Throws<ObjectDisposedException>(() => scope.CreateChild(ScopeKind.Scene));
        }

        [Test]
        public void LifetimeScope_RegisterNull_Throws()
        {
            var scope = new LifetimeScope(ScopeKind.Application);
            Assert.Throws<ArgumentNullException>(() => scope.Register<IDisposable>(null));
        }

        [Test]
        public void LifetimeScope_ThrowingItem_StillDisposesOthers_AndAggregatesErrors()
        {
            var log = new List<string>();
            var scope = new LifetimeScope(ScopeKind.Loop);
            scope.Register(new RecordingDisposable("a", log, throwOnDispose: true));
            scope.Register(new RecordingDisposable("b", log));
            scope.Register(new RecordingDisposable("c", log, throwOnDispose: true));

            var error = Assert.Throws<AggregateException>(() => scope.Dispose());

            Assert.That(log, Is.EqualTo(new[] { "c", "b", "a" }));
            Assert.That(error.InnerExceptions.Count, Is.EqualTo(2));
            Assert.That(scope.IsDisposed, Is.True);
        }

        [Test]
        public void CompositionRoot_CreatesApplicationScope_AndDisposesIt()
        {
            var root = new CompositionRoot();
            var token = root.ApplicationScope.Cancellation;

            Assert.That(root.ApplicationScope.Kind, Is.EqualTo(ScopeKind.Application));
            Assert.That(root.ApplicationScope.Parent, Is.Null);
            Assert.That(root.ApplicationScope.IsDisposed, Is.False);

            root.Dispose();

            Assert.That(root.ApplicationScope.IsDisposed, Is.True);
            Assert.That(token.IsCancellationRequested, Is.True);
        }

        private sealed class ActionDisposable : IDisposable
        {
            private readonly Action _onDispose;

            public ActionDisposable(Action onDispose)
            {
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                _onDispose();
            }
        }
    }
}
