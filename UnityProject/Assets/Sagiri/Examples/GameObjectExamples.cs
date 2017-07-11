using Assets.Sagiri;
using System.Reflection;
using UnityEngine;

/**
 * Example console commands for getting information about GameObjects
 */
public static class GameObjectCommands {

    [Assets.Sagiri.Command("object list", "lists all the game objects in the scene")]
    public static void ListGameObjects() {
        UnityEngine.Object[] objects = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
        foreach (UnityEngine.Object obj in objects) {
            Assets.Sagiri.Console.Log(obj.name);
        }
    }

    [Assets.Sagiri.Command("object print", "lists properties of the object")]
    public static void PrintGameObject(string[] args) {
        if (args.Length < 1) {
            Assets.Sagiri.Console.Log("expected : object print <Object Name>");
            return;
        }

        GameObject obj = GameObject.Find(args[0]);
        if (obj == null) {
            Assets.Sagiri.Console.Log("GameObject not found : " + args[0]);
        } else {
            Assets.Sagiri.Console.Log("Game Object : " + obj.name);
            foreach (Component component in obj.GetComponents(typeof(Component))) {
                Assets.Sagiri.Console.Log("  Component : " + component.GetType());
                foreach (FieldInfo f in component.GetType().GetFields()) {
                    Assets.Sagiri.Console.Log("    " + f.Name + " : " + f.GetValue(component));
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

    [Assets.Sagiri.Route("^/object/list.json$", @"(GET|HEAD)", true)]
    public static void ListGameObjects(Assets.Sagiri.RequestContext context) {
        string json = "[";
        UnityEngine.Object[] objects = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
        foreach (UnityEngine.Object obj in objects) {
            // FIXME object names need to be escaped.. use minijson or similar
            json += string.Format("\"{0}\", ", obj.name);
        }
        json = json.TrimEnd(new char[] { ',', ' ' }) + "]";

        context.Response.WriteString(json, "application/json");
    }
}
