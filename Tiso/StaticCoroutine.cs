using System.Collections;
using UnityEngine;

namespace Tiso
{
    public class StaticCoroutine : MonoBehaviour
    {
        private static StaticCoroutine _instance;

        // OnDestroy is called when the MonoBehaviour will be destroyed.
        // Coroutines are not stopped when a MonoBehaviour is disabled, but only when it is definitely destroyed.
        private void OnDestroy()
        {
            _instance.StopAllCoroutines();
        }

        // OnApplicationQuit is called on all game objects before the application is closed.
        // In the editor it is called when the user stops playmode.
        private void OnApplicationQuit()
        {
            _instance.StopAllCoroutines();
        }

        // Build will attempt to retrieve the class-wide instance, returning it when available.
        // If no instance exists, attempt to find another StaticCoroutine that exists.
        // If no StaticCoroutines are present, create a dedicated StaticCoroutine object.
        private static StaticCoroutine Build()
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = (StaticCoroutine) FindObjectOfType(typeof(StaticCoroutine));

            if (_instance != null)
            {
                return _instance;
            }

            GameObject instanceObject = new GameObject("StaticCoroutine");
            instanceObject.AddComponent<StaticCoroutine>();
            _instance = instanceObject.GetComponent<StaticCoroutine>();

            if (_instance != null)
            {
                return _instance;
            }

            Log("Build did not generate a replacement instance. Method Failed!");

            return null;
        }

        // Overloaded Static Coroutine Methods which use Unity's default Coroutines.
        // Polymorphism applied for best compatibility with the standard engine.
        /*public static void Start(string methodName)
        {
            Build().StartCoroutine(methodName);
        }

        public static void Start(string methodName, object value)
        {
            Build().StartCoroutine(methodName, value);
        }*/

        public static void Start(IEnumerator routine)
        {
            Log("Starting Coroutine");
            Build().StartCoroutine(routine);
        }

        private static void Log(object message) => Modding.Logger.Log("[Static Coroutine]: " + message);
    }
}