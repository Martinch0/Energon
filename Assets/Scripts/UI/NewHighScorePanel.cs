using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/*
A class to handle the game over high score panel.
*/
public class NewHighScorePanel : RESTapi {

    [SerializeField] private GameObject NewHighScore;
    [SerializeField] private GameObject OldHighScore;
    [SerializeField] private InputField InputName;
    [SerializeField] private Text NewHighScoreText;
    [SerializeField] private Text CurrentScoreText;
    [SerializeField] private Text OldHighScoreText;

    // Show Panel on activation.
    public void SetActive(bool active)
    {
        if(active)
        {
            ShowPanel();
        }
        gameObject.SetActive(active);
    }

    // Check if a New highscore has been achieved or not and display the corresponding panel.
    public void ShowPanel()
    {
        int score = GameLogic.GetGameLogic().GetScore();
        int oldHighScore = PlayerSettings.GetHighScore();

        if(score > oldHighScore)
        {
            ShowNewHighScore();
        }
        else
        {
            ShowOldHighScore();
        }
    }

    // Shows the NewHighScore panel and loads its information.
    private void ShowNewHighScore()
    {
        InputName.text = PlayerSettings.GetName();
        NewHighScoreText.text = GameLogic.GetGameLogic().GetScore()+"";
        OldHighScore.SetActive(false);
        NewHighScore.SetActive(true);
    }

    // Shows the OldHighScore panel and loads its information.
    private void ShowOldHighScore()
    {
        CurrentScoreText.text = GameLogic.GetGameLogic().GetScore() + "";
        OldHighScoreText.text = PlayerSettings.GetHighScore() + "";
        OldHighScore.SetActive(true);
        NewHighScore.SetActive(false);
    }

    // Submits a new High Score to the server.
    public void SubmitNewHighscore()
    {
        string name = InputName.text;
        int score = GameLogic.GetGameLogic().GetScore();

        PlayerSettings.SetName(name);

        Dictionary<string, string> formData = new Dictionary<string, string>();
        formData.Add("uid", SystemInfo.deviceUniqueIdentifier);
        formData.Add("name", name);
        formData.Add("score", score+"");
        formData.Add("secret", secret);

        POST(server + "newHighScore", formData, OnSubmitted, OnSubmittedError);
    }

    private void OnSubmittedError()
    {

    }

    // Callback after the POST request has been completed.
    private void OnSubmitted(string result)
    {
        PlayerSettings.SetHighScore(GameLogic.GetGameLogic().GetScore());
        GameLogic.GetGameLogic().HideNewHighScore();
        GameLogic.GetGameLogic().ShowMainMenu();
    }
}
