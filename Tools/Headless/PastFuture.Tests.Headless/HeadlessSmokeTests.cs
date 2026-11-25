using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PastFuture.Application.Lifecycle;

namespace PastFuture.Tests.Headless
{
    /// <summary>
    /// Pure .NET headless smoke (Handover 4 M0 "순수 C# smoke", 16-18; ADR-570).
    /// Builds the Unity Domain/Application sources outside Unity and runs them with no
    /// engine present. Handover has no Test ID for these, so names carry no ID prefix.
    /// </summary>
    [Category("HeadlessSmoke")]
    public sealed class HeadlessSmokeTests
    {
        private static readonly Assembly[] EngineFreeAssemblies =
        {
            typeof(ScopeKind).Assembly,
            Assembly.Load("PastFuture.Domain"),
        };

        [Test]
        public void EngineFreeAssemblies_LoadWithoutUnity_AndReferenceNoUnityAssembly()
        {
            foreach (var assembly in EngineFreeAssemblies)
            {
                var unityReferences = assembly.GetReferencedAssemblies()
                    .Select(r => r.Name)
                    .Where(n => n.StartsWith("Unity", StringComparison.Ordinal))
                    .ToArray();

                Assert.That(unityReferences, Is.Empty, assembly.GetName().Name);
            }

            Assert.That(EngineFreeAssemblies.Select(a => a.GetName().Name),
                Is.EquivalentTo(new[] { "PastFuture.Application", "PastFuture.Domain" }));
        }

        [Test]
        public void LifetimeScope_RunsHeadless_DisposesChildThenItemsAndCancels()
        {
            var log = new List<string>();
            var session = new LifetimeScope(ScopeKind.Session);
            session.Register(new LoggingDisposable("session-item", log));
            var loop = session.CreateChild(ScopeKind.Loop);
            loop.Register(new LoggingDisposable("loop-item", log));
            var loopToken = loop.Cancellation;

            session.Dispose();

            Assert.That(log, Is.EqualTo(new[] { "loop-item", "session-item" }));
            Assert.That(loopToken.IsCancellationRequested, Is.True);
            Assert.That(session.IsDisposed && loop.IsDisposed, Is.True);
        }

        private sealed class LoggingDisposable : IDisposable
        {
            private readonly string _name;
            private readonly List<string> _log;

            public LoggingDisposable(string name, List<string> log)
            {
                _name = name;
                _log = log;
            }

            public void Dispose()
            {
                _log.Add(_name);
            }
        }
    }
}
