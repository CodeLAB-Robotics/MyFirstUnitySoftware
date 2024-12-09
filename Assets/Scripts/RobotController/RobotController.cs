using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Collections;
using System.IO;
using static RobotController;
using System.Linq;

/// <summary>
/// 로봇 3D Object를 RobotController의 버튼, 인풋필드의 값으로 움직인다.
/// - Teach 버튼을 누르면 각 Axis의 값이 Step으로 저장된다.
/// - SingleCycle, Cycle, Stop, E-Stop 버튼을 누르면 로봇이 동작한다.
/// 필요속성: 로봇의 모터 회전 속도(0~100), Duration, Min Angle, Max Angle
///          step(speed, duration, suction, angles)
/// </summary>
public class RobotController : MonoBehaviour
{
    [Serializable]
    public class Step
    {
        public int stepNumber;
        public float speed = 100;
        public float duration;
        public bool isSuctionOn;

        public float angleAxis1;
        public float minAngleAxis1;
        public float maxAngleAxis1;

        public float angleAxis2;
        public float minAngleAxis2;
        public float maxAngleAxis2;

        public float angleAxis3;
        public float minAngleAxis3;
        public float maxAngleAxis3;

        public float angleAxis4;
        public float minAngleAxis4;
        public float maxAngleAxis4;

        public float angleAxis5;
        public float minAngleAxis5;
        public float maxAngleAxis5;

        public Step(int _stepNumber, float _speed, float _duration, bool _isSuctionOn)
        {
            stepNumber = _stepNumber;
            speed = _speed;
            duration = _duration;
            isSuctionOn = _isSuctionOn;
        }
    }
    public List<Step> steps = new List<Step>();
    [SerializeField] Step originStep;
    [SerializeField] Step eStopStep;
    public bool isRunning = false;
    [SerializeField] bool isStopped = false;
    [SerializeField] bool isEStopped = false;
    [SerializeField] bool isCycleClicked = false;
    [SerializeField] float resolution = 0.002f;

    [Header("Axis Pivots")]
    [SerializeField] Transform motorAxis1;
    [SerializeField] Transform motorAxis2;
    [SerializeField] Transform motorAxis3;
    [SerializeField] Transform motorAxis4;
    [SerializeField] Transform motorAxis5;

    [Header("UI 정리")]
    [SerializeField] TMP_Text nowStepInfoTxt;
    [SerializeField] int totalSteps;
    public int currentStepNumber;
    [SerializeField] TMP_InputField stepInput;
    [SerializeField] TMP_InputField speedInput;
    [SerializeField] TMP_InputField durationInput;
    [SerializeField] Toggle suctionToggle;
    [SerializeField] TMP_InputField angleAxis1Input;
    [SerializeField] TMP_InputField angleAxis2Input;
    [SerializeField] TMP_InputField angleAxis3Input;
    [SerializeField] TMP_InputField angleAxis4Input;
    [SerializeField] TMP_InputField angleAxis5Input;
    [SerializeField] List<Button> buttons = new List<Button>();
    [SerializeField] TMP_InputField loadFileInput;

    Coroutine currentCoroutine;
    int cycleClkCnt = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originStep = new Step(-1, 100, 0, false);

        nowStepInfoTxt.text = $"Total step count: {totalSteps} / Current step: {currentStepNumber}";
        
        stepInput.text = "0";
        speedInput.text = "100";
        durationInput.text = "0";
        suctionToggle.isOn = false;

        angleAxis1Input.text = "0";
        angleAxis2Input.text = "0";
        angleAxis3Input.text = "0";
        angleAxis4Input.text = "0";
        angleAxis5Input.text = "0";

