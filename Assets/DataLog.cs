using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


public class DataLog : MonoBehaviour
{

    // Singleton Pattern for Centralizing Data Collection and Access to the HD only once per participant
    public static DataLog Instance { get; private set; }

    // ==================== Storage Variables ====================
    [HideInInspector]
    public float TimeStamp = 0.0f;
    [HideInInspector]
    public int experiment = 0;
    [HideInInspector]
    public int stimuliPosition = -1;
    [HideInInspector]
    public int selectedPosition = -1;
    [HideInInspector]
    public int Trial = 0;
    [HideInInspector]
    public int Run = 0;
    [HideInInspector]
    public bool Condition = false;
    [HideInInspector]
    public int Reversal = 0;
    [HideInInspector]
    private int behavior = 0;

    public List<int> BehaviorArray = new List<int>();
    [HideInInspector]
    public int Behavior
    {
        set { behavior = value - 1; 
        // * Internal Storage 

        BehaviorArray.Add(behavior);
        if(BehaviorArray.Count>1 &&BehaviorArray[BehaviorArray.Count-1] == 0)
        // * Replace UNCHANGED By the Previous value to facilitate reversal calculation
        BehaviorArray[BehaviorArray.Count-1] = BehaviorArray[BehaviorArray.Count-2];
        }
        get { return behavior + 1; }
    }

    [HideInInspector]
    public float StepSize = 0.0f;
    [HideInInspector]
    public float ReactionTime = 0.0f;
    [HideInInspector]
    private int answer = 0;
    [HideInInspector]
    public int Answer
    {
        set { answer = value - 1; }
        get { return answer + 1; }
    }
    [HideInInspector]
    public float Speed = 0.0f;
    [HideInInspector]
    public float intensity = 0.0f;
    [HideInInspector]
    public float Length = 0.0f;
    

    // ===================== Data Handling Variables
    public int Participant = 0;
    private string file;
    private StringBuilder Storage;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start(){

        createFile();
    }

    private void Update(){

    }

    public void createFile()
    {


        while (File.Exists("Participant_" + Participant + ".csv"))
        {
            Participant++;
        }

        file = "Participant_" + Participant + ".csv";

        string header = "Timestamp, Trial, Run, Condition (Static?), Experiment, Reversal, Stepsize, ReactionTime, Answer (Miss/Detect), SelectedPosition, StimPosition, Behavior(Increase/Decrease), Length, Speed, Intensity, Participant";


        Storage = new StringBuilder();

        Storage.AppendLine(header);

    }

    public void writeLine()
    {


        TimeStamp = Time.realtimeSinceStartup;

        // * Calculate

        // * Save In Memory
        int conditionInt = Condition ? 1 : 0;
        string[] AllVariables = { TimeStamp.ToString(), (Trial+1).ToString(), (Run+1).ToString(), (conditionInt).ToString(), (experiment+1).ToString(), Reversal.ToString(), StepSize.ToString(), ReactionTime.ToString(), answer.ToString(), (selectedPosition+1).ToString(), (stimuliPosition+1).ToString(), behavior.ToString(), Length.ToString(), Speed.ToString(), intensity.ToString(), Participant.ToString() };
        string newLine = string.Join(", ", AllVariables);
        Storage.AppendLine(newLine);
        
    }

    public void save(){
        File.WriteAllText(file, Storage.ToString());
    }
    private void OnDestroy()
    {

        save();
        
    }
}
