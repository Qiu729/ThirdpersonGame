using System.Collections;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ThirdpersonGame.Tests
{
    public sealed class StartupSceneTests
    {
        private Scene startupScene;

        [UnityTest]
        public IEnumerator FirstBuildSceneLoads()
        {
            Assert.That(SceneManager.sceneCountInBuildSettings, Is.GreaterThan(0));

            var load = SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
            Assert.That(load, Is.Not.Null);
            yield return load;

            startupScene = SceneManager.GetSceneByBuildIndex(0);
            Assert.That(startupScene.isLoaded, Is.True);
        }

        [UnityTearDown]
        public IEnumerator UnloadStartupScene()
        {
            if (startupScene.IsValid() && startupScene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(startupScene);
            }
        }
    }
}
