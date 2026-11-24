using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ThirdpersonGame.Tests
{
    public sealed class BuildSceneTests
    {
        [Test]
        public void BuildSettingsContainEnabledSceneAssets()
        {
            var enabledScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).ToArray();

            Assert.That(enabledScenes, Is.Not.Empty);
            foreach (var scene in enabledScenes)
            {
                Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path), Is.Not.Null, scene.path);
            }
        }
    }
}
