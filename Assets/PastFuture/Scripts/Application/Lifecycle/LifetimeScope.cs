using System;
using System.Collections.Generic;
using System.Threading;

namespace PastFuture.Application.Lifecycle
{
    /// <summary>
    /// Owns everything created for one lifetime scope (Handover 16-1: "Scope 종료 시
    /// 구독과 비동기 작업을 해제/취소"). See ADR-569.
    /// <para>Dispose order: cancel the scope token (child tokens are linked and cancel
    /// too), dispose child scopes newest first, then dispose registered items in reverse
    /// registration order. Every item is attempted even if an earlier one throws; the
    /// failures are rethrown together as one <see cref="AggregateException"/>.</para>
    /// <para>Not thread-safe: create, register and dispose from the owning (main) thread.</para>
    /// </summary>
    public sealed class LifetimeScope : IDisposable
    {
        private readonly List<IDisposable> _items = new List<IDisposable>();
        private readonly List<LifetimeScope> _children = new List<LifetimeScope>();
        private readonly CancellationTokenSource _cancellation;
        private readonly CancellationToken _token;
        private bool _isDisposing;
        private bool _isDisposed;

        public LifetimeScope(ScopeKind kind)
            : this(kind, null)
        {
        }

        private LifetimeScope(ScopeKind kind, LifetimeScope parent)
        {
            Kind = kind;
            Parent = parent;
            _cancellation = parent == null
                ? new CancellationTokenSource()
                : CancellationTokenSource.CreateLinkedTokenSource(parent._token);
            _token = _cancellation.Token;
        }

        public ScopeKind Kind { get; }

        public LifetimeScope Parent { get; }

        public bool IsDisposed => _isDisposed;

        /// <summary>Cancelled when this scope or any ancestor is disposed.</summary>
        public CancellationToken Cancellation => _token;

        public LifetimeScope CreateChild(ScopeKind kind)
        {
            ThrowIfDisposed();
            var child = new LifetimeScope(kind, this);
            _children.Add(child);
            return child;
        }

        public T Register<T>(T item)
            where T : IDisposable
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            ThrowIfDisposed();
            _items.Add(item);
            return item;
        }

        public void Dispose()
        {
            if (_isDisposed || _isDisposing)
            {
                return;
            }

            _isDisposing = true;
            List<Exception> errors = null;

            try
            {
                _cancellation.Cancel();
            }
            catch (AggregateException e)
            {
                (errors ??= new List<Exception>()).AddRange(e.InnerExceptions);
            }

            var children = _children.ToArray();
            for (var i = children.Length - 1; i >= 0; i--)
            {
                try
                {
                    children[i].Dispose();
                }
                catch (Exception e)
                {
                    (errors ??= new List<Exception>()).Add(e);
                }
            }

            for (var i = _items.Count - 1; i >= 0; i--)
            {
                try
                {
                    _items[i].Dispose();
                }
                catch (Exception e)
                {
                    (errors ??= new List<Exception>()).Add(e);
                }
            }

            _children.Clear();
            _items.Clear();
            _cancellation.Dispose();
            _isDisposed = true;
            _isDisposing = false;
            Parent?.DetachChild(this);

            if (errors != null)
            {
                throw new AggregateException($"Errors while disposing {Kind} scope.", errors);
            }
        }

        private void DetachChild(LifetimeScope child)
        {
            if (!_isDisposing)
            {
                _children.Remove(child);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed || _isDisposing)
            {
                throw new ObjectDisposedException($"{Kind} {nameof(LifetimeScope)}");
            }
        }
    }
}