        string path = gameObject.name.Contains("RobotA") ? "robotA.csv" : "robotB.csv";
        LoadTeachingFile(path);
    }

    // Teach 버튼을 누르면 각 Axis의 값이 Step으로 저장된다.
    public void OnTeachBtnClkEvent()
    {
        //int stepNumber;
        //bool isCorrect = int.TryParse(stepInput.text, out stepNumber);
        //if (!isCorrect)
        //{
        //    print("올바른 값을 입력해 주세요.");
        //    return;
        //}

        float speed;
        bool isCorrect = float.TryParse(speedInput.text, out speed);
        if (!isCorrect)
        {
            print("올바른 값을 입력해 주세요.");
            return;
        }

        float duration;
        isCorrect = float.TryParse(durationInput.text, out duration);
        if (!isCorrect)
        {
            print("올바른 값을 입력해 주세요.");
            return;
        }

        float angleAxis1;
        isCorrect = float.TryParse(angleAxis1Input.text, out angleAxis1);
        if (!isCorrect)
        {
            print("올바른 값을 입력해 주세요.");
            return;
        }

        float angleAxis2;
        isCorrect = float.TryParse(angleAxis2Input.text, out angleAxis2);
        if (!isCorrect)
        {
            print("올바른 값을 입력해 주세요.");
            return;
        }

        float angleAxis3;
        isCorrect = float.TryParse(angleAxis3Input.text, out angleAxis3);
        if (!isCorrect)
        {
            print("올바른 값을 입력해 주세요.");
            return;
        }

        float angleAxis4;
        isCorrect = float.TryParse(angleAxis4Input.text, out angleAxis4);
        if (!isCorrect)
        {
            print("올바른 값을 입력해 주세요.");
            return;
        }

        float angleAxis5;
        isCorrect = float.TryParse(angleAxis5Input.text, out angleAxis5);
        if (!isCorrect)
        {
            print("올바른 값을 입력해 주세요.");
            return;
        }


        Step step = new Step(totalSteps, speed, duration, suctionToggle.isOn);
        step.angleAxis1 = angleAxis1;
        step.angleAxis2 = angleAxis2;
        step.angleAxis3 = angleAxis3;
        step.angleAxis4 = angleAxis4;
        step.angleAxis5 = angleAxis5;


        if(cycleClkCnt > 0 && steps.Count != 0)
        {
            steps.RemoveAt(steps.Count - 1);

            cycleClkCnt = 0;
        }

        steps.Add(step);

        totalSteps++;
        stepInput.text = totalSteps.ToString();
        nowStepInfoTxt.text = $"Total step count: {totalSteps} / Current step: {currentStepNumber}";
        print("Step이 추가되었습니다.");
    }

    // Clear 버튼을 누르면 모든 Step들이 지워진다.
    public void OnClearBtnClkEvent()
    {
        steps.Clear();

        totalSteps = currentStepNumber = 0;

        nowStepInfoTxt.text = $"Total step count: {totalSteps} / Current step: {currentStepNumber}";

        stepInput.text = "0";
        speedInput.text = "100";
        durationInput.text = "0";
        suctionToggle.isOn = false;

        angleAxis1Input.text = "0";
        angleAxis2Input.text = "0";
        angleAxis3Input.text = "0";
        angleAxis4Input.text = "0";
        angleAxis5Input.text = "0";
    }

    // SingleCycle, Cycle, Stop, E-Stop 버튼을 누르면 로봇이 동작한다.
    public void OnSingleCycleBtnClkEvent()
    {
        if (isEStopped)
        {
            isEStopped = false;

            List<Step> newSteps = new List<Step>();
            // eStopStep -> (멈춘 당시의 스텝 인덱스 + 1) -> 마지막 스탭까지 작동
            // E-Stop 버튼 누른 시점의 스탭(eStopStep) 부터
            // 기존 작동중이던 남은 스탭들을 newSteps에 저장
            for (int i = currentStepNumber; i < steps.Count; i++)
            {
                newSteps.Add(steps[i]);
            }

            currentCoroutine = StartCoroutine(Run(newSteps));
        }
        else
        {
            if (isRunning) return;

                // 각 스탭에 따라 로봇의 모터가 움직여야 한다.
                currentCoroutine = StartCoroutine(Run());
        }

        SetButtonsActive(false);
    }

    public void OnCycleBtnClkEvent()
    {
        isCycleClicked = true;

        if(cycleClkCnt == 0) 
            steps.Add(steps[0]);

        cycleClkCnt++;

        currentCoroutine = StartCoroutine(RunCycle());

        SetButtonsActive(false);
    }

    public void SetButtonsActive(bool active)
    {
        foreach(var btn in buttons)
        {
            btn.interactable = active;
        }
    }

    IEnumerator RunCycle()
    {
        while(isCycleClicked)
        {
            yield return Run();
        }

        if (cycleClkCnt > 0 && steps.Count != 0)
        {
            steps.RemoveAt(steps.Count - 1);

            cycleClkCnt = 0;

        }

        SetButtonsActive(true);
    }

    /// <summary>
    /// 한 사이클 끝나고 멈춤 -> 원래 위치로 복귀
    /// </summary>
    public void OnStopBtnClkEvent()
    {
        isStopped = true;

        isCycleClicked = false;
    }

    /// <summary>
    /// 현재 위치에서 로봇의 움직임을 즉시 멈춘다. -> 해당 위치에서 부터 다시 시작
    /// </summary>
    public void OnEStopBtnClkEvent()
    {
        isEStopped = true;

        isCycleClicked = false;
        // 멈추는 순간 각도의 정보를 새로운 스탭 정보에 저장
        // -> 다시 싱글 버튼 클릭시 멈춘 위치에서 부터 마지막 스탭까지 연결
    }

    /// <summary>
    /// 로봇이 최근 위치에서 부터 처음 위치로 이동
    /// </summary>
    public void OnOriginBtnClkEvent()
    {
        if (isRunning) return;

        StartCoroutine(RunOrigin(steps[steps.Count - 1], originStep));
    }

    IEnumerator Run()
    {
        isRunning = true;

        if (steps.Count > 0)
        {
            for (int i = 0; i < steps.Count; i++) // 2개: 0, 1, ?
            {
                currentStepNumber = i;
                nowStepInfoTxt.text = $"Total step count: {totalSteps} / Current step: {currentStepNumber}";

                if (i - 1 < 0)
                {
                    continue;
                }

                yield return RunStep(steps[i - 1], steps[i]);

                if (isEStopped)
                {
                    SetButtonsActive(true);
                    break;
                }
            }
        }

        if(!isCycleClicked)
        {
            SetButtonsActive(true);

            //Button originBtn = buttons.Find(btn => btn.name == "Origin Button");
            //originBtn.interactable = true;
        }

        isRunning = false;
    }

    IEnumerator Run(List<Step> stepList)
    {
        isRunning = true;

        if (stepList.Count > 0)
        {
            for (int i = 0; i < stepList.Count; i++) // 2개: 0, 1, ?
            {
                currentStepNumber = i;
                nowStepInfoTxt.text = $"Total step count: {totalSteps} / Current step: {currentStepNumber}";

                if (i - 1 < 0)
                {
                    continue;
                }

                yield return RunStep(stepList[i - 1], stepList[i]);

                if (isEStopped)
                {
                    SetButtonsActive(true);
                    break;
                }
            }
        }

        isRunning = false;
    }

    IEnumerator RunStep(Step prevStep, Step nextStep)
    {
        Vector3 prevAxis1Euler  = new Vector3(0, prevStep.angleAxis1, 0); // Axis1: Y축 기준으로 회전
        Vector3 nextAxis1AEuler = new Vector3(0, nextStep.angleAxis1, 0);

        Vector3 prevAxis2Euler  = new Vector3(0, 0, prevStep.angleAxis2); // Axis2: Z축 기준으로 회전
        Vector3 nextAxis2AEuler = new Vector3(0, 0, nextStep.angleAxis2);

        Vector3 prevAxis3Euler  = new Vector3(0, 0, prevStep.angleAxis3); // Axis3: Z축 기준으로 회전
        Vector3 nextAxis3AEuler = new Vector3(0, 0, nextStep.angleAxis3);

        Vector3 prevAxis4Euler  = new Vector3(prevStep.angleAxis4, 0, 0); // Axis4: X축 기준으로 회전
        Vector3 nextAxis4AEuler = new Vector3(nextStep.angleAxis4, 0, 0);

        Vector3 prevAxis5Euler  = new Vector3(0, 0, prevStep.angleAxis5); // Axis5: Z축 기준으로 회전
        Vector3 nextAxis5AEuler = new Vector3(0, 0, nextStep.angleAxis5);

        float currentTime = 0;
        while (!isEStopped)
        {
            currentTime += Time.deltaTime;

            if((currentTime / (prevStep.speed * 0.01f)) > 1)
            {
                break;
            }

            motorAxis1.localRotation = RotateAngle(prevAxis1Euler, nextAxis1AEuler, currentTime / (prevStep.speed * 0.01f));
            motorAxis2.localRotation = RotateAngle(prevAxis2Euler, nextAxis2AEuler, currentTime / (prevStep.speed * 0.01f));
            motorAxis3.localRotation = RotateAngle(prevAxis3Euler, nextAxis3AEuler, currentTime / (prevStep.speed * 0.01f));
            motorAxis4.localRotation = RotateAngle(prevAxis4Euler, nextAxis4AEuler, currentTime / (prevStep.speed * 0.01f));
            motorAxis5.localRotation = RotateAngle(prevAxis5Euler, nextAxis5AEuler, currentTime / (prevStep.speed * 0.01f));

            yield return new WaitForEndOfFrame();
        }

        if (isEStopped)
        {
            eStopStep = new Step(-1, prevStep.speed, prevStep.duration, prevStep.isSuctionOn);
            eStopStep.angleAxis1 = motorAxis1.localRotation.eulerAngles.y;
            eStopStep.angleAxis2 = motorAxis2.localRotation.eulerAngles.z;
            eStopStep.angleAxis3 = motorAxis3.localRotation.eulerAngles.z;
            eStopStep.angleAxis4 = motorAxis4.localRotation.eulerAngles.x;
            eStopStep.angleAxis5 = motorAxis5.localRotation.eulerAngles.z;
        }

        yield return new WaitForSeconds(prevStep.duration);
    }

    IEnumerator RunOrigin(Step prevStep, Step nextStep)
    {
        isRunning = true;

        Vector3 prevAxis1Euler = new Vector3(0, prevStep.angleAxis1, 0); // Axis1: Y축 기준으로 회전
        Vector3 nextAxis1AEuler = new Vector3(0, nextStep.angleAxis1, 0);

        Vector3 prevAxis2Euler = new Vector3(0, 0, prevStep.angleAxis2); // Axis2: Z축 기준으로 회전
        Vector3 nextAxis2AEuler = new Vector3(0, 0, nextStep.angleAxis2);

        Vector3 prevAxis3Euler = new Vector3(0, 0, prevStep.angleAxis3); // Axis3: Z축 기준으로 회전
        Vector3 nextAxis3AEuler = new Vector3(0, 0, nextStep.angleAxis3);

        Vector3 prevAxis4Euler = new Vector3(prevStep.angleAxis4, 0, 0); // Axis4: X축 기준으로 회전
        Vector3 nextAxis4AEuler = new Vector3(nextStep.angleAxis4, 0, 0);

        Vector3 prevAxis5Euler = new Vector3(0, 0, prevStep.angleAxis5); // Axis5: Z축 기준으로 회전
        Vector3 nextAxis5AEuler = new Vector3(0, 0, nextStep.angleAxis5);

        float currentTime = 0;
        while (!isEStopped)
        {
            currentTime += Time.deltaTime;

            if ((currentTime / (prevStep.speed * 0.01f)) > 1)
            {
                break;
            }

            motorAxis1.localRotation = RotateAngle(prevAxis1Euler, nextAxis1AEuler, currentTime / (prevStep.speed * 0.01f));
            motorAxis2.localRotation = RotateAngle(prevAxis2Euler, nextAxis2AEuler, currentTime / (prevStep.speed * 0.01f));
            motorAxis3.localRotation = RotateAngle(prevAxis3Euler, nextAxis3AEuler, currentTime / (prevStep.speed * 0.01f));
            motorAxis4.localRotation = RotateAngle(prevAxis4Euler, nextAxis4AEuler, currentTime / (prevStep.speed * 0.01f));
            motorAxis5.localRotation = RotateAngle(prevAxis5Euler, nextAxis5AEuler, currentTime / (prevStep.speed * 0.01f));

            yield return new WaitForEndOfFrame();
        }

        if (isEStopped)
        {
            eStopStep = new Step(-1, prevStep.speed, prevStep.duration, prevStep.isSuctionOn);
            eStopStep.angleAxis1 = motorAxis1.localRotation.eulerAngles.y;
            eStopStep.angleAxis2 = motorAxis2.localRotation.eulerAngles.z;
            eStopStep.angleAxis3 = motorAxis3.localRotation.eulerAngles.z;
            eStopStep.angleAxis4 = motorAxis4.localRotation.eulerAngles.x;
            eStopStep.angleAxis5 = motorAxis5.localRotation.eulerAngles.z;
        }


        yield return new WaitForSeconds(prevStep.duration);

        isRunning = false;
    }

    private Quaternion RotateAngle(Vector3 from, Vector3 to, float t)
    {
        return Quaternion.Slerp(Quaternion.Euler(from), Quaternion.Euler(to), t);
    }

    /// <summary>
    /// Steps 리스트에 저장된 step들을 CSV 파일형식으로 저장한다.
    /// 형식: stepNumber,speed,duration,isSuctionOn,axis1,axis2,axis3,axis4,axis5
    /// </summary>
    public void SaveCSVFile(Step step)
    {
        using (FileStream fs = new FileStream("robotSteps.csv", FileMode.Append))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                string line = $"{step.stepNumber},{step.speed},{step.duration},{step.isSuctionOn}," +
                    $"{step.angleAxis1},{step.angleAxis2},{step.angleAxis3},{step.angleAxis4},{step.angleAxis5}";

                sw.WriteLine(line);
            }
        }
    }

    int fileCnt = 0;
    public void OnSaveBtnClkEvent()
    {
        string path = $"robotSteps_{fileCnt++}.csv";
        if (File.Exists(path))
        {
            print("파일이 이미 존재합니다. 다시 한번 클릭하면 새로운 파일이 생성됩니다.");

            return;
        }
        
        using (FileStream fs = new FileStream(path, FileMode.Append))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                foreach(var step in steps)
                {
                    string line = $"{step.stepNumber},{step.speed},{step.duration},{step.isSuctionOn}," +
                    $"{step.angleAxis1},{step.angleAxis2},{step.angleAxis3},{step.angleAxis4},{step.angleAxis5}";

                    sw.WriteLine(line);
                }
            }
        }

        print($"파일을 저장했습니다. {path}");
    }

    public void OnLoadBtnClkEvent()
    {
        string path = loadFileInput.text; // robotSteps_0.csv

        if(File.Exists(path))
        {
            List<Step> tempSteps = new List<Step>();

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string line;
                    while((line = sr.ReadLine()) != null)
                    {
                        // stepNumber,speed,duration,axis1,axis2,axis3,axis4,axis5
                        string[] splited = line.Split(",");

                        int stepNumber;
                        bool isCorrect = int.TryParse(splited[0], out stepNumber);
                        if(!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        float speed;
                        isCorrect = float.TryParse(splited[1], out speed);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        float duration;
                        isCorrect = float.TryParse(splited[2], out duration);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        bool isSuctionOn;
                        isCorrect = bool.TryParse(splited[3], out isSuctionOn);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        float angleAxis1;
                        isCorrect = float.TryParse(splited[4], out angleAxis1);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        float angleAxis2;
                        isCorrect = float.TryParse(splited[5], out angleAxis2);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        float angleAxis3;
                        isCorrect = float.TryParse(splited[6], out angleAxis3);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        float angleAxis4;
                        isCorrect = float.TryParse(splited[7], out angleAxis4);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        float angleAxis5;
                        isCorrect = float.TryParse(splited[8], out angleAxis5);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        Step stepLoaded = new Step(stepNumber, speed, duration, isSuctionOn);
                        stepLoaded.angleAxis1 = angleAxis1;
                        stepLoaded.angleAxis2 = angleAxis2;
                        stepLoaded.angleAxis3 = angleAxis3;
                        stepLoaded.angleAxis4 = angleAxis4;
                        stepLoaded.angleAxis5 = angleAxis5;

                        tempSteps.Add(stepLoaded);
                    }
                }
            }

            steps.Clear();
            steps.AddRange(tempSteps);
            //steps = tempSteps.ToList();

            print("파일 읽기가 완료되었습니다.");
        }
        else
        {
            print("파일이 존재하지 않습니다. 파일 이름을 확인해 주세요.");
        }
    }

    public void LoadTeachingFile(string path)
    {
        if (File.Exists(path))
        {
            List<Step> tempSteps = new List<Step>();

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        // stepNumber,speed,duration,axis1,axis2,axis3,axis4,axis5
                        string[] splited = line.Split(",");

                        int stepNumber;
                        bool isCorrect = int.TryParse(splited[0], out stepNumber);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        float speed;
                        isCorrect = float.TryParse(splited[1], out speed);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        float duration;
                        isCorrect = float.TryParse(splited[2], out duration);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        bool isSuctionOn;
                        isCorrect = bool.TryParse(splited[3], out isSuctionOn);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        float angleAxis1;
                        isCorrect = float.TryParse(splited[4], out angleAxis1);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        float angleAxis2;
                        isCorrect = float.TryParse(splited[5], out angleAxis2);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        float angleAxis3;
                        isCorrect = float.TryParse(splited[6], out angleAxis3);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        float angleAxis4;
                        isCorrect = float.TryParse(splited[7], out angleAxis4);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        float angleAxis5;
                        isCorrect = float.TryParse(splited[8], out angleAxis5);
                        if (!isCorrect)
                        {
                            print("파일 구조가 잘못되었습니다. 파일을 확인 후 시도해 주세요.");

                            return;
                        }

                        Step stepLoaded = new Step(stepNumber, speed, duration, isSuctionOn);
                        stepLoaded.angleAxis1 = angleAxis1;
                        stepLoaded.angleAxis2 = angleAxis2;
                        stepLoaded.angleAxis3 = angleAxis3;
                        stepLoaded.angleAxis4 = angleAxis4;
                        stepLoaded.angleAxis5 = angleAxis5;

                        tempSteps.Add(stepLoaded);
                    }
                }
            }

            steps.Clear();
            steps.AddRange(tempSteps);
            //steps = tempSteps.ToList();

            print("파일 읽기가 완료되었습니다.");
        }
        else
        {
            print("파일이 존재하지 않습니다. 파일 이름을 확인해 주세요.");
        }
    }

    bool isUpBtnPressed = false;
    bool isDownBtnPressed = false;
    /// <summary>
    /// 1. axis의 종류에 따라, UI에 있는 Axis Input의 값 변경
    /// 2. 변경되는 값을 실제 motorAxis1에 적용
    /// </summary>
    /// <param name="axis"></param>
    public void OnUpBtnEnterClkEvent(string axis)
    {
        isUpBtnPressed = true;
        StartCoroutine(OnPointerUpStay(axis));
    }

    public void OnUpBtnExitClkEvent()
    {
        isUpBtnPressed = false;
    }

    public void OnDownBtnEnterClkEvent(string axis)
    {
        isDownBtnPressed = true;
        StartCoroutine(OnPointerDownStay(axis));
    }

    public void OnDownBtnExitClkEvent()
    {
        isDownBtnPressed = false;
    }

    float angleAxis1;
    float angleAxis2;
    float angleAxis3;
    float angleAxis4;
    float angleAxis5;
    IEnumerator OnPointerUpStay(string axis)
    {
        while (isUpBtnPressed)
        {
            switch(axis)
            {
                case "Axis1":
                    angleAxis1 += resolution;
                    angleAxis1Input.text = angleAxis1.ToString();
                    motorAxis1.transform.localRotation = Quaternion.Euler(0, angleAxis1, 0);
                    break;
                case "Axis2":
                    angleAxis2 += resolution;
                    angleAxis2Input.text = angleAxis2.ToString();
                    motorAxis2.transform.localRotation = Quaternion.Euler(0, 0, angleAxis2);
                    break;
                case "Axis3":
                    angleAxis3 += resolution;
                    angleAxis3Input.text = angleAxis3.ToString();
                    motorAxis3.transform.localRotation = Quaternion.Euler(0, 0, angleAxis3);
                    break;
                case "Axis4":
                    angleAxis4 += resolution;
                    angleAxis4Input.text = angleAxis4.ToString();
                    motorAxis4.transform.localRotation = Quaternion.Euler(angleAxis4, 0, 0);
                    break;
                case "Axis5":
                    angleAxis5 += resolution;
                    angleAxis5Input.text = angleAxis5.ToString();
                    motorAxis5.transform.localRotation = Quaternion.Euler(0, 0, angleAxis5);
                    break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator OnPointerDownStay(string axis)
    {
        while (isDownBtnPressed)
        {
            switch (axis)
            {
                case "Axis1":
                    angleAxis1 -= resolution;
                    angleAxis1Input.text = angleAxis1.ToString();
                    motorAxis1.transform.localRotation = Quaternion.Euler(0, angleAxis1, 0);
                    break;
                case "Axis2":
                    angleAxis2 -= resolution;
                    angleAxis2Input.text = angleAxis2.ToString();
                    motorAxis2.transform.localRotation = Quaternion.Euler(0, 0, angleAxis2);
                    break;
                case "Axis3":
                    angleAxis3 -= resolution;
                    angleAxis3Input.text = angleAxis3.ToString();
                    motorAxis3.transform.localRotation = Quaternion.Euler(0, 0, angleAxis3);
                    break;
                case "Axis4":
                    angleAxis4 -= resolution;
                    angleAxis4Input.text = angleAxis4.ToString();
                    motorAxis4.transform.localRotation = Quaternion.Euler(angleAxis4, 0, 0);
                    break;
                case "Axis5":
                    angleAxis5 -= resolution;
                    angleAxis5Input.text = angleAxis5.ToString();
                    motorAxis5.transform.localRotation = Quaternion.Euler(0, 0, angleAxis5);
                    break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        print(other.name);
    }
}
