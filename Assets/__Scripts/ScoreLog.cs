using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class ScoreLog : MonoBehaviour, INeedReset
{
    public TMP_Text tagTextObj;
    public TMP_Text scoreTextObj;
    public TMP_Text percentTextObj;
    public Image graph;
    public int xAxisMaxSeconds;
    public float yAxisMaxScore;
    private Texture2D _graphTex;
    private Sprite _graphSprite;
    private float _graphInterval;
    private float _graphIntervalTimer;
    private float _totalTime;
    private Dictionary<string, int> _taggedScores = new Dictionary<string, int>();
    private int _currentTotalScore;
    private Color[] _graphColors = new Color[]
    {
        Color.red, Color.blue, Color.green, Color.magenta, Color.yellow
    };

    public bool exportCsv;
    public float csvInterval = 10f;
    private float _csvIntervalTimer;
    Dictionary<float, Dictionary<string, int>> _csvLog = new Dictionary<float, Dictionary<string, int>>();
    List<string> _csvSeenTags = new List<string>();
    public string[] tagsToLog;
    
    List<string> _milestoneLog = new List<string>();
    private float _milestoneInterval = 30f;
    private float _milestoneIntervalTimer;
    private int _milestoneCount;

    private int _colorIndex;
    void Start()
    {
        _graphTex = new Texture2D(1024, 512);
        _graphSprite = Sprite.Create(_graphTex, new Rect(0, 0, 1024, 512), Vector2.zero);
        graph.sprite = _graphSprite;
        _graphInterval = (float)xAxisMaxSeconds / 1024;
    }

    public void ResetForNewGame()
    {
        OnApplicationQuit();
        _graphTex = new Texture2D(1024, 512);
        _graphSprite = Sprite.Create(_graphTex, new Rect(0, 0, 1024, 512), Vector2.zero);
        graph.sprite = _graphSprite;
        _totalTime = 0f;
        _graphIntervalTimer = 0f;
        _milestoneIntervalTimer = 0f;
    }
    
    void Update()
    {
        if (GM.inst.mode == GM.GameMode.Play)
        {
            _totalTime += Time.deltaTime;
            _graphIntervalTimer += Time.deltaTime;
            _csvIntervalTimer += Time.deltaTime;
            _milestoneIntervalTimer += Time.deltaTime;
            if (_graphIntervalTimer >= _graphInterval)
            {
                int x = (int)(_totalTime / xAxisMaxSeconds * 1024);
                int y = (int)((_currentTotalScore / (float)yAxisMaxScore) * 512);
                for (int i = 0; i < y; i++)
                {
                    _graphTex.SetPixel(x, i, _graphColors[_colorIndex]);
                }
                _graphTex.Apply();
                _graphIntervalTimer = 0;
            }

            if (_csvIntervalTimer >= csvInterval)
            {
                AddToCSVLog();
            }

            if (_milestoneIntervalTimer >= _milestoneInterval)
            {
                AddToMilestoneLog();
            }
        }
    }

    public void AddScore(string senderTag, int score)
    {
        if (senderTag == "")
            senderTag = "No Tag";
        if (!_taggedScores.TryAdd(senderTag, score))
        {
            _taggedScores[senderTag] += score;
        }
        _currentTotalScore += score;

        string tagText = "";
        string scoreText = "";
        string percentText = "";
        foreach (string key in _taggedScores.Keys)
        {
            tagText = tagText + "\n" + key + ": ";
            scoreText = scoreText + "\n " + _taggedScores[key].ToString();
            float percent = _taggedScores[key] / (float)_currentTotalScore * 100f;
            percentText = percentText + "\n" + percent.ToString("F1") + "%";
        }
        tagTextObj.text = tagText;
        scoreTextObj.text = scoreText;
        percentTextObj.text = percentText;
    }
    
    public void NextRound()
    {
        _colorIndex++;
        _colorIndex %= _graphColors.Length;
    }

    void AddToCSVLog()
    {

        _csvIntervalTimer = 0f;
        foreach (string key in _taggedScores.Keys)
        {
            if (!_csvSeenTags.Contains(key))
            {
                _csvSeenTags.Add(key);
                foreach (KeyValuePair<float, Dictionary<string, int>> pair in _csvLog)
                {
                    pair.Value.Add(key, 0);
                }
            }
        }
        var newTaggedScores = new Dictionary<string, int>(_taggedScores);
        _csvLog.Add(_totalTime, newTaggedScores);
    }

    void AddToMilestoneLog()
    {
        _milestoneIntervalTimer = 0f;
        _milestoneCount++;
        
        string csvText = "";
        csvText += _milestoneCount * _milestoneInterval + ",";
        csvText += _currentTotalScore + ",";

        for (var i = 0; i < tagsToLog.Length; i++)
        {
            string tag = tagsToLog[i];
            if (_taggedScores.ContainsKey(tag))
            {
                csvText += _taggedScores[tag] + ",";
                csvText += (_taggedScores[tag] / (float)_currentTotalScore * 100f).ToString("F1");
            }

            if (i != tagsToLog.Length - 1)
            {
                csvText += ",";
            }
        }
        _milestoneLog.Add(csvText);
    }

    void OnApplicationQuit()
    {
        if (!exportCsv)
            return;
        string csv = "Time,";
        for (int i = 0; i < tagsToLog.Length; i++)
        {
            csv += tagsToLog[i];
            if (i != tagsToLog.Length - 1)
            {
                csv += ",";
            }
        }
        csv += "\n";
        foreach (KeyValuePair<float, Dictionary<string, int>> pair in _csvLog)
        {
            csv += (int)pair.Key + ",";
            for (int i = 0; i < tagsToLog.Length; i++)
            {
                Dictionary<string, int> valueDict = pair.Value;
                if (valueDict.ContainsKey(tagsToLog[i]))
                {
                    csv += valueDict[tagsToLog[i]];
                }
                else
                {
                    csv += "0, 0%";
                }
                
                if (i < tagsToLog.Length - 1)
                {
                    csv += ",";
                }
                
            }
            csv += "\n";
        }
        string datetime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string filename = "scores_" + datetime + ".csv";
        if (!System.IO.File.Exists(filename))
        {
            System.IO.File.WriteAllText(filename, csv);    
        }

        // Milestone log
        csv = "Time,TotalScore,";
        for (int i = 0; i < tagsToLog.Length; i++)
        {
            csv += tagsToLog[i] + ",";
            csv += tagsToLog[i] + "%";
            if (i < tagsToLog.Length - 1)
            {
                csv += ",";
            }
        }
        csv += "\n";

        foreach (string entry in _milestoneLog)
        {
            csv += entry + "\n";
        }

        
        datetime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        filename = "milestones_" + datetime + ".csv";
        if (!System.IO.File.Exists(filename))
        {
            System.IO.File.WriteAllText(filename, csv);    
        }
    }
}
