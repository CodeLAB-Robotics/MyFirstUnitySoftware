using TMPro;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// 1. �α���: �̸���, �н����� �Է½� ȸ������ ���ο� ���� �α����Ѵ�.
/// 2. ȸ������: �̸���, �н����� �Է� �� �̸��� ������ �Ϸ�ȴٸ� ȸ�������� �ȴ�.
/// 3. ������ �ҷ�����: ���ѿ� ���� DB�� Ư�� ������ �ҷ��´�.
/// </summary>
public class FirebaseAuthManager : MonoBehaviour
{
    [Header("�α��� UI")]
    [SerializeField] GameObject loginPanel;
    [SerializeField] GameObject iDInfoPanel;
    [SerializeField] TMP_InputField loginEmailInput;
    [SerializeField] TMP_InputField loginPWInput;
    [SerializeField] TMP_InputField emailInput;
    [SerializeField] TMP_InputField nameInput;
    [SerializeField] string dbURL;
    [SerializeField] string uID;

    [Header("ȸ������ UI")]
    [SerializeField] GameObject signUpPanel;
    [SerializeField] GameObject verificationPanel;
    [SerializeField] TMP_InputField signUpEmailInput;
    [SerializeField] TMP_InputField signUpPWInput;
    [SerializeField] TMP_InputField signUpPWCheckInput;
    [SerializeField] TMP_Text verificationTxt;

    FirebaseAuth auth;
    FirebaseUser user;
    bool isLoggedIn = false;
    bool isSignupCompleted = false;
    bool signedIn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new System.Uri(dbURL);

        auth = FirebaseAuth.DefaultInstance;

        auth.StateChanged += AuthStateChangedEvent;

        AuthStateChangedEvent(this, null);

