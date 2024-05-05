//author: Jakub Štrof

using Base;
using IO.Swagger.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Base.Singleton<GameManager>
{
    private List<ActionPoint> actionPoints = new List<ActionPoint>();
    private List<SceneObject> sceneObjects = new List<SceneObject>();
    internal PackageInfoData packageInfo;
    private List<ObjectTypeMeta> objectTypes = new List<ObjectTypeMeta>();
    
    private enum ScreenStateEnum
    {
        MainScreen,
        EditingScene,
        EditingProject,
        RunningPackage,
        PausingPackage,
        PausedPackage,
        StoppingPackage,
        StoppedPackage
    }

    // Start is called before the first frame update
    void Start()
    {
        WebsocketManager.Instance.OnActionPointAdded += ApAdd;
        WebsocketManager.Instance.OnActionPointBaseUpdated += ApChangeUpdateBase;
        WebsocketManager.Instance.OnActionPointRemoved += ApRemove;

        WebsocketManager.Instance.OnObjectTypeAdded += AddedObjectType;
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
        ShowInfoTextInGame(ScreenStateEnum.MainScreen);
    }

    public void OpenScene(Scene scene)
    {
        ShowInfoTextInGame(ScreenStateEnum.EditingScene);
        ShowSceneOrProjectNameInGame(scene.Name);
        SpawnSceneInGame(scene);
    }

    public void OpenProject(Scene scene, Project project)
    {
        DestroyObjectsInGame();
        sceneObjects.Clear();
        ShowInfoTextInGame(ScreenStateEnum.EditingProject);
        ShowSceneOrProjectNameInGame(project.Name);
        SpawnProjectInGame(scene, project);
    }

    private async void SpawnSceneInGame(Scene scene)
    {
        objectTypes = await WebsocketManager.Instance.GetObjectTypes();
        

        foreach (var sceneObject in scene.Objects)
        {
            sceneObjects.Add(sceneObject);
            AddSceneObjectToGame(sceneObject);
        }
    }

    private void SpawnProjectInGame(Scene scene, Project project)
    {
        SpawnSceneInGame(scene);

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
                ShowInfoTextInGame(ScreenStateEnum.RunningPackage);
                break;

            case PackageStateData.StateEnum.Pausing:
                ShowInfoTextInGame(ScreenStateEnum.PausingPackage);
                break;

            case PackageStateData.StateEnum.Paused:
                ShowInfoTextInGame(ScreenStateEnum.PausedPackage);
                break;

            case PackageStateData.StateEnum.Stopping:
                ShowInfoTextInGame(ScreenStateEnum.StoppingPackage);
                break;

            case PackageStateData.StateEnum.Stopped:
                StopPackage();
                break;
        }
    }

    public void PackageInfoUpdated()
    {
        ShowPackageNameInGame();

        foreach (var ap in packageInfo.Project.ActionPoints)
        {
            if (packageInfo.Project.ActionPoints.Count != 0)
            {
                actionPoints.Add(ap);
            }
        }

        foreach(var sceneObject in packageInfo.Scene.Objects)
        {
            sceneObjects.Add(sceneObject);
        }
    }

    private void ShowPackageNameInGame()
    {
        GameObject packageName = Instantiate(Resources.Load("OpenedInstanceNameText") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
        packageName.transform.position = new Vector3(-4.4f, -1.3f, 0f);
        packageName.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        TMP_Text packageNameText = packageName.GetComponent<TMP_Text>();

        packageNameText.text = packageInfo.PackageName;
        packageNameText.color = UnityEngine.Color.white;
    }

    private void ShowSceneOrProjectNameInGame(string name)
    {
        GameObject instanceName = Instantiate(Resources.Load("OpenedInstanceNameText") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
        instanceName.transform.position = new Vector3(-4.4f, -1.3f, 0f);
        instanceName.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        TMP_Text instanceNameText = instanceName.GetComponent<TMP_Text>();

        instanceNameText.text = name;
        instanceNameText.color = UnityEngine.Color.white;
    }

    private void ShowInfoTextInGame(ScreenStateEnum state)
    {
        GameObject stateInfo = GameObject.Find("StateInfoText");
        GameObject smallInfo = GameObject.Find("SmallInfoText");

        if (stateInfo == null)
        {
            stateInfo = Instantiate(Resources.Load("StateInfoText") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
            stateInfo.name = "StateInfoText";
            stateInfo.transform.position = new Vector3(-4.4f, -1f, 0f);
            stateInfo.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }
        TMP_Text stateInfoText = stateInfo.GetComponent<TMP_Text>();

        if (smallInfo == null)
        {
            smallInfo = Instantiate(Resources.Load("SmallInfoText") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
            smallInfo.name = "SmallInfoText";
            smallInfo.transform.position = new Vector3(-4.4f, -0.7f, 0f);
            smallInfo.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }
        TMP_Text smallInfoText = smallInfo.GetComponent<TMP_Text>();
        smallInfoText.text = "";

        switch (state)
        {
            case ScreenStateEnum.MainScreen:
                stateInfoText.text = "Stand-by";
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

                smallInfoText.text = "Please wait for the action to finish";
                smallInfoText.color = new Color32(255, 255, 255, 255);
                break;

            case ScreenStateEnum.PausedPackage:
                stateInfoText.text = "Paused";
                stateInfoText.color = new Color32(255, 0, 0, 255);
                break;

            case ScreenStateEnum.StoppingPackage:
                stateInfoText.text = "Stopping program";
                stateInfoText.color = new Color32(255, 0, 100, 255);

                smallInfoText.text = "Please wait for the action to finish";
                smallInfoText.color = new Color32(255, 255, 255, 255);
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
        UpdateActionPointPositionInGame(args.ActionPoint.Id);
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

    public void ActionBaseUpdated(BareAction action)
    {
        ActionPoint parentActionPoint = actionPoints.FirstOrDefault(x => x.Actions.Any(y => y.Id == action.Id));

        if (parentActionPoint != null)
        {
            IO.Swagger.Model.Action updatedBaseAction = parentActionPoint.Actions.FirstOrDefault(x => x.Id == action.Id);
            updatedBaseAction.Name = action.Name;

            GameObject actionInGame = GameObject.Find(action.Id);
            TMP_Text actionText = actionInGame.GetComponent<TMP_Text>();
            actionText.text = updatedBaseAction.Name;
        }
    }

    public void ActionStateBefore(ActionStateBeforeData data)
    {
        ShowActionPlaceInGame(data);
        ShowActionNameInGame(data);
    }

    private void ShowActionPlaceInGame(ActionStateBeforeData data)
    {
        GameObject actionPlace = GameObject.Find("ActionPlace");
        if (actionPlace == null)
        {
            actionPlace = Instantiate(Resources.Load("ActionPlace") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
            actionPlace.name = "ActionPlace";
        }

        Image actionPlaceImage = actionPlace.GetComponent<Image>();
        actionPlaceImage.transform.localScale = new Vector3(1.0f, 1.0f, 0.0f);
        actionPlaceImage.color = new Color32(255, 0, 0, 255);
        if (data.Parameters.Count > 0)
        {
            IO.Swagger.Model.Pose pose = JsonConvert.DeserializeObject<IO.Swagger.Model.Pose>(data.Parameters[0]);
            actionPlaceImage.transform.position = AREditorToSARPosition(pose.Position);
        }
    }

    private void ShowActionNameInGame(ActionStateBeforeData data)
    {
        ActionPoint parentActionPoint = actionPoints.FirstOrDefault(x => x.Actions.Any(y => y.Id == data.ActionId));
        string runningActionType = null;

        if (parentActionPoint != null)
        {
            IO.Swagger.Model.Action runningAction = parentActionPoint.Actions.FirstOrDefault(x => x.Id == data.ActionId);
            string[] parts = runningAction.Type.Split("/");
            if(parts.Length > 0)
            {
                runningActionType = parts[1];
            }
        }

        GameObject parentActionPlace = GameObject.Find("ActionPlace");
        Vector3 actionNamePosition = new Vector3();
        if(parentActionPlace != null)
        {
            actionNamePosition = parentActionPlace.transform.position + new Vector3(0.0f, 0.6f, 0.0f);
        }

        GameObject actionName = Instantiate(Resources.Load("ActionTextRun") as GameObject, actionNamePosition, Quaternion.Euler(0, 0, 180), GameObject.FindGameObjectWithTag("Canvas").transform);
        actionName.name = "ActionTextRun";

        TMP_Text actionNameText = actionName.GetComponent<TMP_Text>();
        actionNameText.text = runningActionType;

        Destroy(actionName.gameObject, 3.0f);
    }

    private void AddActionPointToGame(ActionPoint ap)
    {
        Vector3 apPosition = AREditorToSARPosition(ap.Position);

        GameObject parent = GameObject.FindGameObjectWithTag("Canvas");
        GameObject newActionPoint = Instantiate(Resources.Load("ActionPoint") as GameObject, apPosition, Quaternion.identity, parent.transform);
        newActionPoint.name = ap.Id;
        newActionPoint.GetComponent<Image>().color = new Color32(70, 0, 255, 255);
    }

    private void AddActionToGame(IO.Swagger.Model.Action action, string parentId)
    {
        GameObject parentActionPointInScene = GameObject.Find(parentId);

        ActionPoint parentActionPoint = actionPoints.Find(x => x.Id.Equals(parentId));

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

    private void UpdateActionPointPositionInGame(string id)
    {
        ActionPoint actionPoint = actionPoints.Find(x => x.Id == id);
        GameObject apInScene = GameObject.Find(id);

        if (!string.IsNullOrEmpty(actionPoint.Parent))
        {
            GameObject parent= GameObject.Find(actionPoint.Parent);
            apInScene.transform.position = AREditorToSARPosition(actionPoint.Position) + parent.transform.position;
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

    public void AddedObjectType(object sender, ObjectTypesEventArgs args)
    {
        objectTypes.Add(args.ObjectTypes.FirstOrDefault());
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
                    addedGameObject = Instantiate(Resources.Load("DobotMagician") as GameObject, Vector3.zero, 
                        Quaternion.Euler(0f, 0f, 270f), GameObject.FindGameObjectWithTag("Canvas").transform);
                    addedGameObject.GetComponent<Image>().color = new Color32(50, 255, 0, 255);
                    addedGameObject.name = sceneObject.Id;
                    ShowDobotMagicianRange(addedGameObject);
                    break;

                case "DobotM1":
                    addedGameObject = Instantiate(Resources.Load("DobotM1") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
                    addedGameObject.GetComponent<Image>().color = new Color32(0, 30, 255, 255);
                    addedGameObject.name = sceneObject.Id;
                    ShowDobotM1Range(addedGameObject);
                    break;

                case "sphere":
                    addedGameObject = Instantiate(Resources.Load("Sphere") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
                    addedGameObject.GetComponent<Image>().color = new Color32(255, 228, 0, 255);
                    ObjectTypeMeta sphereObjectType = objectTypes.Find(x => x.Type == sceneObject.Name);
                    addedGameObject.transform.localScale = SetSphereSizeFromObjectType(sphereObjectType);
                    addedGameObject.name = sceneObject.Id;
                    break;

                case "cylinder":
                    addedGameObject = Instantiate(Resources.Load("Cylinder") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
                    addedGameObject.GetComponent<Image>().color = new Color32(255, 228, 0, 255);
                    ObjectTypeMeta cylinderObjectType = objectTypes.Find(x => x.Type == sceneObject.Name);
                    addedGameObject.transform.localScale = SetCylinderSizeFromObjectType(cylinderObjectType);
                    addedGameObject.name = sceneObject.Id;
                    break;

                case "cube":
                    addedGameObject = Instantiate(Resources.Load("Cube") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
                    addedGameObject.GetComponent<Image>().color = new Color32(255, 228, 0, 255);
                    ObjectTypeMeta cubeObjectType = objectTypes.Find(x => x.Type == sceneObject.Type);
                    addedGameObject.transform.localScale = SetCubeSizeFromObjectType(cubeObjectType);
                    addedGameObject.name = sceneObject.Id;
                    break;

                default:
                    break;
            }
            if (addedGameObject != null)
            {
                SetSceneObjectPose(sceneObject);
            }
        }
    }

    private Vector3 SetCubeSizeFromObjectType(ObjectTypeMeta objectType)
    {
        return new Vector3(10 * (float)objectType.ObjectModel.Box.SizeX, 10 * (float)objectType.ObjectModel.Box.SizeY, 0.0f);
    }

    private Vector3 SetSphereSizeFromObjectType(ObjectTypeMeta objectType)
    {
        return new Vector3(10 * (float)objectType.ObjectModel.Sphere.Radius, 10 * (float)objectType.ObjectModel.Sphere.Radius, 0.0f);
    }

    private Vector3 SetCylinderSizeFromObjectType(ObjectTypeMeta objectType)
    {
        return new Vector3(10 * (float)objectType.ObjectModel.Cylinder.Radius, 10 * (float)objectType.ObjectModel.Cylinder.Radius, 0.0f);
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
        GameObject dobotMagicianRange = Instantiate(Resources.Load("RobotRange") as GameObject, dobotMagician.transform);
        DrawCircle(dobotMagicianRange, 3.2f, 0.01f, new Color32(50, 255, 0, 255));
        dobotMagicianRange.transform.Rotate(90f, 0, 0);
    }

    private void ShowDobotM1Range(GameObject dobotM1)
    {
        GameObject dobotM1Range = Instantiate(Resources.Load("RobotRange") as GameObject, dobotM1.transform);
        DrawCircle(dobotM1Range, 4.0f, 0.01f, new Color32(0, 30, 255, 255));
        dobotM1Range.transform.Rotate(90f, 0, 0);
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
        convertedOrientation.Set(0f, 0f, (float)orientation.Z, (float)orientation.W);
        return convertedOrientation;
    }

    private void SetSceneObjectPose(SceneObject sceneObject)
    {
        GameObject sceneObjectInGame = GameObject.Find(sceneObject.Id);
        sceneObjectInGame.transform.rotation = AREditorToSAROrientation(sceneObject.Pose.Orientation);
        sceneObjectInGame.transform.position = AREditorToSARPosition(sceneObject.Pose.Position);
    }

    //Following code in this function was adapted from: https://www.loekvandenouweland.com/content/use-linerenderer-in-unity-to-draw-a-circle.html
    private void DrawCircle(GameObject container, float radius, float lineWidth, Color32 color)
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
        line.startColor = color;
        line.endColor = color;
    }
}
