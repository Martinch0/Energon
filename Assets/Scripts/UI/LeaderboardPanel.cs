using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/*
A class to load the data into the Leaderboard panel.
*/
public class LeaderboardPanel : RESTapi {

    [SerializeField] private Text LeaderBoardNames;
    [SerializeField] private Text LeaderBoardScores;

    [System.Serializable]
    public class LeaderboardRecord
    {
        public bool isMine;
        public string uid;
        public int position;
        public string name;
        public int score;

        public static LeaderboardRecord CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<LeaderboardRecord>(jsonString);
        }
    }

    // Update on activation.
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        if (active)
        {
            UpdateData();
        }
    }

    // Reloads the data from the server.
    public void UpdateData()
    {
        GET(server + "getHighScores/" + SystemInfo.deviceUniqueIdentifier, OnDataRecieved, OnDataError);
    }

    private void OnDataError()
    {
        LeaderBoardNames.text = "Error loading data.\nTry again later.";
    }

    // Parses the recieved JSON into the leaderboard.
    private void OnDataRecieved(string json)
    {
        string names = "";
        string scores = "";
        JSONObject jsonObject = new JSONObject(json);

        List<LeaderboardRecord> leaderboard = LeaderboardRecordListFromJSON(jsonObject);

        int i = 0;
        bool hasHighscore = false;
        bool inTopTen = false;

        if(leaderboard.Count > 0 && leaderboard[i].isMine)
        {
            i++;
            hasHighscore = true;
        }

        for(; i < leaderboard.Count; i++)
        {
            if(hasHighscore && leaderboard[i].uid.Equals(leaderboard[0].uid))
            {
                // If my record is in top ten, make it BOLD.
                inTopTen = true;
                names += "<b>" + leaderboard[i].position + ". " + leaderboard[i].name + "</b>\n";
                scores += "<b>" + leaderboard[i].score + "</b>\n";
            }
            else
            {
                // Else - it is not my record
                names += leaderboard[i].position + ". " + leaderboard[i].name + "\n";
                scores += leaderboard[i].score + "\n";
            }
        }

        if(leaderboard.Count > 0 && !inTopTen && leaderboard[0].isMine)
        {
            // If my record was not in top ten, add me at the bottom.
            names += "...\n<b>" + leaderboard[0].position + ". " + leaderboard[0].name + "</b>";
            scores += "...\n<b>" + leaderboard[0].score + "</b>";
        }

        LeaderBoardScores.text = scores;
        LeaderBoardNames.text = names;
    }

    // Parses the jsonObject and returns a list of LeaderBoardRecords
    private List<LeaderboardRecord> LeaderboardRecordListFromJSON(JSONObject jsonObject)
    {
        List<LeaderboardRecord> leaderboard = new List<LeaderboardRecord>();
        if (jsonObject.type == JSONObject.Type.ARRAY)
        {
            foreach (JSONObject j in jsonObject.list)
            {
                LeaderboardRecord record = new LeaderboardRecord();
                for (int i = 0; i < j.list.Count; i++)
                {
                    string key = (string)j.keys[i];
                    JSONObject val = (JSONObject)j.list[i];
                    switch (key)
                    {
                        case "position":
                            record.position = (int)val.n;
                            break;
                        case "uid":
                            record.uid = val.str;
                            break;
                        case "isMine":
                            record.isMine = ((int)val.n) == 1;
                            break;
                        case "score":
                            record.score = (int)val.n;
                            break;
                        case "name":
                            record.name = val.str;
                            break;
                        default:
                            break;
                    }
                }
                leaderboard.Add(record);
            }
        }
        return leaderboard;
    }
}
