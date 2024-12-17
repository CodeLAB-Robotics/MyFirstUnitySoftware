using TMPro;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Collections;
using System.Threading.Tasks;
using System;
using System.Net.Mail;
using UnityEngine.SceneManagement;

/// <summary>
/// 1. 로그인: 이메일, 패스워드 입력시 회원가입 여부에 따라 로그인한다.
/// 2. 회원가입: 이메일, 패스워드 입력 후 이메일 인증이 완료된다면 회원가입이 된다.
/// 3. 데이터 불러오기: 권한에 따라 DB의 특정 정보를 불러온다.
/// </summary>
public class FirebaseAuthManager : MonoBehaviour
{
    [Header("로그인 UI")]
    [SerializeField] GameObject loginPanel;
    [SerializeField] TMP_InputField loginEmailInput;
    [SerializeField] TMP_InputField loginPWInput;

    [Header("회원가입 UI")]
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
        // 로그인 완료시 다른 씬으로 넘어가기
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
                print("로그인 취소");
            }
            else if (t.IsFaulted)
            {
                print("로그인 실패");
            }
            else if(t.IsCompleted)
            {
                print("로그인 확인(이메일 인증 확인 필요");
            }
        });

        yield return new WaitUntil(() => logInTask.IsCompleted);

        user = logInTask.Result.User;

        if (!user.IsEmailVerified)
        {
            print("이메일의 인증코드를 확인해 주세요.");
            
            verificationPanel.SetActive(true);
            verificationTxt.text = "이메일의 인증코드를 확인해 주세요.";

            yield return new WaitForSeconds(3);

            verificationPanel.SetActive(false);
            verificationTxt.text = "";

            yield break;
        }

        print("로그인 되었습니다.");

        loginEmailInput.text = "";
        loginPWInput.text = "";

        // 다른 씬 불러오기
        AsyncOperation oper = SceneManager.LoadSceneAsync("MPSwithTCPClient");

        while(!oper.isDone)
        {
            print(oper.progress + "%");

            yield return null;
        }
        
        yield return new WaitUntil(() => oper.isDone);

        print("Load가 완료되었습니다.");
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
            print("이메일 또는 패스워드를 입력해 주세요.");

            yield break;
        }

        if (password != passwordCheck)
        {
            print("비밀번호와 확인비밀번호가 같지 않습니다. 입력을 확인해 주세요.");

            yield break;
        }

        Task t = auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            FirebaseException exception = task.Exception.GetBaseException() as FirebaseException;

            AuthError authError = (AuthError)exception.ErrorCode;

            switch (authError)
            {
                case AuthError.InvalidEmail:
                    print("유효하지 않은 이메일 형식입니다.");
                    break;
                case AuthError.WeakPassword:
                    print("비밀번호가 취약합니다.");
                    break;
                case AuthError.EmailAlreadyInUse:
                    print("이미 사용중인 이메일 입니다.");
                    break;
                default:
                    print(authError);
                    break;
            }


            if(task.IsCanceled)
            {
                print("유저 생성 취소");
            }
            else if(task.IsFaulted)
            {
                print("유저 생성 실패");
            }
            else if(task.IsCompletedSuccessfully)
            {
                print("회원가입이 완료되었습니다.");
                isSignupCompleted = true;
            }
            else if (task.IsCompleted)
            {
                print("회원가입이 완료되었습니다.");
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
                print($"{email}로 인증코드를 보냈습니다. 확인해 주세요.");
            }
        });
        
        yield return new WaitUntil(() => t.IsCompleted);

        verificationPanel.SetActive(true);
        verificationTxt.text = $"{email}로 인증코드를 보냈습니다. 확인해 주세요.";

        yield return new WaitForSeconds(3);

        loginPanel.SetActive(true);
        verificationPanel.SetActive(false);
        signUpPanel.SetActive(false);
    }
}
