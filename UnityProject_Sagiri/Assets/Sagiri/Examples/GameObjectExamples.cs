using System.Reflection;
using UnityEngine;

namespace Assets.Sagiri.Examples {
    /**
     * Example console commands for getting information about GameObjects
     */
    public static class GameObjectCommands {

        [Command("object list", "lists all the game objects in the scene")]
        public static void ListGameObjects() {
            Object[] objects = Object.FindObjectsOfType(typeof(GameObject));
            foreach (Object obj in objects) {
                Shell.Log(obj.name);
            }
        }

        [Command("object print", "lists properties of the object")]
        public static void PrintGameObject(string[] args) {
            if (args.Length < 1) {
                Shell.Log("expected : object print <Object Name>");
                return;
            }

            GameObject obj = GameObject.Find(args[0]);
            if (obj == null) {
                Shell.Log("GameObject not found : " + args[0]);
            } else {
                Shell.Log("Game Object : " + obj.name);
                foreach (Component component in obj.GetComponents(typeof(Component))) {
                    Shell.Log("  Component : " + component.GetType());
                    foreach (FieldInfo f in component.GetType().GetFields()) {
                        Shell.Log("    " + f.Name + " : " + f.GetValue(component));
                    }
                }
            }
        }
    }



    /**
     * Example console route for getting information about GameObjects
     *
     */
    public static class GameObjectRoutes {
        [Route("^/object/list.json$", @"(GET|HEAD)", true)]
        public static void ListGameObjects(RequestContext context) {
#if !NETFX_CORE
            string json = "[";
            Object[] objects = Object.FindObjectsOfType(typeof(GameObject));
            foreach (Object obj in objects) {
                // FIXME object names need to be escaped.. use minijson or similar
                json += string.Format("\"{0}\", ", obj.name);
            }
            json = json.TrimEnd(new char[] { ',', ' ' }) + "]";

            context.Response.WriteString(json, "application/json");
#endif
        }
    }
}