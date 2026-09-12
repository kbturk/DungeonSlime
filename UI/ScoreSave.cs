using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace DungeonSlime.UI;

///<summary>
///A user's name and score
///</summary>
public struct HighScoreEntry
{
    ///<summary>
    ///User's name
    ///</summary>
    public string name;

    ///<summary>
    ///User's score after game over
    ///</summary>
    public int score;
}

///<summary>
///A list of high scores with names
///</summary>
[System.Serializable]
public class Leaderboard
{
    ///<summary>
    ///List of high scores
    ///</summary>
    public List<HighScoreEntry> list = new List<HighScoreEntry>();
}

///<summary>
///Manages High Scores across game instances.
///</summary>
public class ScoreManager
{
    ///<summary>
    ///the scoremanager's list of playerscores
    ///</summary>
    public Leaderboard playerScores;

    ///<summary>
    ///Filepath
    ///</summary>
    private string path;


    ///<summary>
    ///the scoremanager class
    ///</summary>
    public ScoreManager()
    {
        //TODO: in the future, this will be a collection of 
        //strings and scores(int)
        playerScores = new Leaderboard();
    }

    ///<summary>
    /// Load the existing high score list on Load Content
    ///</summary>
    public void LoadContent()
    {
        path = Directory.GetCurrentDirectory();
        Console.WriteLine(path);
        //create a directory if the high score directory doesn't exist.
        if (!Directory.Exists(path + "/HighScores/"))
        {
            Directory.CreateDirectory(path + "/HighScores/");
            Console.WriteLine("Made Directory!");
        }

        if (File.Exists(path +"/HighScores/highscores.xml"))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Leaderboard));

            //If the XML document has been altered with unknown nodes or attributes
            //handle them with the UnknownNode and UnknownAttribute events.
            serializer.UnknownNode+= new
                XmlNodeEventHandler(serializer_UnknownNode);
            serializer.UnknownAttribute+= new
                XmlAttributeEventHandler(serializer_UnknownAttribute);

            FileStream stream = new FileStream(path + "/HighScores/highscores.xml", FileMode.Open);
            playerScores = (Leaderboard) serializer.Deserialize(stream);
            stream.Close();
        }
        else
        {
            AddNewScore("mom", 1000);
        }
    }

    ///<summary>
    /// Add a new score to the list after a game over
    ///</summary>
    public void AddNewScore(string entryName, int entryScore)
    {
        playerScores.list.Add(new HighScoreEntry {name = entryName, score = entryScore});
        playerScores.list.Sort((HighScoreEntry x, HighScoreEntry y) => y.score.CompareTo(x.score));
    }

    ///<summary>
    ///After a game is over, we save the score out to an xml file.
    ///</summary>
    public void SaveScores()
    {
        //TODO: Only save top 10.
        XmlSerializer serializer = new XmlSerializer(typeof(Leaderboard));
        TextWriter stream = new StreamWriter(path + "/HighScores/highscores.xml");
        serializer.Serialize(stream, playerScores);
        stream.Close();
    }

    private void serializer_UnknownNode
        (object sender, XmlNodeEventArgs e)
        {
            Console.WriteLine("Unknown Node:" +   e.Name + "\t" + e.Text);
        }

    private void serializer_UnknownAttribute
        (object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            Console.WriteLine("Unknown attribute " +
                    attr.Name + "='" + attr.Value + "'");
        }
}
