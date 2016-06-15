using UnityEngine;
using System.Collections;
using System.IO;
using Polenter.Serialization;

/*
A class for saving the player settings.
*/
public class PlayerSettings : MonoBehaviour {

    // The name of the saved data file.
    private static string playerFile = "settings.dat";

    [System.Serializable]
    private class Player
    {
        public string name { get; set; }
        public int highscore { get; set; }
        public int coins { get; set; }
        public int restarts { get; set; }
        public int shields { get; set; }
        public bool hasSeenTutorial { get; set; }

        public Player()
        {
            name = "";
            highscore = 0;
            coins = 100;
            restarts = 5;
            shields = 5;
            hasSeenTutorial = false;
        }
    }

    private static Player player = null;

    // Load the player data file or if not existent, create the default player settings and create the file.
    public static void LoadPlayer()
    {
        if (player == null)
        {
            if(File.Exists(Application.persistentDataPath + "/" + playerFile))
            {
                //Ignore if "...exists in both..." error exists: This is due to Unity.
                SharpSerializer s = new SharpSerializer(true);
                FileStream fs = File.OpenRead(Application.persistentDataPath + "/" + playerFile);
                player = (Player)s.Deserialize(fs);
                fs.Dispose();
            }
            else
            {
                player = new Player();
                SavePlayer();
            }
        }
    }

    // Saves the player settings to the data file.
    public static void SavePlayer()
    {
        //Ignore if "...exists in both..." error exists: This is due to Unity.
        SharpSerializer s = new SharpSerializer(true);
        FileStream fs = File.OpenWrite(Application.persistentDataPath + "/" + playerFile);
        s.Serialize(player, fs);
        fs.Dispose();
    }

    // Getters and Setters

    public static void SetName(string name)
    {
        if(player == null)
        {
            LoadPlayer();
        }
        player.name = name;
        SavePlayer();
    }

    public static string GetName()
    {
        if (player == null)
        {
            LoadPlayer();
        }
        return player.name;
    }

    public static void SetHighScore(int score)
    {
        if (player == null)
        {
            LoadPlayer();
        }
        player.highscore = score;
        SavePlayer();
    }

    public static int GetHighScore()
    {
        if (player == null)
        {
            LoadPlayer();
        }
        return player.highscore;
    }

    public static void AddCoins(int coins)
    {
        player.coins += coins;
        SavePlayer();
    }

    public static bool SpendCoins(int coins)
    {
        if (player.coins >= coins)
        { 
            player.coins -= coins;
            SavePlayer();
            return true;
        }
        return false;
    }

    public static int GetCoins()
    {
        if (player == null)
        {
            LoadPlayer();
        }
        return player.coins;
    }

    public static void AddShields(int shields)
    {
        if (player == null)
        {
            LoadPlayer();
        }
        player.shields += shields;
        SavePlayer();
    }

    public static bool UseShield()
    {
        if (player == null)
        {
            LoadPlayer();
        }
        if(player.shields > 0)
        {
            player.shields -= 1;
            SavePlayer();
            return true;
        }
        return false;
    }

    public static int GetShields()
    {
        if (player == null)
        {
            LoadPlayer();
        }
        return player.shields;
    }

    public static int GetRestarts()
    {
        if (player == null)
        {
            LoadPlayer();
        }
        return player.restarts;
    }

    public static void AddRestarts(int restarts)
    {
        if (player == null)
        {
            LoadPlayer();
        }
        player.restarts += restarts;
        SavePlayer();
    }

    public static bool UseRestart()
    {
        if (player == null)
        {
            LoadPlayer();
        }

        if(player.restarts > 0)
        {
            player.restarts -= 1;
            SavePlayer();
            return true;
        }

        return false;
    }

    public static bool HasSeenTutorial()
    {
        if (player == null)
        {
            LoadPlayer();
        }
        return player.hasSeenTutorial;
    }

    public static void MarkSeenTutorial()
    {
        if (player == null)
        {
            LoadPlayer();
        }
        player.hasSeenTutorial = true;
        SavePlayer();
    }
}
