using Base;
using IO.Swagger.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : Base.Singleton<GameManager>
{
    private List<ActionPoint> actionPoints = new List<ActionPoint>();

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
    }

    public void OpenProject(Scene scene, Project project)
    {
        PrintStateInfoText(ScreenStateEnum.EditingProject);

        foreach (var ap in project.ActionPoints)
        {
            if (project.ActionPoints.Count != 0)
            {
                actionPoints.Add(ap);
                AddActionPointToScene(ap);
                SpawnActionsInScene(ap.Actions, ap.Id);
            }
        }   
    }

    public void PackageStateUpdated(PackageStateData packageStateData)
    {
        switch(packageStateData.State)
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
        }
    }

    private void PrintStateInfoText(ScreenStateEnum state)
    {
        GameObject stateInfo = GameObject.Find("StateInfoText");
        if (stateInfo == null)
        {
            stateInfo = Instantiate(Resources.Load("StateInfoText") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform);
            stateInfo.name = "StateInfoText";
        }
        TMP_Text stateInfoText = stateInfo.GetComponent<TMP_Text>();

        switch (state)
        {
            case ScreenStateEnum.MainScreen:
                stateInfoText.text = "Main screen";
                stateInfoText.color = new Color32(255, 255, 255, 255);
                stateInfo.GetComponent<TMP_Text>().ForceMeshUpdate();
                break;

            case ScreenStateEnum.EditingScene:
                stateInfoText.text = "Editing scene";
                stateInfoText.color = new Color32(0, 150, 255, 255);
                stateInfo.GetComponent<TMP_Text>().ForceMeshUpdate();
                break;

            case ScreenStateEnum.EditingProject:
                stateInfoText.text = "Editing project";
                stateInfoText.color = new Color32(255, 0, 255, 255);
                stateInfo.GetComponent<TMP_Text>().ForceMeshUpdate();
                break;

            case ScreenStateEnum.RunningPackage:
                stateInfoText.text = "Running";
                stateInfoText.color = new Color32(0, 255, 0, 255);
                stateInfo.GetComponent<TMP_Text>().ForceMeshUpdate();
                break;

            case ScreenStateEnum.PausingPackage:
                stateInfoText.text = "Pausing";
                stateInfoText.color = new Color32(255, 0, 100, 255);
                stateInfo.GetComponent<TMP_Text>().ForceMeshUpdate();
                break;

            case ScreenStateEnum.PausedPackage:
                stateInfoText.text = "Paused";
                stateInfoText.color = new Color32(255, 0, 0, 255);
                stateInfo.GetComponent<TMP_Text>().ForceMeshUpdate();
                break;
        }
    }

    public void CloseScene()
    {
        DestroyObjectPrefabs();
    }

    public void CloseProject()
    {
        DestroyObjectPrefabs();
        actionPoints.Clear();
    }

    private void ApAdd(object sender, ProjectActionPointEventArgs args)
    {
        ActionPoint newAp = args.ActionPoint;
        actionPoints.Add(newAp);
        AddActionPointToScene(newAp);
    }

    private void ApChangeUpdateBase(object sender, BareActionPointEventArgs args)
    {
        ActionPoint updatedAp = actionPoints.Find(x => x.Id == args.ActionPoint.Id);
        updatedAp.Position = args.ActionPoint.Position;
        UpdateActionPointPoistionInScene(args.ActionPoint.Id);
    }

    private void ApRemove(object sender, StringEventArgs args)
    {
        ActionPoint removedAp = actionPoints.Find(x => x.Id == args.Data);
        actionPoints.Remove(removedAp);
        DestroyActionPointInScene(args.Data);
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

        AddActionToScene(action, parentId);
        parentActionPoint.Actions.Add(action);
    }

    public void ActionRemoved(BareAction action)
    {
        ActionPoint parentActionPoint = actionPoints.FirstOrDefault(x => x.Actions.Any(y => y.Id == action.Id));
        
        if(parentActionPoint != null)
        {
            IO.Swagger.Model.Action removedAction = parentActionPoint.Actions.FirstOrDefault(x => x.Id == action.Id);
            parentActionPoint.Actions.Remove(removedAction);
        }

        DestroyActionInScene(action.Id);
    }

    private void AddActionPointToScene(ActionPoint ap)
    {
        GameObject newActionPoint = Instantiate(Resources.Load("ActionPointPrefab") as GameObject,
                AREditorToSARPosition(ap.Position),
                Quaternion.identity, GameObject.FindGameObjectWithTag("Canvas").transform);
        newActionPoint.name = ap.Id;
    }

    private void AddActionToScene(IO.Swagger.Model.Action action, string parentId)
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

    private void SpawnActionsInScene(List<IO.Swagger.Model.Action> actions, string parentId)
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

    private void UpdateActionPointPoistionInScene(string id)
    {
        ActionPoint actionPoint = actionPoints.Find(x => x.Id == id);
        GameObject apInScene = GameObject.Find(id);
        apInScene.transform.position = AREditorToSARPosition(actionPoint.Position);
    }

    private void DestroyActionPointInScene(string id)
    {
        GameObject removedAp = GameObject.Find(id);
        Destroy(removedAp);
    }

    private void DestroyActionInScene(string id)
    {
        GameObject removedAction = GameObject.Find(id);
        Destroy(removedAction);
    }

    private void DestroyObjectPrefabs()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Prefab");
        foreach (var gameObject in gameObjects)
        {
            Destroy(gameObject.gameObject);
        }
    }
    private Vector3 AREditorToSARPosition(Position passedPosition)
    {
        Vector3 position = new Vector3();
        position.Set(-1 * 10 * (float)passedPosition.X, -1 * 10 * (float)passedPosition.Y, 0.0f);
        return position;
    }
}
