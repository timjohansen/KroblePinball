using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DotMatrixDisplay : MonoBehaviour, INeedReset
{
    // This class manages the dot matrix display textures at the center of the playfield. It consists of four materials
    // that all use the DotMatrix shader, and it handles updating those material's textures (either from pre-made images 
    // or generated on the fly from strings) and playing simple animations.
    
    // The size in pixels of each texture.
    public Vector2Int scoreTextureSize;     
    public Vector2Int goalTextureSize;
    public Vector2Int timeTextureSize;
    public Vector2Int levelTextureSize;
    
    // Large digits and word textures used for the main score display
    public Texture2D[] largeDigitTexArray;
    public Texture2D largeCommaTex;
    public Texture2D largeColonTex;
    public DictCollection<Texture2D> wordTextures = new DictCollection<Texture2D>();
    
    // Small digits and word textures used for the goal and level materials
    public Texture2D[] smallDigitTexArray;
    public Texture2D smallCommaTex;
    public Texture2D smallGoalTex;
    public Texture2D smallLevelTex;
    
    
    private Queue<Message> _messageQueue = new();    // Messages waiting to be displayed. Normally a message is sent
                                                     // to the back of the queue, but if it's flagged as Override and
                                                     // the current message is flagged Overrideable, the queue will be
                                                     // cleared and the incoming message displayed immediately.
    private Message _currentMessage; 
    private int _curAnimIndex;
    private float _animationTimeRemaining = 0f;
    
    private string _prevTimeString = "";
    private Material _scoreMat;
    private Material _goalMat;
    private Material _timeMat;
    private Material _levelMat;
    private Texture2D _scoreTex;
    private Texture2D _goalTex;
    private Texture2D _timeTex;
    private Texture2D _levelTex;
    private bool _initialized;
    
    void Start()
    {
        try
        {
            List<Material> matList = new();
            GetComponent<Renderer>().GetMaterials(matList);
            if (matList.Count < 4)
            {
                Debug.LogError("DotMatrixDisplay needs at least 4 Materials");
            }
            _scoreMat = matList[0];
            _levelMat = matList[1];
            _goalMat = matList[2];
            _timeMat = matList[3];

            
            if (_scoreMat.shader.name != "Shader Graphs/DotMatrix" ||
                _goalMat.shader.name != "Shader Graphs/DotMatrix" ||
                _timeMat.shader.name != "Shader Graphs/DotMatrix" ||
                _levelMat.shader.name != "Shader Graphs/DotMatrix")
            {
                Debug.LogError("A DotMatrixDisplay material has been assigned the wrong shader");
                return;
            }
            _scoreMat.SetVector("_Dimensions", (Vector2)scoreTextureSize);
            _goalMat.SetVector("_Dimensions", (Vector2)goalTextureSize);
            _timeMat.SetVector("_Dimensions", (Vector2)timeTextureSize);
            _levelMat.SetVector("_Dimensions", (Vector2)levelTextureSize);
            
            _scoreMat.SetTexture("_DisplayTexture", _scoreTex);
            _goalMat.SetTexture("_DisplayTexture", _goalTex);
            _timeMat.SetTexture("_DisplayTexture", _timeTex);
            _levelMat.SetTexture("_DisplayTexture", _levelTex);
            
            _initialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("An unexpected error prevented DotMatrixDisplay from initializing: " + e);
            return;
        }

        ResetForNewGame();
    }

    public void ResetForNewGame()
    {
        if (!_initialized)
        {
            return;
        }
        UpdateScoreTex(0, 0f);
        UpdateTargetScoreTex(0);
        UpdateTimeTex(60f);
        UpdateLevelTex(0, 9);
        
        _scoreMat.SetVector("_TexOffset", Vector2.zero);
        _goalMat.SetVector("_TexOffset", Vector2.zero);
        _timeMat.SetVector("_TexOffset", Vector2.zero);
        _levelMat.SetVector("_TexOffset", Vector2.zero);
        
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
        
        // If there is no animation in progress, check if a new message is waiting to be played. 
        if (_animationTimeRemaining <= 0f)
        {
            if (_messageQueue.Count > 0)
            {
                StartNextMessage();
            }
            else
            {
                _scoreMat.SetTexture("_DisplayTexture", _scoreTex);
                return;    
            }
        }
        
        // Progress the currently playing animation. If it finishes this frame, start the next one.
        // If no animations are remaining, attempt to move to the next queued message. If no messages remain,
        // revert back to displaying the score.
        _animationTimeRemaining = Mathf.MoveTowards(_animationTimeRemaining, 0f, Time.deltaTime);
        if (_animationTimeRemaining <= 0f)
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
            _scoreMat.SetTexture("_DisplayTexture", _scoreTex);
            return;
        }

        DmdAnim curAnim = _currentMessage.AnimSequence[_curAnimIndex];
        
        float animTime = _animationTimeRemaining / curAnim.Duration;
        animTime %= 1f;     // For repeating messages
        animTime = 1 - animTime;
        
        int dim = (curAnim.Orientation == AnimOrient.Horizontal) ? scoreTextureSize.x : scoreTextureSize.y;

        Vector2 animVector = curAnim.Orientation == AnimOrient.Horizontal ? new Vector2(dim, 0f) : new Vector2(0f, dim);
        
        switch (curAnim.Type)
            {
                case AnimType.Hold:
                    break;
                case AnimType.ScrollIn:
                    _scoreMat.SetVector("_TexOffset", Vector2.Lerp(-animVector, Vector2.zero, animTime) );
                    break;
                case AnimType.ScrollOut:
                    _scoreMat.SetVector("_TexOffset", Vector2.Lerp(Vector2.zero, animVector, animTime));
                    break;
                case AnimType.ScrollInOut:
                    _scoreMat.SetVector("_TexOffset", Vector2.Lerp(-animVector, animVector, animTime));
                    break;
            }
    }

    public void AddMessage(Message message)
    {
        // Adds a message to the queue
        if (_currentMessage == null || (message.Override && !_currentMessage.Overridable))
        {
            _messageQueue.Clear();
        }
        _messageQueue.Enqueue(message);
        
    }
    
    private void StartNextMessage()
    {
        // Retrieves and initializes playing the next queued message, if one exists.
        if (_messageQueue.Count == 0)
        {
            _currentMessage = null;
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
        
        _scoreMat.SetVector("_TexOffset", Vector2.zero);
        
        if (prevAnim == null || prevAnim.TexString != nextAnim.TexString)
        {
            _scoreMat.SetTexture("_DisplayTexture", nextTexture);
        }
        
        _animationTimeRemaining = nextAnim.Duration * (nextAnim.RepeatTimes + 1);
    }

    public void UpdateScoreTex(int score, float percentage)
    {
        // Generates a new score texture and sets the background fill a percentage indicating the progress toward
        // the next score goal.
        if (!_scoreTex)
        {
            _scoreTex = new Texture2D(scoreTextureSize.x, scoreTextureSize.y, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
        }
        _scoreTex = NumberTextureFromString(_scoreTex, score.ToString("N0"));
        _scoreMat.SetInt("_BackgroundFill", (int)(scoreTextureSize.x * percentage));
        }

    public void UpdateTargetScoreTex(int score)
    {
        if (!_goalTex)
        {
            _goalTex = new Texture2D(goalTextureSize.x, goalTextureSize.y, TextureFormat.ARGB32, false)
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
            _levelTex = new Texture2D(levelTextureSize.x, levelTextureSize.y, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
        }
        _levelTex = CreateLevelTexture(_levelTex, level, maxLevel);
        }
    
    public void UpdateTimeTex(float time)
    {
        // Generates a time texture by converting a float time value into a clock string (e.g. 3.1415 to "3:14")
        if (!_timeTex)
        {
            _timeTex = new Texture2D(timeTextureSize.x, timeTextureSize.y, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
        }

        string timeString = "";
        timeString += ((int)time).ToString("D2") + ":";         // First two digits with leading zeros and colon (3.1415 to "03:")
        string dec = (time % 1f).ToString("F2");                // Truncated decimal value (3.1415 to "0.14")
        timeString += dec[2..4];                                      // Decimal string with the front "0." sliced off
        if (timeString == _prevTimeString)                            // Don't bother regenerating the texture if the result is the same as last frame
            return;
        
        _timeTex = NumberTextureFromString(_timeTex, timeString);
    }
    
    Texture2D NumberTextureFromString(Texture2D tex2D, string message)
    {
        // Creates a texture with a string of numbers along with a few other hardcoded character options.
        // TODO: Modify to work with the full alphanumeric character set as well as arbitrary pre-made textures
        
        if (!tex2D)
        {
            Debug.LogError("TextureFromString can't be passed a null texture parameter");
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
                totalWidth += largeCommaTex.width;
            }
            else if (message[i] == ':')
            {
                totalWidth += largeColonTex.width;
            }
            else
            {
                int index = message[i] - 48;
                if (index < 0 || index > 9)
                {
                    Debug.LogError("Invalid DMD character: " + message[i]);
                    return tex2D;
                } 
                totalWidth += largeDigitTexArray[message[i] - 48].width;
            }
        }

        int pos = tex2D.width / 2 - totalWidth / 2;
        
        for (int i = 0; i < message.Length; i++)
        {
            Texture2D letter;
            if (message[i] == ',')
            {
                letter = largeCommaTex;
            }
            else if (message[i] == ':')
            {
                letter = largeColonTex;
            }
            else
            {
                letter = largeDigitTexArray[message[i] - 48];
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
        // Generates the level texture by taking the existing "Level" texture and dropping two numbers into it.
        // Digit position values are hard-coded, but could be easily turned into parameters if needed.
        
        if (!tex2D)
        {
            Debug.LogError("CreateLevelTexture can't be passed a null texture parameter");
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
        // Generates the goal texture by creating a sequence of numbers after the pre-existing "Level:" texture.
        // This duplicates much of NumberTextureFromString and should be replaced by it eventually.
        
        if (!tex2D)
        {
            Debug.LogError("GoalTextureFromString can't be passed a null texture parameter");
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
        // This class contains all information needed to display a sequence of textures and animations, and is
        // created by other classes prior to sending a "ShowMessage" event.
        
        // TODO: possibly replace the override and overridable fields with a single priority value, and replace the
        // simple message Queue with a PriorityQueue
        
        public DmdAnim[] AnimSequence;      // Animations to be played by this message
        public bool Override;               // Should this message play immediately and cancel other messages?
        public bool Overridable;            // Should an "Override" message be able to cancel this one?

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
        public string TexString;            // The string associated with the texture in the wordTextures collection.
        public AnimType Type;               // What animation should play
        public AnimOrient Orientation;      // Horizontal or vertical 
        public float Duration;              // How long it should take to finish
        public int RepeatTimes;             // Should the animation loop? How many times?
        public TextureWrapMode TexWrapMode; // Should the texture wrap around and repeat, or be singular?

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
