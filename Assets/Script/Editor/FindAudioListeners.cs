using UnityEngine;
using UnityEditor;
using System.Linq;

public class FindAudioListeners : EditorWindow {
    [MenuItem("Tools/Find AudioListeners in Scene")]
    static void ShowWindow() {
        var listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"Found {listeners.Length} AudioListener(s) in scene:");

        foreach (var al in listeners) {
            if (al != null && al.gameObject != null) {
                Debug.Log($"¡ú {al.gameObject.name} (active: {al.gameObject.activeInHierarchy})", al.gameObject);
            }
        }

        if (listeners.Length == 0) {
            Debug.LogWarning("No AudioListener found in the current scene.");
        }
    }
}