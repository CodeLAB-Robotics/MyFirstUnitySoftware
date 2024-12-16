using Firebase.Database;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class BulletinBoardManager : MonoBehaviour
{
    public GameObject boardPanel;   // 게시판Panel
    public GameObject contentPanel; // 게시글Panel
    public GameObject boardGrp;     // 게시판Panel의 게시판Grp
    public GameObject buttonPrefab; // 게시글 Button

    public TMP_InputField inputField;

    /// <summary>
    /// UI 상단의 +버튼 클릭시 실행되는 이벤트 메서드
    /// </summary>
    public void OnAddBtnClkEvent()
    {
        boardPanel.SetActive(false);
        contentPanel.SetActive(true);

        Transform obj = Instantiate(buttonPrefab, boardGrp.transform).transform;
        obj.SetParent(transform);


        GameObject obj2 = Instantiate(buttonPrefab);
        obj2.transform.SetParent(transform);

        // 버튼의 자식오브젝트에서 TMP_Text 찾기
        obj2.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text = "안녕하세요.";
        obj2.transform.GetChild(1);

        for (int i = 0; i < obj2.transform.childCount; i++)
        {
            obj2.transform.GetChild(i).gameObject.SetActive(false);
        }


        //buttonPrefab.transform.GetChild(1).gameObject.SetActive(true);
    }

    public void OnExitBtnClkEvent()
    {
        Application.Quit();
    }

    public void OnPostBtnClkEvent()
    {
        OpenPost();
    }

    private void OpenPost()
    {
        
    }

    public void OnOKBtnClkEvent()
    {
        SavePost();
    }

    private void SavePost()
    {
        
    }

    public void OnCancleBtnClkEvent()
    {
        GoToMain();
    }

    private void GoToMain()
    {

    }

    public void OnDeleteBtnClkEvent()
    {
        DeletePost();
    }

    private void DeletePost()
    {

    }

    Query query;
    string totalInfo;
    public void SelectData()
    {

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("student");

        // code에 해당하는 쿼리 찾기
        query = dbRef.OrderByChild("postId").EqualTo("0");

        query.ValueChanged += OnDataLoaded;
    }

    private void OnDataLoaded(object sender, ValueChangedEventArgs args)
    {
        DataSnapshot snapshot = args.Snapshot;

        totalInfo = "";

        if (snapshot.ChildrenCount == 0)
        {
            print("해당 코드를 가진 학생 데이터 없음.");
        }
        else
        {
            // code: 0000, 0001
            foreach (var data in snapshot.Children)
            {
                IDictionary studentData = (IDictionary)data.Value;
                totalInfo += $"code: {studentData["code"]}\n";

                DataSnapshot grade = data.Child("grade");
                IDictionary gradeData = (IDictionary)grade.Value;
                totalInfo += $"grade: Englise_{gradeData["English"]}" +
                             $"/Korean_{gradeData["Korean"]}" +
                             $"/Math_{gradeData["Math"]}" +
                             $"/Science_{gradeData["Science"]}\n";

                DataSnapshot info = data.Child("info");
                IDictionary infoData = (IDictionary)info.Value;
                totalInfo += $"info: Age_{infoData["age"]}" +
                             $"/Gender_{infoData["gender"]}" +
                             $"/Name_{infoData["name"]}\n";
            }
        }

        query.ValueChanged -= OnDataLoaded;
    }
}
