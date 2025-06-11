using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultLetters : ToggleableLetters
{
    protected override void AllLit()
    {
        base.AllLit();
        boardEvent.Invoke(new EventInfo(this, EventType.AddBallMult));
        
        DotMatrixDisplay.Message message = new DotMatrixDisplay.Message(
            new DotMatrixDisplay.DmdAnim[]
            {
                new (DotMatrixDisplay.AnimType.ScrollIn, 
                    DotMatrixDisplay.AnimOrient.Vertical,
                    .25f, 0, "Mult1.5x", TextureWrapMode.Clamp),
                new (DotMatrixDisplay.AnimType.Hold, 
                    DotMatrixDisplay.AnimOrient.Horizontal,
                    1f, 0, "Mult1.5x", TextureWrapMode.Clamp),
                

            }
        );
        boardEvent.Invoke(new EventInfo(this, EventType.ShowMessage, message));
    }
}
