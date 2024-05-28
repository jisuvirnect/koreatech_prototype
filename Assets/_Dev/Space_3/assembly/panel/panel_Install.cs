using SpatialSys.UnitySDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class panel_Install : panel_Base
{
    public GameObject prefab_go_Title;
    public GameObject prefab_go_Sequence;

    private GameObject go_SequencePreviewRoot;

    private int sequenceIndex;

    //private List<Sequence> sequenceList;
    private List<Space_3_Sequence> sequences = new List<Space_3_Sequence>();
    private Space_3_Sequence prevSequence = null;

    public scaffold01_1 scaffold01_1;
    private void Awake()
    {
        go_SequencePreviewRoot = gameObject.Search(nameof(go_SequencePreviewRoot)).gameObject;
    }

    private void Start()
    {
        //ResetSequence();
    }

    public override void Open()
    {
        base.Open();
        ResetSequence();
    }

    public override void Close()
    {
        base.Close();
        sequenceIndex = -1;
        prevSequence = null;
        scaffold01_1.Action_ResetObjects();
        DestroySequenceUI();
    }

    private void Update()
    {
        if (!SpatialBridge.networkingService.isMasterClient)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
#if UNITY_EDITOR
            PrevSequence();
#else
            SpatialBridge.networkingService.remoteEvents.RaiseEventAll((byte)RemoteEventIDs.Uninstall);
#endif
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
#if UNITY_EDITOR
            NextSequence();
#else
            SpatialBridge.networkingService.remoteEvents.RaiseEventAll((byte)RemoteEventIDs.Install);
#endif
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ResetSequence();
        }
    }



    /// <summary>
    /// 시퀀스데이터 초기화
    /// </summary>
    /// <param name="item"></param>
    private void CreateSequenceUI()
    {
        for (int i = 0; i < Space_3_SequenceManager.instance.sequenceList.Count; i++)
        {
            Sequence sequence = Space_3_SequenceManager.instance.sequenceList[i];
            Space_3_Sequence space_3_Sequence = Instantiate(prefab_go_Sequence, go_SequencePreviewRoot.transform).GetComponent<Space_3_Sequence>();
            sequences.Add(space_3_Sequence);
            space_3_Sequence.SetData(sequence);
        }
    }

    /// <summary>
    /// 시퀀스 삭제
    /// </summary>
    private void DestroySequenceUI()
    {
        for (int i = go_SequencePreviewRoot.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(go_SequencePreviewRoot.transform.GetChild(i).gameObject);
        }
        sequences.Clear();
    }


    public void ResetSequence()
    {
        sequenceIndex = -1;
        prevSequence = null;
        scaffold01_1.Action_ResetObjects();
        DestroySequenceUI();
        CreateSequenceUI();
        NextSequence();
    }

    public void NextSequence()
    {
        if (prevSequence != null)
        {
            prevSequence.SetSequenceState(SEQUENCE_STATE.AFTER);
            scaffold01_1.Action_scaffold_RenderMode((eBuildScaffold)sequenceIndex, BlendMode.Opaque, prevSequence.sequence);
            scaffold01_1.Action_scaffold_Position((eBuildScaffold)sequenceIndex, BlendMode.Opaque, prevSequence.sequence);
        }
        if (sequences.Count > sequenceIndex + 1)
        {
            sequenceIndex++;
            prevSequence = sequences[sequenceIndex];

            prevSequence.SetSequenceState(SEQUENCE_STATE.FOCUS);
            scaffold01_1.Action_scaffold_RenderMode((eBuildScaffold)sequenceIndex, BlendMode.Transparent, prevSequence.sequence);
            scaffold01_1.Action_scaffold_Position((eBuildScaffold)sequenceIndex, BlendMode.Transparent, prevSequence.sequence);
            scaffold01_1.Action_scaffold_Active((eBuildScaffold)sequenceIndex, true);

        }
    }

    public void GotoSequence(int sequence)
    {
        if (sequenceIndex < sequence)
        {
            NextSequence();
            GotoSequence(sequence);
        }
        if (sequenceIndex > sequence)
        {
            PrevSequence();
            GotoSequence(sequence);
        }
    }

    public void PrevSequence()
    {
        if (sequenceIndex > 0)
        {
            prevSequence.SetSequenceState(SEQUENCE_STATE.BEFORE);
            scaffold01_1.Action_scaffold_RenderMode((eBuildScaffold)sequenceIndex, BlendMode.Opaque, prevSequence.sequence);
            scaffold01_1.Action_scaffold_Position((eBuildScaffold)sequenceIndex, BlendMode.Opaque, prevSequence.sequence);
            scaffold01_1.Action_scaffold_Active((eBuildScaffold)sequenceIndex, false);
            sequenceIndex--;

            prevSequence = sequences[sequenceIndex];
            prevSequence.SetSequenceState(SEQUENCE_STATE.FOCUS);
            scaffold01_1.Action_scaffold_RenderMode((eBuildScaffold)sequenceIndex, BlendMode.Transparent, prevSequence.sequence);
            scaffold01_1.Action_scaffold_Position((eBuildScaffold)sequenceIndex, BlendMode.Transparent, prevSequence.sequence);
            scaffold01_1.Action_scaffold_Active((eBuildScaffold)sequenceIndex, true);
        }
    }

    private void OnEnable()
    {
        SpatialBridge.networkingService.remoteEvents.onEvent += HandleEventReceived;
        SpatialBridge.networkingService.onMasterClientChanged += HandleMasterClientChanged;
    }

    private void OnDisable()
    {
        SpatialBridge.networkingService.remoteEvents.onEvent -= HandleEventReceived;
        SpatialBridge.networkingService.onMasterClientChanged -= HandleMasterClientChanged;
    }

    private void HandleMasterClientChanged(int actorNumber)
    {

    }
    Dictionary<string, object> serverProperties = new Dictionary<string, object>();
    private void HandleEventReceived(NetworkingRemoteEventArgs args)
    {
        switch ((RemoteEventIDs)args.eventID)
        {
            case RemoteEventIDs.SendMagicNumber:
                break;
            case RemoteEventIDs.PrivateMessage:
                SpatialBridge.coreGUIService.DisplayToastMessage((string)args.eventArgs[0]);
                break;
            case RemoteEventIDs.Install:
                NextSequence();

                if(!serverProperties.ContainsKey("idinstall"))
                    serverProperties.Add("idinstall", sequenceIndex);
                else
                    serverProperties["idinstall"] = sequenceIndex;

                // SetServerProperties 메서드 호출
                SpatialBridge.networkingService.SetServerProperties(serverProperties);
                break;
            case RemoteEventIDs.Checklist:
                break;
            case RemoteEventIDs.Checkout:
                break;
            case RemoteEventIDs.Uninstall:
                PrevSequence();
                break;
            default:
                break;
        }
    }
}
