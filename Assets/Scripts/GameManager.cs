using Base;
using IO.Swagger.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    private List<ActionPoint> actionPoints = new List<ActionPoint>();

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
        PrintStateInfoText("main screen");
    }

    public void OpenProject(Scene scene, Project project)
    {
        PrintStateInfoText("project");

        foreach (var ap in project.ActionPoints)
        {
            ActionPoint newAp = ap;

            actionPoints.Add(newAp);
            AddActionPointToScene(newAp);
        }
    }

    public void OpenScene(Scene scene)
    {
        PrintStateInfoText("scene");
    }

    private void PrintStateInfoText(string state)
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
            case "main screen":
                stateInfoText.text = "Main screen";
                stateInfoText.color = new Color32(255, 255, 255, 255);
                stateInfo.GetComponent<TMP_Text>().ForceMeshUpdate();
                break;

            case "scene":
                stateInfoText.text = "Editing scene";
                stateInfoText.color = new Color32(0, 255, 0, 255);
                stateInfo.GetComponent<TMP_Text>().ForceMeshUpdate();
                break;

            case "project":
                stateInfoText.text = "Editing project";
                stateInfoText.color = new Color32(0, 150, 255, 255);
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

    private void AddActionPointToScene(ActionPoint ap)
    {
        GameObject newActionPoint = Instantiate(Resources.Load("ActionPointPrefab") as GameObject,
                AREditorToSARPosition(ap.Position),
                Quaternion.identity, GameObject.FindGameObjectWithTag("Canvas").transform);
        newActionPoint.name = ap.Id;
    }

    private void UpdateActionPointPoistionInScene(string id)
    {
        ActionPoint actionPoint = actionPoints.Find(x => x.Id == id);
        GameObject apInScene = GameObject.Find(id);
        apInScene.transform.position = AREditorToSARPosition(actionPoint.Position);
    }

    private Vector3 AREditorToSARPosition(Position passedPosition)
    {
        Vector3 position = new Vector3();
        position.Set(-1 * 10 * (float)passedPosition.X, -1 * 10 * (float)passedPosition.Y, 0.0f);
        return position;
    }

    private void DestroyActionPointInScene(string id)
    {
        GameObject removedAp = GameObject.Find(id);
        Destroy(removedAp);
    }

    private void DestroyObjectPrefabs()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("ObjectPrefab");
        foreach (var gameObject in gameObjects)
        {
            Destroy(gameObject.gameObject);
        }
    }
}
