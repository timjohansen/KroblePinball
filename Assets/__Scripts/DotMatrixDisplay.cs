using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotMatrixDisplay : MonoBehaviour, INeedReset
{
    // public GameObject scoreObj;
    // public GameObject timeObj;
    public int scoreTexWidth;
    public int scoreTexHeight;
    public int goalTexWidth;
    public int goalTexHeight;
    public int timeTexWidth;
    public int timeTexHeight;
    public int levelTexWidth;
    public int levelTexHeight;
    
    public Texture2D[] digitTexArray;
    
    public Texture2D commaTex;
    public Texture2D colonTex;
    public Texture2D[] smallDigitTexArray;
    public Texture2D smallCommaTex;
    public Texture2D smallGoalTex;
    public Texture2D smallLevelTex;
    public DictCollection<Texture2D> wordTextures = new DictCollection<Texture2D>();
    
    private bool _initialized;
    private Material _scoreMat;
    private Material _goalMat;
    private Material _timeMat;
    private Material _levelMat;
    private Texture2D _scoreTex;
    private Texture2D _goalTex;
    private Texture2D _timeTex;
    private Texture2D _levelTex;
    private Queue<Message> _messageQueue = new Queue<Message>();
    private Message _currentMessage;
    private int _curAnimIndex;
    private float _animationTimeRemaining = 0f;
    
    private string _prevTimeString = "";
    
    void Start()
    {
        try
        {
            List<Material> matList = new();
            GetComponent<Renderer>().GetMaterials(matList);
            _scoreMat = matList[0];
            _levelMat = matList[1];
            _goalMat = matList[2];
            _timeMat = matList[3];

            _scoreMat.SetInt("_X_Mult", scoreTexWidth);
            _scoreMat.SetInt("_Y_Mult", scoreTexHeight);
            _goalMat.SetInt("_X_Mult", goalTexWidth);
            _goalMat.SetInt("_Y_Mult", goalTexHeight);
            _timeMat.SetInt("_X_Mult", timeTexWidth);
            _timeMat.SetInt("_Y_Mult", timeTexHeight);
            _levelMat.SetInt("_X_Mult", levelTexWidth);
            _levelMat.SetInt("_Y_Mult", levelTexHeight);
            
            _initialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to initialize DotMatrixDisplay: " + e);
            return;
        }

        ResetForNewGame();
    }

    public void ResetForNewGame()
    {
        UpdateScoreTex(0, 0f);
        UpdateTargetScoreTex(0);
        UpdateTimeTex(60f);
        UpdateLevelTex(0, 9);
        _scoreMat.SetTexture("_DisplayTexture", _scoreTex);
        _goalMat.SetTexture("_DisplayTexture", _goalTex);
        _timeMat.SetTexture("_DisplayTexture", _timeTex);
        _levelMat.SetTexture("_DisplayTexture", _levelTex);
    }

    void Update()
    {
        if (!_initialized)
        {
            return;
        }
        // if (GM.inst.mode != GM.GameMode.Shop)
        // {
        //     return;
        // }
        
        if (_animationTimeRemaining <= 0f)
        {
            if (_messageQueue.Count > 0)
            {
                StartNextMessage();
            }
            else
            {
                return;    
            }
        }
        
        _animationTimeRemaining -= Time.deltaTime;
        if (_animationTimeRemaining <= 0)
        {
            _curAnimIndex++;
            if (_curAnimIndex < _currentMessage.AnimSequence.Length)
                StartAnimation();
            else
            {
                StartNextMessage();
            }

        }

        if (_currentMessage == null)
        {
            return;
        }

        DmdAnim curAnim = _currentMessage.AnimSequence[_curAnimIndex];
        float animTime = _animationTimeRemaining / curAnim.Duration;
        // animTime *= curAnim.RepeatTimes + 1;
        animTime %= 1f;
        animTime = 1 - animTime;
        
        int dim = (curAnim.Orientation == AnimOrient.Horizontal) ? scoreTexWidth : scoreTexHeight;
        string mainPropName = (curAnim.Orientation == AnimOrient.Horizontal) ? "_X_Pos" : "_Y_Pos";

        switch (curAnim.Type)
            {
                case AnimType.Hold:
                    break;
                case AnimType.ScrollIn:
                    _scoreMat.SetFloat(mainPropName, Mathf.Lerp(-dim, 0f, animTime));
                    break;
                case AnimType.ScrollOut:
                    _scoreMat.SetFloat(mainPropName, Mathf.Lerp(0f, dim, animTime));
                    break;
                case AnimType.ScrollInOut:
                    _scoreMat.SetFloat(mainPropName, Mathf.Lerp(-dim, dim, animTime));
                    break;
            }
    }

    public void AddMessage(Message message)
    {
        if (_currentMessage == null || (message.Override && !_currentMessage.Overridable))
        {
            _messageQueue.Clear();
        }
        _messageQueue.Enqueue(message);
        
    }
    
    private void StartNextMessage()
    {
        if (_messageQueue.Count == 0)
        {
            _currentMessage = null;
            _scoreMat.SetTexture("_DisplayTexture", _scoreTex);
            return;
        }
        _currentMessage = _messageQueue.Dequeue();
        _curAnimIndex = 0;
        
        StartAnimation();
    }

    private void StartAnimation()
    {
        if (_curAnimIndex == _currentMessage.AnimSequence.Length)
        {
            Debug.LogWarning("Animation sequence length exceeded, this function shouldn't have been called");
            return;
        }
        DmdAnim prevAnim = null;
        if (_curAnimIndex > 0)
        {
            prevAnim = _currentMessage.AnimSequence[_curAnimIndex - 1];
        }
        DmdAnim nextAnim = _currentMessage.AnimSequence[_curAnimIndex];

        Texture2D nextTexture = wordTextures.Get(nextAnim.TexString).obj;
        nextTexture.wrapMode = nextAnim.TexWrapMode;            
        
        _scoreMat.SetFloat("_X_Pos", 0f);
        _scoreMat.SetFloat("_Y_Pos", 0f);
        
        if (prevAnim == null || prevAnim.TexString != nextAnim.TexString)
        {
            _scoreMat.SetTexture("_DisplayTexture", nextTexture);
        }

        
        
        _animationTimeRemaining = nextAnim.Duration * (nextAnim.RepeatTimes + 1);
    }

    public void UpdateScoreTex(int score, float percentage)
    {
        if (!_scoreTex)
        {
            _scoreTex = new Texture2D(scoreTexWidth, scoreTexHeight, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
        }
        _scoreTex = TextureFromString(_scoreTex, score.ToString("N0"));
        _scoreMat.SetInt("_BackgroundFill", (int)(scoreTexWidth * percentage));
    }

    public void UpdateTargetScoreTex(int score)
    {
        if (!_goalTex)
        {
            _goalTex = new Texture2D(goalTexWidth, goalTexHeight, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
        }
        _goalTex = GoalTextureFromString(_goalTex, score.ToString("N0"));
    }
    
    public void UpdateLevelTex(int level, int maxLevel)
    {
        if (!_levelTex)
        {
            _levelTex = new Texture2D(levelTexWidth, levelTexHeight, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
        }
        _levelTex = CreateLevelTexture(_levelTex, level, maxLevel);
    }
    
    public void UpdateTimeTex(float time)
    {
        if (!_timeTex)
        {
            _timeTex = new Texture2D(timeTexWidth, timeTexHeight, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
        }

        string timeString = "";
        timeString += ((int)time).ToString("D2") + ":";
        string dec = (time % 1f).ToString("F2");
        timeString += dec[2..4];
        if (timeString == _prevTimeString)
            return;
        
        _timeTex = TextureFromString(_timeTex, timeString);
    }
    
    Texture2D TextureFromString(Texture2D tex2D, string message)
    {
        if (!tex2D)
        {
            Debug.LogError("Null texture");
            return null;
        }
        Color backColor = Color.black;
        Color foregroundColor = Color.white;

        for (int y = 0; y < tex2D.height; y++)
        {
            for (int x = 0; x < tex2D.width; x++)
            {
                tex2D.SetPixel(x,y, backColor);   
            }
        }
        
        int totalWidth = 0;
        for (int i = 0; i < message.Length; i++)
        {
            if (message[i] == ',')
            {
                totalWidth += commaTex.width;
            }
            else if (message[i] == ':')
            {
                totalWidth += colonTex.width;
            }
            else
            {
                int index = message[i] - 48;
                if (index < 0 || index > 9)
                {
                    Debug.LogError("Invalid DMD character: " + message[i]);
                    return tex2D;
                } 
                totalWidth += digitTexArray[message[i] - 48].width;
            }
        }

        int pos = tex2D.width / 2 - totalWidth / 2;
        
        for (int i = 0; i < message.Length; i++)
        {
            Texture2D letter;
            if (message[i] == ',')
            {
                letter = commaTex;
            }
            else if (message[i] == ':')
            {
                letter = colonTex;
            }
            else
            {
                letter = digitTexArray[message[i] - 48];
            }
            
            for (int y = 0; y < letter.height; y++)
            {
                if (y >= tex2D.height)
                {
                    continue;
                }
                for (int x = 0; x < letter.width; x++)
                {
                    if (letter.GetPixel(x, y) == Color.white)
                        tex2D.SetPixel(x + pos, y, foregroundColor);
                }
            }
            pos += letter.width;
        }
        tex2D.Apply();
        return tex2D;
    }    
    
    Texture2D CreateLevelTexture(Texture2D tex2D, int level, int maxLevel)
    {
        if (!tex2D)
        {
            Debug.LogError("Null texture");
            return null;
        }
        Color backColor = Color.black;
        Color foregroundColor = Color.white;

        for (int y = 0; y < tex2D.height; y++)
        {
            for (int x = 0; x < tex2D.width; x++)
            {
                tex2D.SetPixel(x,y, backColor);   
            }
        }
        
        int totalWidth = smallLevelTex.width;
        int pos = tex2D.width / 2 - totalWidth / 2;
        
        for (int y = 0; y < smallLevelTex.height; y++)
        {
            if (y >= tex2D.height)
            {
                continue;
            }
            for (int x = 0; x < smallLevelTex.width; x++)
            {
                if (smallLevelTex.GetPixel(x, y) == Color.white)
                {
                    tex2D.SetPixel(x + pos, y, foregroundColor);
                }
            }
        }

        pos += 38;
        Texture2D number = smallDigitTexArray[level];
        for (int y = 0; y < number.height; y++)
        {
            if (y >= tex2D.height)
            {
                continue;
            }
            for (int x = 0; x < number.width; x++)
            {
                if (number.GetPixel(x, y) == Color.white)
                    tex2D.SetPixel(x + pos, y, foregroundColor);
            }
        }
        
        pos += 18;
        number = smallDigitTexArray[maxLevel];
        for (int y = 0; y < number.height; y++)
        {
            if (y >= tex2D.height)
            {
                continue;
            }
            for (int x = 0; x < number.width; x++)
            {
                if (number.GetPixel(x, y) == Color.white)
                    tex2D.SetPixel(x + pos, y, foregroundColor);
            }
        }
        
        tex2D.Apply();
        return tex2D;
    }    
    
    Texture2D GoalTextureFromString(Texture2D tex2D, string message)
    {
        if (!tex2D)
        {
            Debug.LogError("Null texture");
            return null;
        }
        Color backColor = Color.black;
        Color foregroundColor = Color.white;

        for (int y = 0; y < tex2D.height; y++)
        {
            for (int x = 0; x < tex2D.width; x++)
            {
                tex2D.SetPixel(x,y, backColor);   
            }
        }
        
        int totalWidth = smallGoalTex.width;
        
        for (int i = 0; i < message.Length; i++)
        {
            if (message[i] == ',')
            {
                totalWidth += smallCommaTex.width;
            }
            else
            {
                int index = message[i] - 48;
                if (index < 0 || index > 9)
                {
                    Debug.LogError("Invalid DMD character: " + message[i]);
                    return tex2D;
                } 
                totalWidth += smallDigitTexArray[message[i] - 48].width;
            }
        }

        int pos = tex2D.width / 2 - totalWidth / 2;
        
        for (int y = 0; y < smallGoalTex.height; y++)
        {
            if (y >= tex2D.height)
            {
                continue;
            }
            for (int x = 0; x < smallGoalTex.width; x++)
            {
                if (smallGoalTex.GetPixel(x, y) == Color.white)
                {
                    tex2D.SetPixel(x + pos, y, foregroundColor);
                }
            }
        }
        pos += smallGoalTex.width;
        
        
        for (int i = 0; i < message.Length; i++)
        {
            Texture2D letter;
            if (message[i] == ',')
            {
                letter = smallCommaTex;
            }
            else
            {
                letter = smallDigitTexArray[message[i] - 48];
            }
            
            for (int y = 0; y < letter.height; y++)
            {
                if (y >= tex2D.height)
                {
                    continue;
                }
                for (int x = 0; x < letter.width; x++)
                {
                    if (letter.GetPixel(x, y) == Color.white)
                        tex2D.SetPixel(x + pos, y, foregroundColor);
                }
            }
            pos += letter.width;
        }
        tex2D.Apply();
        return tex2D;
    }    
    
    
    public class Message
    {
        public DmdAnim[] AnimSequence;
        public bool Override;
        public bool Overridable;

        public Message(DmdAnim[] animSequence)
        {
            this.Override = false;
            this.Overridable = true;
            this.AnimSequence = animSequence;
        }
        
        public Message(DmdAnim[] animSequence, bool overRide, bool overridable)
        {
            this.AnimSequence = animSequence;
            this.Override = overRide;
            this.Overridable = overridable;
        }
    }

    public class DmdAnim
    {
        public string TexString;
        public AnimType Type;
        public AnimOrient Orientation;
        public float Duration;
        public int RepeatTimes;
        public TextureWrapMode TexWrapMode;

        public DmdAnim(AnimType type, AnimOrient orientation, float duration, int repeatTimes, string texString, TextureWrapMode texWrapMode)
        {
            TexString = texString;
            Type = type;
            Orientation = orientation;
            Duration = duration;
            RepeatTimes = repeatTimes;
            TexWrapMode = texWrapMode;
        }
    }
    
    public enum AnimType
    {
        ScrollIn, ScrollOut, ScrollInOut, Hold
    }

    public enum AnimOrient
    {
        Horizontal, Vertical
    }
    
}