        auth.SignOut();
    }

    void AuthStateChangedEvent(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            signedIn = user != auth.CurrentUser && auth.CurrentUser != null
                && auth.CurrentUser.IsValid();
            if (!signedIn && user != null)
            {
                print("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                print("Signed in " + user.UserId);
            }
        }
    }

    public void OnLoginBtnClkEvent()
    {
        // �α��� �Ϸ�� �ٸ� ������ �Ѿ��
        StartCoroutine(CoLogin(loginEmailInput.text, loginPWInput.text));
    }

    public void OnCancelBtnClkEvent()
    {
        Application.Quit();
    }

    IEnumerator CoLogin(string email,  string password)
    {
        var logInTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        logInTask.ContinueWith(t =>
        {
            if (t.IsCanceled)
            {
                print("�α��� ���");
            }
            else if (t.IsFaulted)
            {
                print("�α��� ����");
            }
            else if(t.IsCompleted)
            {
                print("�α��� Ȯ��(�̸��� ���� Ȯ�� �ʿ�");
            }
        });

        yield return new WaitUntil(() => logInTask.IsCompleted);

        user = logInTask.Result.User;

        if (!user.IsEmailVerified)
        {
            print("�̸����� �����ڵ带 Ȯ���� �ּ���.");
            
            verificationPanel.SetActive(true);
            verificationTxt.text = "�̸����� �����ڵ带 Ȯ���� �ּ���.";

            yield return new WaitForSeconds(3);

            verificationPanel.SetActive(false);
            verificationTxt.text = "";

            yield break;
        }

        print("�α��� �Ǿ����ϴ�.");

        uID = user.UserId;

        DownloadMyDBInfo(uID);

        loginEmailInput.text = "";
        loginPWInput.text = "";

        // �ٸ� �� �ҷ�����
        //AsyncOperation oper = SceneManager.LoadSceneAsync("MPSwithTCPClient");

        //while (!oper.isDone)
        //{
        //    print(oper.progress + "%");

        //    yield return null;
        //}

        //yield return new WaitUntil(() => oper.isDone);

        //print("Load�� �Ϸ�Ǿ����ϴ�.");
    }

    [Serializable]
    public class User
    {
        public string email;
        public string name;
    }
    [SerializeField] User userInfo;
    bool isDataDownloaded = false;
    private void DownloadMyDBInfo(string uID)
    {
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("users");

        dbRef.Child(uID).GetValueAsync().ContinueWith(task =>
        {
            DataSnapshot snapshot = task.Result;

            string json = snapshot.GetRawJsonValue();

            print(json);

            userInfo = JsonUtility.FromJson<User>(json);

            // Refactoring: �ڵ�����
            if (task.IsCanceled)
            {
                print("������ �ٿ�ε� ���");
            }
            else if (task.IsFaulted)
            {
                print("������ �ٿ�ε� ����");
            }
            else if (task.IsCompleted)
            {
                isDataDownloaded = true;

                print("������ �ٿ�ε� �Ϸ�");
            }
        });

        StartCoroutine("CoUpdateIDInfo");
    }

    IEnumerator CoUpdateIDInfo()
    {
        yield return new WaitUntil(() => isDataDownloaded);

        emailInput.text = userInfo.email;
        nameInput.text = userInfo.name;

        loginPanel.SetActive(false);
        iDInfoPanel.SetActive(true);
    }

    public void OnEditBtnClkEvent()
    {
        userInfo.email = emailInput.text;
        userInfo.name = nameInput.text;

        UploadMyDBInfo(uID, userInfo);
    }

    bool isTaskDone = false;
    private void UploadMyDBInfo(string uID, User info)
    {
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("users");

        string json = JsonUtility.ToJson(info);
        dbRef.Child(uID).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                isTaskDone = true;
            }
        });

        StartCoroutine("CoEditDone");
    }

    IEnumerator CoEditDone()
    {
        yield return new WaitUntil(() => isTaskDone);

        print("���� �Ϸ�");
    }

    public void OnSignupBtnClkEvent()
    {
        loginPanel.SetActive(false);
        signUpPanel.SetActive(true);

        loginEmailInput.text = "";
        loginPWInput.text = "";
    }

    public void OnSignupOKBtnClkEvent()
    {
        StartCoroutine(CoSignUp(signUpEmailInput.text, signUpPWInput.text, signUpPWCheckInput.text));
    }

    public void OnSignupCancelBtnClkEvent()
    {
        loginPanel.SetActive(true);
        signUpPanel.SetActive(false);

        signUpEmailInput.text = "";
        signUpPWInput.text = "";
        signUpPWCheckInput.text = "";
    }

    IEnumerator CoSignUp(string email, string password, string passwordCheck)
    {
        if(email == "" || password == "" || passwordCheck == "")
        {
            print("�̸��� �Ǵ� �н����带 �Է��� �ּ���.");

            yield break;
        }

        if (password != passwordCheck)
        {
            print("��й�ȣ�� Ȯ�κ�й�ȣ�� ���� �ʽ��ϴ�. �Է��� Ȯ���� �ּ���.");

            yield break;
        }

        Task t = auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            FirebaseException exception = task.Exception.GetBaseException() as FirebaseException;

            AuthError authError = (AuthError)exception.ErrorCode;

            switch (authError)
            {
                case AuthError.InvalidEmail:
                    print("��ȿ���� ���� �̸��� �����Դϴ�.");
                    break;
                case AuthError.WeakPassword:
                    print("��й�ȣ�� ����մϴ�.");
                    break;
                case AuthError.EmailAlreadyInUse:
                    print("�̹� ������� �̸��� �Դϴ�.");
                    break;
                default:
                    print(authError);
                    break;
            }


            if(task.IsCanceled)
            {
                print("���� ���� ���");
            }
            else if(task.IsFaulted)
            {
                print("���� ���� ����");
            }
            else if(task.IsCompletedSuccessfully)
            {
                print("ȸ�������� �Ϸ�Ǿ����ϴ�.");
                isSignupCompleted = true;
            }
            else if (task.IsCompleted)
            {
                print("ȸ�������� �Ϸ�Ǿ����ϴ�.");
                isSignupCompleted = true;
            }
        });

        yield return CoSendVerificationEmail(email, password);

        signUpEmailInput.text = "";
        signUpPWInput.text = "";
        signUpPWCheckInput.text = "";
    }

    IEnumerator CoSendVerificationEmail(string email, string password)
    {
        yield return new WaitUntil(() => signedIn);

        user = auth.CurrentUser;
        print($"Email: {user.Email}, UID: {user.UserId}, Verified: {user.IsEmailVerified}");

        var t = user.SendEmailVerificationAsync().ContinueWith(task =>
        {
            if(task.IsCompleted)
            {
                print($"{email}�� �����ڵ带 ���½��ϴ�. Ȯ���� �ּ���.");
            }
        });
        
        yield return new WaitUntil(() => t.IsCompleted);

        verificationPanel.SetActive(true);
        verificationTxt.text = $"{email}�� �����ڵ带 ���½��ϴ�. Ȯ���� �ּ���.";

        yield return new WaitForSeconds(3);

        loginPanel.SetActive(true);
        verificationPanel.SetActive(false);
        signUpPanel.SetActive(false);
    }
}