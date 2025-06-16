using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INeedReset
{
    // The GM will search for every object implementing this interface and run ResetForNewGame
    // whenever a new game is started.
    
    public void ResetForNewGame();

}
