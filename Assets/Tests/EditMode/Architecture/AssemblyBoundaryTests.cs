using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace PastFuture.Tests.EditMode.Architecture
{
    /// <summary>
    /// Layer boundary checks for the assembly layout adopted in Handover v0.3 16-17
    /// (ADR-479 layered asmdef, ADR-480 Domain No Engine References).
    /// UNITY_T001: Domain compiles without UnityEngine.
    /// UNITY_T002: assembly dependencies have no cycles.
    /// Application is also engine-free (M0 proposal approved 2026-11-25) so that a
    /// headless simulation runner can host it.
    /// </summary>
    [Category("Architecture")]
    public sealed class AssemblyBoundaryTests
    {
        private const string Domain = "PastFuture.Domain";
        private const string Application = "PastFuture.Application";
        private const string Infrastructure = "PastFuture.Infrastructure";
        private const string UnityAdapters = "PastFuture.UnityAdapters";
        private const string Presentation = "PastFuture.Presentation";
        private const string Bootstrap = "PastFuture.Bootstrap";
        private const string EditorAssembly = "PastFuture.Editor";
        private const string EditModeTests = "PastFuture.Tests.EditMode";

        private static readonly string[] EngineFreeAssemblies = { Domain, Application };

        private static readonly string[] RuntimeAssemblies =
        {
            Domain, Application, Infrastructure, UnityAdapters, Presentation, Bootstrap
        };

        private static readonly string[] EditorOnlyAssemblies = { EditorAssembly, EditModeTests };

        // Allowed PastFuture.* references per assembly. Non-PastFuture references
        // (Unity packages) are only restricted for the engine-free assemblies.
        private static readonly Dictionary<string, HashSet<string>> AllowedProjectReferences =
            new Dictionary<string, HashSet<string>>
            {
                { Domain, new HashSet<string>() },
                { Application, new HashSet<string> { Domain } },
                { Infrastructure, new HashSet<string> { Application, Domain } },
                { UnityAdapters, new HashSet<string> { Application, Domain } },
                { Presentation, new HashSet<string> { Application, Domain, UnityAdapters } },
                { Bootstrap, new HashSet<string> { Domain, Application, Infrastructure, UnityAdapters, Presentation } },
                { EditorAssembly, new HashSet<string>(RuntimeAssemblies) },
                { EditModeTests, new HashSet<string>(RuntimeAssemblies) },
            };

        [Serializable]
        private sealed class AsmdefData
        {
            // Defaults mirror Unity's defaults when a field is absent from the file.
            public string name = string.Empty;
            public string[] references = Array.Empty<string>();
            public string[] includePlatforms = Array.Empty<string>();
            public bool autoReferenced = true;
            public bool noEngineReferences;
        }

        [TestCase(Domain)]
        [TestCase(Application)]
        public void UNITY_T001_EngineFreeAssembly_DoesNotReferenceUnity(string assemblyName)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);
            Assert.That(assembly, Is.Not.Null, $"{assemblyName} is not loaded (asmdef missing or has no scripts).");

            var unityReferences = assembly.GetReferencedAssemblies()
                .Select(r => r.Name)
                .Where(n => n.StartsWith("UnityEngine", StringComparison.Ordinal)
                            || n.StartsWith("UnityEditor", StringComparison.Ordinal)
                            || n.StartsWith("Unity.", StringComparison.Ordinal))
                .ToArray();

            Assert.That(unityReferences, Is.Empty,
                $"{assemblyName} references Unity assemblies: {string.Join(", ", unityReferences)}");
        }

        [Test]
        public void UNITY_T001_EngineFreeAsmdef_HasNoEngineReferencesFlag()
        {
            var asmdefs = LoadProjectAsmdefs();
            foreach (var name in EngineFreeAssemblies)
            {
                Assert.That(asmdefs.ContainsKey(name), Is.True, $"{name}.asmdef not found.");
                Assert.That(asmdefs[name].noEngineReferences, Is.True, $"{name} must set noEngineReferences: true.");
                Assert.That(asmdefs[name].references.All(AllowedProjectReferences[name].Contains), Is.True,
                    $"{name} may only reference [{string.Join(", ", AllowedProjectReferences[name])}] " +
                    $"but references [{string.Join(", ", asmdefs[name].references)}].");
            }
        }

        [Test]
        public void UNITY_T002_ProjectAsmdefs_FollowLayerRules()
        {
            var asmdefs = LoadProjectAsmdefs();

            var missing = AllowedProjectReferences.Keys.Where(k => !asmdefs.ContainsKey(k)).ToArray();
            Assert.That(missing, Is.Empty, $"Missing asmdef: {string.Join(", ", missing)}");

            var unexpected = asmdefs.Keys.Where(k => !AllowedProjectReferences.ContainsKey(k)).ToArray();
            Assert.That(unexpected, Is.Empty,
                $"Unregistered PastFuture asmdef (add it to the layer rules first): {string.Join(", ", unexpected)}");

            var violations = new List<string>();
            foreach (var pair in asmdefs)
            {
                var data = pair.Value;
                foreach (var reference in data.references)
                {
                    if (reference.StartsWith("GUID:", StringComparison.Ordinal))
                    {
                        violations.Add($"{pair.Key} uses GUID reference '{reference}'; use name references.");
                    }
                    else if (reference.StartsWith("PastFuture.", StringComparison.Ordinal)
                             && !AllowedProjectReferences[pair.Key].Contains(reference))
                    {
                        violations.Add($"{pair.Key} -> {reference} is not an allowed direction.");
                    }
                }

                if (RuntimeAssemblies.Contains(pair.Key) && data.autoReferenced)
                {
                    violations.Add($"{pair.Key} must set autoReferenced: false.");
                }

                if (EditorOnlyAssemblies.Contains(pair.Key)
                    && !(data.includePlatforms.Length == 1 && data.includePlatforms[0] == "Editor"))
                {
                    violations.Add($"{pair.Key} must be Editor-only.");
                }
            }

            Assert.That(violations, Is.Empty, string.Join("\n", violations));
        }

        [Test]
        public void UNITY_T002_ProjectAsmdefs_HaveNoCycles()
        {
            var asmdefs = LoadProjectAsmdefs();
            var graph = asmdefs.ToDictionary(
                p => p.Key,
                p => p.Value.references.Where(asmdefs.ContainsKey).ToArray());

            var state = new Dictionary<string, int>(); // 0 = unvisited, 1 = visiting, 2 = done
            var path = new Stack<string>();

            bool Visit(string node)
            {
                state.TryGetValue(node, out var s);
                if (s == 1) { path.Push(node); return true; }
                if (s == 2) { return false; }
                state[node] = 1;
                path.Push(node);
                foreach (var next in graph[node])
                {
                    if (Visit(next)) { return true; }
                }
                path.Pop();
                state[node] = 2;
                return false;
            }

            foreach (var node in graph.Keys)
            {
                if (Visit(node))
                {
                    Assert.Fail("Assembly cycle: " + string.Join(" <- ", path));
                }
            }
        }

        private static Dictionary<string, AsmdefData> LoadProjectAsmdefs()
        {
            // UnityEngine.Application is fully qualified: inside PastFuture.* the bare name
            // 'Application' resolves to the PastFuture.Application namespace.
            var assetsPath = UnityEngine.Application.dataPath;
            var result = new Dictionary<string, AsmdefData>();
            foreach (var file in Directory.GetFiles(assetsPath, "*.asmdef", SearchOption.AllDirectories))
            {
                var data = new AsmdefData();
                UnityEngine.JsonUtility.FromJsonOverwrite(File.ReadAllText(file), data);
                if (!data.name.StartsWith("PastFuture.", StringComparison.Ordinal))
                {
                    continue;
                }

                Assert.That(result.ContainsKey(data.name), Is.False, $"Duplicate asmdef name {data.name}");
                result[data.name] = data;
            }

            return result;
        }
    }
}
