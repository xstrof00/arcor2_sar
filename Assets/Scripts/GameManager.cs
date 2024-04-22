using Base;
using IO.Swagger.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Base.Singleton<GameManager>
{
    private List<ActionPoint> actionPoints = new List<ActionPoint>();
    private List<SceneObject> sceneObjects = new List<SceneObject>();
    internal PackageInfoData packageInfo;

    private enum ScreenStateEnum
    {
        MainScreen,
        EditingScene,
        EditingProject,
        RunningPackage,
        PausingPackage,
        PausedPackage
    }

    // Start is called before the first frame update
    void Start()
    {
        WebsocketManager.Instance.OnActionPointAdded += ApAdd;
        WebsocketManager.Instance.OnActionPointBaseUpdated += ApChangeUpdateBase;
        WebsocketManager.Instance.OnActionPointRemoved += ApRemove;

        WebsocketManager.Instance.OnObjectTypeUpdated += ObjectTypeUpdate;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ConnectToServer()
    {
        WebsocketManager.Instance.ConnectToServer("192.168.104.100", 6789);
    }

    public void ShowMainScreen()
    {
        PrintStateInfoText(ScreenStateEnum.MainScreen);
    }

    public void OpenScene(Scene scene)
    {
        PrintStateInfoText(ScreenStateEnum.EditingScene);
        SpawnSceneInGame(scene);
    }

    public void OpenProject(Scene scene, Project project)
    {
        PrintStateInfoText(ScreenStateEnum.EditingProject);
        SpawnProjectInGame(scene, project);
    }


    private void SpawnSceneInGame(Scene scene)
    {
        foreach (var sceneObject in scene.Objects)
        {
            sceneObjects.Add(sceneObject);
            AddSceneObjectToGame(sceneObject);
        }
    }

    private void SpawnProjectInGame(Scene scene, Project project)
    {
        foreach (var sceneObject in scene.Objects)
        {
            if (scene.Objects.Count != 0)
            {
                sceneObjects.Add(sceneObject);
                AddSceneObjectToGame(sceneObject);
            }
        }

        foreach (var ap in project.ActionPoints)
        {
            if (project.ActionPoints.Count != 0)
            {
                actionPoints.Add(ap);
                AddActionPointToGame(ap);
                SpawnActionsInGame(ap.Actions, ap.Id);
            }
        }
    }

    public void PackageStateUpdated(PackageStateData packageStateData)
    {
        switch (packageStateData.State)
        {
            case PackageStateData.StateEnum.Running:
                PrintStateInfoText(ScreenStateEnum.RunningPackage);
                break;

            case PackageStateData.StateEnum.Pausing:
                PrintStateInfoText(ScreenStateEnum.PausingPackage);
                break;

            case PackageStateData.StateEnum.Paused:
                PrintStateInfoText(ScreenStateEnum.PausedPackage);
                break;

            case PackageStateData.StateEnum.Stopped:
                StopPackage();
                break;
        }
    }

    public void PackageInfoUpdated()
    {
        Project project = packageInfo.Project;
        Scene scene = packageInfo.Scene;
        SpawnProjectInGame(scene, project);
    }

    private void PrintStateInfoText(ScreenStateEnum state)
    {
        GameObject stateInfo = GameObject.Find("StateInfoText");
        GameObject smallInfo = GameObject.Find("SmallInfoText");

        if (stateInfo == null)
        {
            stateInfo = Instantiate(Resources.Load("StateInfoText") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
            stateInfo.name = "StateInfoText";
        }
        TMP_Text stateInfoText = stateInfo.GetComponent<TMP_Text>();

        if (smallInfo == null)
        {
            smallInfo = Instantiate(Resources.Load("SmallInfoText") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
            smallInfo.name = "SmallInfoText";
        }
        TMP_Text smallInfoText = smallInfo.GetComponent<TMP_Text>();
        smallInfoText.text = "";

        switch (state)
        {
            case ScreenStateEnum.MainScreen:
                stateInfoText.text = "Main screen";
                stateInfoText.color = new Color32(255, 255, 255, 255);
                break;

            case ScreenStateEnum.EditingScene:
                stateInfoText.text = "Editing scene";
                stateInfoText.color = new Color32(0, 150, 255, 255);
                break;

            case ScreenStateEnum.EditingProject:
                stateInfoText.text = "Editing project";
                stateInfoText.color = new Color32(255, 0, 255, 255);
                break;

            case ScreenStateEnum.RunningPackage:
                stateInfoText.text = "Running program";
                stateInfoText.color = new Color32(0, 255, 0, 255);
                break;

            case ScreenStateEnum.PausingPackage:
                stateInfoText.text = "Pausing program";
                stateInfoText.color = new Color32(255, 0, 100, 255);

                smallInfoText.text = "Please wait, robot is finishing action";
                smallInfoText.color = new Color32(255, 255, 255, 255);
                break;

            case ScreenStateEnum.PausedPackage:
                stateInfoText.text = "Paused";
                stateInfoText.color = new Color32(255, 0, 0, 255);
                break;

            default:
                if (smallInfo != null)
                {
                    Destroy(smallInfo.gameObject);
                }
                break;
        }
    }

    public void CloseScene()
    {
        DestroyObjectsInGame();
        sceneObjects.Clear();
    }

    public void CloseProject()
    {
        DestroyObjectsInGame();
        actionPoints.Clear();
    }

    private void StopPackage()
    {
        DestroyObjectsInGame();
        actionPoints.Clear();
    }

    private void ApAdd(object sender, ProjectActionPointEventArgs args)
    {
        ActionPoint newAp = args.ActionPoint;
        actionPoints.Add(newAp);
        AddActionPointToGame(newAp);
    }

    private void ApChangeUpdateBase(object sender, BareActionPointEventArgs args)
    {
        ActionPoint updatedAp = actionPoints.Find(x => x.Id == args.ActionPoint.Id);
        updatedAp.Position = args.ActionPoint.Position;
        UpdateActionPointPoistionInGame(args.ActionPoint.Id);
    }

    private void ApRemove(object sender, StringEventArgs args)
    {
        ActionPoint removedAp = actionPoints.Find(x => x.Id == args.Data);
        actionPoints.Remove(removedAp);
        DestroyObjectInGame(args.Data);
    }

    private void ObjectTypeUpdate(object sender, ObjectTypesEventArgs args)
    {
        foreach (var objectType in args.ObjectTypes)
        {
            UpdateSceneObjectDimensions(objectType);
        }
    }

    private void UpdateSceneObjectDimensions(ObjectTypeMeta objectType)
    {
        SceneObject sceneObject = sceneObjects.Find(x => x.Type == objectType.Type);
        GameObject sceneObjectInGame = null;
        if (sceneObject != null)
        {
            sceneObjectInGame = GameObject.Find(sceneObject.Id);
        }

        if(sceneObjectInGame != null)
        {
            Image image = sceneObjectInGame.GetComponent<Image>();
            Vector2 dimensions = new Vector2();
            switch (objectType.ObjectModel.Type)
            {
                case ObjectModel.TypeEnum.Sphere:
                    dimensions = new Vector2((float)objectType.ObjectModel.Sphere.Radius * 10, (float)objectType.ObjectModel.Sphere.Radius * 10);
                    break;

                case ObjectModel.TypeEnum.Cylinder:
                    dimensions = new Vector2((float)objectType.ObjectModel.Cylinder.Radius * 10, (float)objectType.ObjectModel.Cylinder.Radius * 10);
                    break;

                case ObjectModel.TypeEnum.Box:
                    dimensions = new Vector2((float)objectType.ObjectModel.Box.SizeX * 10, (float)objectType.ObjectModel.Box.SizeY * 10);
                    break;
            }
            image.rectTransform.localScale = dimensions;
        }
    }

    public void ActionAdded(IO.Swagger.Model.Action action, string parentId)
    {
        ActionPoint parentActionPoint = actionPoints.FirstOrDefault(x => x.Id == parentId);

        if (parentActionPoint == null)
        {
            Debug.LogError("Parent action point with id " + parentId + " not found");
            return;
        }

        if (parentActionPoint.Actions == null)
        {
            parentActionPoint.Actions = new List<IO.Swagger.Model.Action>();
        }

        AddActionToGame(action, parentId);
        parentActionPoint.Actions.Add(action);
    }

    public void ActionRemoved(BareAction action)
    {
        ActionPoint parentActionPoint = actionPoints.FirstOrDefault(x => x.Actions.Any(y => y.Id == action.Id));

        if (parentActionPoint != null)
        {
            IO.Swagger.Model.Action removedAction = parentActionPoint.Actions.FirstOrDefault(x => x.Id == action.Id);
            parentActionPoint.Actions.Remove(removedAction);
        }

        DestroyObjectInGame(action.Id);
    }

    private void AddActionPointToGame(ActionPoint ap)
    {
        Vector3 apPosition = AREditorToSARPosition(ap.Position);
        GameObject parent = GameObject.FindGameObjectWithTag("Canvas");
        GameObject newActionPoint;
        if (!string.IsNullOrEmpty(ap.Parent))
        {
            parent = GameObject.Find(ap.Parent);
            newActionPoint = Instantiate(Resources.Load("ActionPointPrefab") as GameObject, Vector3.zero, Quaternion.identity, parent.transform);
            newActionPoint.transform.localPosition = apPosition;
        }
        else
        {
            newActionPoint = Instantiate(Resources.Load("ActionPointPrefab") as GameObject, apPosition, Quaternion.identity, parent.transform);
        }
        newActionPoint.name = ap.Id;
        newActionPoint.GetComponent<Image>().color = new Color32(70, 0, 255, 255);
    }

    private Position addTwoPositions(Position position1, Position position2)
    {
        Position addedPosition = new Position(position1.X + position2.X, position1.Y + position2.Y, position1.Z + position2.Z);
        return addedPosition;
    }

    private void AddActionToGame(IO.Swagger.Model.Action action, string parentId)
    {
        GameObject parentActionPointInScene = GameObject.Find(parentId);

        ActionPoint parentActionPoint = actionPoints.Find(x => x.Id.Equals(parentId));

        Debug.LogError("Parent action point ID: " + parentActionPoint.Id);

        float moveActionByNumberOfExisting = 0.0f;

        foreach (var foundAction in parentActionPoint.Actions)
        {
            moveActionByNumberOfExisting += 0.15f;
        }

        GameObject newAction = Instantiate(Resources.Load("ActionText") as GameObject,
                new Vector3(parentActionPointInScene.transform.position.x, parentActionPointInScene.transform.position.y + moveActionByNumberOfExisting,
                parentActionPointInScene.transform.position.z), Quaternion.identity, parentActionPointInScene.transform);
        newAction.transform.Rotate(0f, 0f, 180f);
        newAction.name = action.Id;

        TMP_Text actionText = newAction.GetComponent<TMP_Text>();
        actionText.text = action.Name;
    }

    private void SpawnActionsInGame(List<IO.Swagger.Model.Action> actions, string parentId)
    {
        GameObject parentActionPointInScene = GameObject.Find(parentId);

        ActionPoint parentActionPoint = actionPoints.Find(x => x.Id.Equals(parentId));

        float moveActionByNumberOfExisting = 0.0f;

        foreach (var foundAction in actions)
        {
            GameObject newAction = Instantiate(Resources.Load("ActionText") as GameObject,
                new Vector3(parentActionPointInScene.transform.position.x, parentActionPointInScene.transform.position.y + moveActionByNumberOfExisting,
                parentActionPointInScene.transform.position.z), Quaternion.identity, parentActionPointInScene.transform);
            newAction.transform.Rotate(0f, 0f, 180f);
            newAction.name = foundAction.Id;

            TMP_Text actionText = newAction.GetComponent<TMP_Text>();
            actionText.text = foundAction.Name;
            moveActionByNumberOfExisting += 0.15f;
        }
    }

    private void UpdateActionPointPoistionInGame(string id)
    {
        ActionPoint actionPoint = actionPoints.Find(x => x.Id == id);
        GameObject apInScene = GameObject.Find(id);

        if (!string.IsNullOrEmpty(actionPoint.Parent))
        {
            apInScene.transform.localPosition = AREditorToSARPosition(actionPoint.Position);
        }
        else
        {
            apInScene.transform.position = AREditorToSARPosition(actionPoint.Position);
        }
    }

    public void SceneObjectAdded(SceneObject sceneObject)
    {
        sceneObjects.Add(sceneObject);
        AddSceneObjectToGame(sceneObject);
    }

    public void SceneObjectRemoved(SceneObject sceneObject)
    {
        sceneObjects.Remove(sceneObject);
        DestroyObjectInGame(sceneObject.Id);
    }

    public void SceneObjectUpdated(SceneObject sceneObject)
    {
        SceneObject updatedSceneObject = sceneObjects.Find(x => x.Id == sceneObject.Id);
        updatedSceneObject = sceneObject;
        UpdateSceneObjectInGame(sceneObject);
    }

    private void AddSceneObjectToGame(SceneObject sceneObject)
    {
        if (sceneObject != null)
        {
            string[] parts = sceneObject.Type.Split("_");
            string objectTypeWithoutNumber = parts[0];
            GameObject addedGameObject = null;

            switch (objectTypeWithoutNumber)
            {
                case "DobotMagician":
                    addedGameObject = Instantiate(Resources.Load("DobotMagician") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
                    addedGameObject.GetComponent<Image>().color = new Color32(50, 255, 0, 255);
                    ShowDobotMagicianRange(addedGameObject);
                    break;

                case "sphere":
                    addedGameObject = Instantiate(Resources.Load("Sphere") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
                    addedGameObject.GetComponent<Image>().color = new Color32(255, 228, 0, 255);
                    break;

                case "cylinder":
                    addedGameObject = Instantiate(Resources.Load("Sphere") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
                    addedGameObject.GetComponent<Image>().color = new Color32(255, 228, 0, 255);
                    break;

                case "cube":
                    addedGameObject = Instantiate(Resources.Load("Cube") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
                    addedGameObject.GetComponent<Image>().color = new Color32(255, 228, 0, 255);
                    break;
            }
            
            addedGameObject.name = sceneObject.Id;
            SetSceneObjectPose(sceneObject);
        }
    }

    private void UpdateSceneObjectInGame(SceneObject sceneObject)
    {
        SetSceneObjectPose(sceneObject);
    }

    private void DestroyObjectInGame(string id)
    {
        GameObject removedPrefabObject = GameObject.Find(id);
        Destroy(removedPrefabObject.gameObject);
    }

    private void DestroyObjectsInGame()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Prefab");
        foreach (var gameObject in gameObjects)
        {
            Destroy(gameObject.gameObject);
        }
    }

    private void ShowDobotMagicianRange(GameObject dobotMagician)
    {
        GameObject dobotMagicianRange = Instantiate(Resources.Load("DobotMagicianRange") as GameObject, dobotMagician.transform);
        DrawCircle(dobotMagicianRange, 3.2f, 0.01f);
        dobotMagicianRange.transform.Rotate(90f, 0, 0);
    }

    private Vector3 AREditorToSARPosition(Position position)
    {
        Vector3 convertedPosition = new Vector3();
        convertedPosition.Set(-1 * 10 * (float)position.X, -1 * 10 * (float)position.Y, 0.0f);
        return convertedPosition;
    }

    private Quaternion AREditorToSAROrientation(Orientation orientation)
    {
        Quaternion convertedOrientation = new Quaternion();
        convertedOrientation.Set((float)orientation.X, (float)orientation.Y, (float)orientation.Z, (float)orientation.W);
        return convertedOrientation;
    }

    private void SetSceneObjectPose(SceneObject sceneObject)
    {
        GameObject sceneObjectInGame = GameObject.Find(sceneObject.Id);
        sceneObjectInGame.transform.rotation = AREditorToSAROrientation(sceneObject.Pose.Orientation);
        sceneObjectInGame.transform.position = AREditorToSARPosition(sceneObject.Pose.Position);
    }

    //Following code in this function was adapted from: https://www.loekvandenouweland.com/content/use-linerenderer-in-unity-to-draw-a-circle.html
    private void DrawCircle(GameObject container, float radius, float lineWidth)
    {
        var segments = 360;
        var line = container.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = segments + 1;

        var pointCount = segments + 1;
        var points = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            var rad = Mathf.Deg2Rad * (i * 360f / segments);
            points[i] = new Vector3(Mathf.Sin(rad) * radius, 0, Mathf.Cos(rad) * radius);
        }

        line.SetPositions(points);
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color32(50, 255, 0, 255);
        line.endColor = new Color32(50, 255, 0, 255);
    }
}
