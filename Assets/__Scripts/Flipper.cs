using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flipper : MonoBehaviour
{
    ObjectLink objLink;
    FlipperPre flipPre;
    
    public BoxCollider2D boxCol;
    public CircleCollider2D circleCol1;
    public CircleCollider2D circleCol2;
    
    // Object for collision checking and its associated colliders
    GameObject testObj;
    BoxCollider2D testBoxCol;
    CircleCollider2D testCircleCol1;
    CircleCollider2D testCircleCol2;
    
    List<GameObject> kinematicBalls = new();
    List<GameObject> ballsAlreadyHit = new();
    

    List<FlipVectors> fVecs;

    
    void Awake()
    {
        objLink = GetComponent<ObjectLink>();
        flipPre = GetComponent<FlipperPre>();
        testObj = new GameObject();
        testObj.layer = 3;
        testObj.transform.position = objLink.obj2D.transform.position;
        testObj.transform.localRotation = objLink.obj2D.transform.localRotation;
        
        testBoxCol = testObj.AddComponent<BoxCollider2D>();
        testBoxCol.offset = boxCol.offset;
        testBoxCol.size = boxCol.size;
        
        testCircleCol1 = testObj.AddComponent<CircleCollider2D>();
        testCircleCol1.offset = circleCol1.offset;
        testCircleCol1.radius = circleCol1.radius;
        
        testCircleCol2 = testObj.AddComponent<CircleCollider2D>();
        testCircleCol2.offset = circleCol2.offset;
        testCircleCol2.radius = circleCol2.radius;

        Rigidbody2D rb = testObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        fVecs = new List<FlipVectors>();
        float sign = (flipPre.rightSide) ? 1f : -1f; 
        float offset = objLink.sharedRotation.y;
        FlipVectors fv = new FlipVectors(0f);
        fv.AddVector(.035f, AngleToVector(29f * sign + offset));
        fv.AddVector(.12f, AngleToVector(17f * sign + offset));
        fv.AddVector(.373f, AngleToVector(-17f * sign + offset));
        fv.AddVector(.42f, AngleToVector(-60f * sign + offset));
        fVecs.Add(fv);

        fv = new FlipVectors(1f);
        fv.AddVector(.035f, AngleToVector(65f * sign + offset));
        fv.AddVector(.12f, AngleToVector(55f * sign + offset));
        fv.AddVector(.373f, AngleToVector(55f * sign + offset));
        fv.AddVector(.42f, AngleToVector(29f * sign + offset));
        fVecs.Add(fv);
    }

    void Start()
    {
        GM.inst.ballDispenser.GetBallDespawnEvent().AddListener(OnBallDespawn);
    }
    
    void FixedUpdate()
    {
        float rotateValue = flipPre.rotateValue;
        float prevAngle = flipPre.prevAngle;
        float nextAngle = flipPre.angle;
        bool swingingUp = flipPre.swingingUp;
        
        testObj.GetComponent<Rigidbody2D>().rotation = nextAngle + objLink.baseRotation2D;
        
        if (swingingUp)
        {
            // Check all balls for collision next frame.
            foreach (GameObject ball in GM.inst.ballDispenser.GetActiveBalls())
            {
                Ball ballComp = ball.GetComponent<Ball>();
                Rigidbody2D rb = ballComp.ball2D.GetComponent<Rigidbody2D>();                
                
                
                // float radius = ball.GetComponent<CircleCollider2D>().radius;
                // Vector2 nextPos = rb.position + rb.velocity * Time.fixedDeltaTime;
                // Vector2 direction = (rb.position - nextPos).normalized;
                // float distance = (rb.position - nextPos).magnitude;
                // float deltaAngle = (prevAngle - nextAngle) * Mathf.Deg2Rad;
                
                RaycastHit2D hit = CheckForFlipperHit(ballComp, rb.position, rb.velocity, prevAngle, nextAngle);
                if (hit.collider && hit.collider.gameObject == testObj)
                {                    
                    Vector2 nextVelocity = CalcHitVelocity(hit, ballComp, prevAngle, nextAngle);                    
                    rb.velocity += nextVelocity;
                }
                else
                {
                    if (kinematicBalls.Contains(ball))
                    {
                        kinematicBalls.Remove(ball);                        
                        Ball.StoredBallInfo stored = ballComp.MakeDynamic();
                        rb.velocity += stored.velocity;
                    }
                }
            }
        }
        else    // Not swinging up
        {
            foreach (GameObject ball in kinematicBalls)
            {                
                Ball ballComp = ball.GetComponent<Ball>();
                Rigidbody2D rb = ballComp.ball2D.GetComponent<Rigidbody2D>();
                Ball.StoredBallInfo stored = ballComp.MakeDynamic();
                
                rb.velocity += stored.velocity;
            }
            kinematicBalls = new List<GameObject>();
            ballsAlreadyHit = new List<GameObject>();
        }
        
        objLink.RotateToAngle(flipPre.angle);
        return;

        // LOCAL FUNCTIONS

        RaycastHit2D CheckForFlipperHit(Ball ball, Vector2 ballPos, Vector2 ballV, float flipperPrevAngle, float flipperNextAngle)
        {
            testObj.GetComponent<Rigidbody2D>().rotation = flipperNextAngle + objLink.baseRotation2D;

            // Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            float radius = ball.ball2D.GetComponent<CircleCollider2D>().radius;
            Vector2 nextPos = ballPos + ballV * Time.fixedDeltaTime;
            Vector2 direction = (ballPos - nextPos).normalized;
            float distance = (ballPos - nextPos).magnitude;
            return Physics2D.CircleCast(ballPos, radius, direction, distance, LayerMask.GetMask("FlipperTest"));
        }
        

        Vector2 CalcHitVelocity(RaycastHit2D hit, Ball ball, float flipperPrevAngle, float flipperNextAngle, bool canSetKinematic = true)
        {
            Rigidbody2D rb = ball.ball2D.GetComponent<Rigidbody2D>();

            // Find the surface point where the ball actually impacted
            RaycastHit2D outHit = Physics2D.Raycast(hit.point + hit.normal * ball.radius * 2f, -hit.normal, ball.radius * 2.1f, LayerMask.GetMask("FlipperTest"));
            // Debug.DrawRay(hit.point + hit.normal * ball.radius * 2f, -hit.normal * ball.radius * 2.1f, Color.red, 2f);
            // Debug.DrawRay(hit.point + hit.normal * ball.radius * 2f, -hit.normal * ball.radius * .5f, Color.green, 2f);
            
            if (!outHit.collider)
            {
                Debug.LogError("No collider detected from outside cast");
                Debug.Break();
            }
            Vector2 surfacePoint = outHit.point;
            // Debug.DrawLine(surfacePoint, hit.point, Color.red, 2f);

            // Calculate the velocity needed to eject it from the flipper interior, since simply moving the ball's position causes jitter.
            Vector2 ejectVelocity = hit.point - (surfacePoint - hit.normal * ball.radius);
            // Debug.DrawRay(hit.point, ejectVelocity, Color.green, 2f);

            // If the ball was already hit, stop here. Otherwise, calculate how much velocity should be added to the ball based on where and when it hit.
            // Theoretically, if the ball was already hit, it would be flying away rather than impacting a second time, but this adds safety for any edge cases.
            if (ballsAlreadyHit.Contains(ball.gameObject))
            {
                return ejectVelocity * (1f / Time.fixedDeltaTime);
            }
            else
            {
                ballsAlreadyHit.Add(ball.gameObject);
            }
            
            if (canSetKinematic && rb.bodyType != RigidbodyType2D.Kinematic)
            {
                ball.MakeKinematic(rotateValue, surfacePoint);
                kinematicBalls.Add(rb.transform.parent.gameObject);
            }
            
            float deltaAngle = (flipperPrevAngle - flipperNextAngle) * Mathf.Deg2Rad;

            Vector2 pivotPos = objLink.obj2D.GetComponent<Rigidbody2D>().position;                    
            Vector2 pivotVector = surfacePoint - pivotPos;            

            Vector2 rotatedPivotVector = new()
            {
                x = pivotVector.x * Mathf.Cos(deltaAngle) - pivotVector.y * Mathf.Sin(deltaAngle),
                y = pivotVector.x * Mathf.Sin(deltaAngle) + pivotVector.y * Mathf.Cos(deltaAngle)
            };

            Vector2 rotatedPoint = pivotPos + rotatedPivotVector;

            Vector2 flipperV = surfacePoint - rotatedPoint;     // The velocity of the flipper impacting the ball.
            Ball.StoredBallInfo stored = ball.storedInfo;
            Vector2 hitVector = CalcHitAngle(stored);
            
            // EXPERIMENTAL
            float swingMult = Mathf.Lerp(2f, .5f, flipPre.rotateValue);
            
            hitVector = hitVector.normalized * (flipperV.magnitude * swingMult);
            // hitVector = hitVector.normalized * (flipperV.magnitude * 2f);

            Debug.DrawRay(surfacePoint, hitVector * 2f, Color.magenta,2f);

            return (hitVector + ejectVelocity) * (1f / Time.fixedDeltaTime);   
        }
    }

    Vector2 CalcHitAngle(Ball.StoredBallInfo stored)
        {
            float firstFlipVal = 0f;
            int firstFlipIndex = 0;
            float secondFlipVal = 0f;
            int secondFlipIndex = 0;
            bool insideFVecs = false;
            
            for (int i = 0; i < fVecs.Count - 1; i++)
            {
                firstFlipIndex = i;
                secondFlipIndex = i+1;
                firstFlipVal = fVecs[i].flipValue;
                secondFlipVal = fVecs[i+1].flipValue;
                
                
                if (stored.flipperValue >= firstFlipVal && stored.flipperValue <= secondFlipVal)
                {
                    insideFVecs = true;
                    break;
                }
            }
            
            float flipVal;  // The 0-1 value of the ball's position between firstFlipVal and secondFlipVal
            
            if (insideFVecs)
            {
                flipVal = Mathf.InverseLerp(firstFlipVal, secondFlipVal, stored.flipperValue);
            }
            else
            {
                if (stored.flipperValue < fVecs[0].flipValue)
                {
                    flipVal = fVecs[0].flipValue;
                }
                else
                {
                    flipVal = fVecs[^1].flipValue;
                }
            }

            float distFromPivot = Vector2.Distance(stored.position, objLink.obj2D.GetComponent<Rigidbody2D>().position);

            float firstPoint = 0;
            int firstPositionIndex = -1;
            float secondPoint = 0;
            int secondPositionIndex = -1;
            float positionVal;

            bool inside = false;

            for (int i = 0; i < fVecs[firstFlipIndex].positionVectors.Count - 1; i++)
            {
                firstPoint = fVecs[firstFlipIndex].positionVectors[i].position;
                secondPoint = fVecs[firstFlipIndex].positionVectors[i+1].position;
                if (distFromPivot >= firstPoint && distFromPivot <= secondPoint)
                {
                    firstPositionIndex = i;
                    secondPositionIndex = i + 1;
                    
                    inside = true;
                    break;
                }
            }

            if (!inside)
            {
                if (distFromPivot < fVecs[firstFlipIndex].positionVectors[0].position)
                {
                    firstPositionIndex = 0;
                    secondPositionIndex = 0;
                }
                else {
                    firstPositionIndex = fVecs[firstFlipIndex].positionVectors.Count - 1;
                    secondPositionIndex = fVecs[firstFlipIndex].positionVectors.Count - 1;
                }
            }                    
            if (firstPositionIndex == -1 || secondPositionIndex == -1)
            {
                Debug.LogError("Position indices failed to be assigned");
                return Vector2.zero;
            }

            positionVal = Mathf.InverseLerp(firstPoint, secondPoint, distFromPivot);

            Vector2 firstVec = Vector3.Lerp(fVecs[firstFlipIndex].positionVectors[firstPositionIndex].vector, fVecs[secondFlipIndex].positionVectors[firstPositionIndex].vector, flipVal);
            Vector2 secondVec = Vector3.Lerp(fVecs[firstFlipIndex].positionVectors[secondPositionIndex].vector, fVecs[secondFlipIndex].positionVectors[secondPositionIndex].vector, flipVal);
            Vector2 finalVector = Vector2.Lerp(firstVec, secondVec, positionVal);

            // Debug.DrawRay(info.position, firstVec, Color.red, 3f);
            // Debug.DrawRay(info.position, secondVec, Color.blue, 3f);
            // Debug.DrawRay(info.position, finalVector, Color.magenta, 3f);
            
            // Debug.Log("Dist from pivot:" + distFromPivot + "  1st: " + firstPositionIndex + "  2nd: " + secondPositionIndex);
            return finalVector;
    }


    Vector2 AngleToVector(float angle)
    {
        angle *= Mathf.Deg2Rad;
        return new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
    }

    void OnBallDespawn(GameObject ball)
    {
        // Clean up to prevent orphaned references
        kinematicBalls.Remove(ball);
        ballsAlreadyHit.Remove(ball);
    }
    
    class FlipVectors {
        public float flipValue;
        public List<FVPair> positionVectors;

        public FlipVectors(float flipV)
        {
            flipValue = flipV;
            positionVectors = new();
        }
        
        public void AddVector(float position, Vector2 vector)
        {
            FVPair toAdd = new(position, vector);
            positionVectors.Add(toAdd);
            // TODO: make sure it gets inserted in the correct order
        }
    
        public struct FVPair
        {
            public float position;
            public Vector2 vector;
            
            public FVPair(float p, Vector2 v)
            {
                position = p;
                vector = v;
            }
        }
    }
}
