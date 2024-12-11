using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TubeRendererInternals;
using UnityEngine.InputSystem;


public class TubeTesting : MonoBehaviour
{

    [SerializeField] private InputActionAsset ActionAsset;

    [SerializeField] private InputActionReference TriggerPressedR;
    [SerializeField] private InputActionReference TriggerPressedL;
    [SerializeField] private InputActionReference DevicePositionR;
    [SerializeField] private InputActionReference DevicePositionL;


    public GameObject prefab;
    public int numPoints;

    
    public GameObject RTrackingHandPos;
    public GameObject LTrackingHandPos;

    public GameObject HandTracker;
    public GameObject collisionPrefab;
    public GameObject collisionPrefab2;
    public GameObject collisionPrefab3;
    public GameObject collisionPrefab4;
    public GameObject collisionPrefab5;

    VectorManager VectorFunctions;
    float recStrength = .05f;
    float angleStrength = .05f;
    float distanceApart = 5f;
    float desiredAngle = 90f;
    float epsilon = .2f;
    float ptpWeight = .1f;
    float lineToPointWeight = .025f;
    float lineToLineWeight = 1f;

    float radius = 1f;
    public int pTorus = 2;
    public int qTorus = 3;
    
    
    


    // Start is called before the first frame update
    GameObject[] PointArray;
    Vector3[] PointPositionsArray;
    TubeRenderer tube;

    private bool isDragging = false;

    private bool isDraggingL = false;
    private bool isDraggingR = false;
    private int selectedPointIndexL = -1;
    private int selectedPointIndexR = -1;
    private int selectedPointIndex = -1;
    public Camera mainCamera;
    float inputDistance = 8f;

    float inputVRDistance = 5f;

    // refactor loops into their own functions soon

    private void OnEnable() 
    {
        if (ActionAsset != null) 
        {
            ActionAsset.Enable(); 
        } 
    } 

    void changePoints()
    {
        // The below loop is for point length
        for (int test = 0; test < 1; test++)
        {
            // The below loop is for point length
            for (int i = 0; i < PointArray.Length; i++)
            {
                VectorList Rec = VectorFunctions.ropeSegmentRecommendation(new Vector3(PointArray[i].transform.position.x, PointArray[i].transform.position.y, PointArray[i].transform.position.z), new Vector3(PointArray[((i + 1) % PointArray.Length)].transform.position.x, PointArray[((i + 1) % PointArray.Length)].transform.position.y, PointArray[((i + 1) % PointArray.Length)].transform.position.z), distanceApart);
                PointArray[i].transform.position = Rec.Item1 * recStrength + PointArray[i].transform.position;
                PointArray[((i + 1) % PointArray.Length)].transform.position = Rec.Item2 * recStrength + PointArray[((i + 1) % PointArray.Length)].transform.position;
            }

            // The below loop is for angles
            for (int i = 0; i < PointArray.Length; i++)
            {
                bool AngleRec = VectorFunctions.ropeSegmentAngleTest(new Vector3(PointArray[i].transform.position.x, PointArray[i].transform.position.y, PointArray[i].transform.position.z), new Vector3(PointArray[((i + 1) % PointArray.Length)].transform.position.x, PointArray[((i + 1) % PointArray.Length)].transform.position.y, PointArray[((i + 1) % PointArray.Length)].transform.position.z), new Vector3(PointArray[((i + 2) % PointArray.Length)].transform.position.x, PointArray[((i + 2) % PointArray.Length)].transform.position.y, PointArray[((i + 2) % PointArray.Length)].transform.position.z), epsilon);
                if (AngleRec)
                {
              
                    VectorList Rec = VectorFunctions.ropeSegmentAngle(new Vector3(PointArray[i].transform.position.x, PointArray[i].transform.position.y, PointArray[i].transform.position.z), new Vector3(PointArray[((i + 1) % PointArray.Length)].transform.position.x, PointArray[((i + 1) % PointArray.Length)].transform.position.y, PointArray[((i + 1) % PointArray.Length)].transform.position.z), new Vector3(PointArray[((i + 2) % PointArray.Length)].transform.position.x, PointArray[((i + 2) % PointArray.Length)].transform.position.y, PointArray[((i + 2) % PointArray.Length)].transform.position.z), desiredAngle);
                    PointArray[i].transform.position = Rec.Item1 * angleStrength + PointArray[i].transform.position;
                    PointArray[((i + 1) % PointArray.Length)].transform.position = Rec.Item2 * angleStrength + PointArray[((i + 1) % PointArray.Length)].transform.position;
                    PointArray[((i + 2) % PointArray.Length)].transform.position = Rec.Item3 * angleStrength + PointArray[((i + 2) % PointArray.Length)].transform.position;
                    
                }
            }
            // The below loop is for point collisions
            for (int i = 0; i < numPoints; i++)
            {

                //point to point
                for (int j = 0; j < i; j++)
                {
                    float DistBetweenPoints = Vector3.Distance(PointArray[i].transform.position, PointArray[j].transform.position);
                    if (DistBetweenPoints < 2 * radius)
                    {
                        //Instantiate(collisionPrefab, PointArray[i].transform.position, Quaternion.identity);
                        //Instantiate(collisionPrefab2, PointArray[j].transform.position, Quaternion.identity);
                        VectorList positionRec = VectorFunctions.ropeSegmentCollisionPointToPoint(PointArray[i].transform.position, PointArray[j].transform.position, ptpWeight, radius);
                        Debug.Log("Point Collision!!");
                        PointArray[i].transform.position = positionRec.Item1;
                        PointArray[j].transform.position = positionRec.Item2;
                    }
                }
                // point to line
                for (int j = 0; j < numPoints; j++)
                {
                    Vector3 u = PointArray[j].transform.position;
                    Vector3 v = PointArray[(j + 1) % numPoints].transform.position;
                    Vector3 w = PointArray[i].transform.position;
                    if (j == i || (j + 1) % numPoints == i)
                    {
                        continue;
                    }


                    Vector3 W = w - u;
                    Vector3 V = v - u;
                    Vector3 p = (Vector3.Dot(W, V) / Vector3.Dot(V, V)) * V + u;
                    float Direction = Vector3.Dot((p - u), (p - v));
                    float dist = (w - p).magnitude;
                    Vector3 recVector = (w - p).normalized * (radius-dist/2); 

                    if (Direction < 0)
                    {
                        if ((w - p).magnitude < 2 * radius)
                        {
                            //Instantiate(collisionPrefab3, PointArray[j].transform.position, Quaternion.identity);
                            //Instantiate(collisionPrefab4, p, Quaternion.identity);
                            //Instantiate(collisionPrefab5, PointArray[i].transform.position, Quaternion.identity);
                            Debug.Log("PointToLine Collision Changed!!");
                            // move u, v away from w
                            //VectorFunctions.ropeSegmentRecommendation()

                            //VectorList positionRec = VectorFunctions.ropeSegmentCollisionPointToPoint(u, w, lineToPointWeight);
                            Vector3 positionRec = u - recVector * lineToPointWeight;
                            PointArray[j].transform.position = positionRec;
                            //positionRec = VectorFunctions.ropeSegmentCollisionPointToPoint(v, w, lineToPointWeight);
                            positionRec = v - recVector * lineToPointWeight;
                            PointArray[(j + 1) % numPoints].transform.position = positionRec;
                            //positionRec = VectorFunctions.ropeSegmentCollisionPointToPoint(w, VectorFunctions.calcMidpoint(u, v), lineToPointWeight);
                            positionRec = w + recVector * lineToPointWeight;
                            PointArray[i].transform.position = positionRec;
                        }
                    }

                }

            }
            for (int i = 0; i < numPoints; i++)
            {
                

                for (int j = 0; j < i - 1; j++)

                // if j = i-1 then the two line segments share the point i, which we do not want to check
                 
                {
                    // avoids checking extra collisions on the other side
                    if (j > i -numPoints + 1) {
                        Vector3 a = PointArray[i].transform.position;
                        Vector3 b = PointArray[(i + 1) % numPoints].transform.position;
                        Vector3 c = PointArray[j].transform.position;
                        Vector3 d = PointArray[(j + 1) % numPoints].transform.position;
                        Vector3 x2 = VectorFunctions.ropeSegmentCollisionLineToLineCheck(a, b, c, d);
                        if (x2 == Vector3.zero)
                        {
                            continue;
                        }

                        Vector3 x1 = VectorFunctions.ropeSegmentCollisionLineToLineCheck(c, d, a, b);
                        if(x1 == Vector3.zero)
                        {
                            continue;
                        }
                    
                        if((x1 - x2).magnitude < 2 * radius)
                        {
                            float distanceToMove =  radius - (x1 - x2).magnitude * .5f;

                            Vector3 direction = (x1 - x2).normalized * distanceToMove;
                            PointArray[i].transform.position += direction * lineToLineWeight;
                            PointArray[(i + 1) % numPoints].transform.position += direction * lineToLineWeight;
                            PointArray[j].transform.position -= direction * lineToLineWeight;
                            PointArray[(j + 1) % numPoints].transform.position -= direction * lineToLineWeight;
                            Debug.Log("LineToLine Collision");
                        }
                    }

                }
            }
            for (int i = 0; i < PointArray.Length; i++)
            {
                PointPositionsArray[i] = PointArray[i].transform.position;
            }
            PointPositionsArray[PointArray.Length] = PointArray[0].transform.position;

            
        }
    }

    void HandleVRInput(){

        Vector3 OffsetL =  LTrackingHandPos.transform.TransformDirection(Vector3.forward * 5);
        Vector3 OffsetR =  RTrackingHandPos.transform.TransformDirection(Vector3.forward * 5);
        float closestDistanceR = inputVRDistance;
        float closestDistanceL = inputVRDistance;
        float closestDistance = inputDistance;
         if(TriggerPressedR.action.ReadValue<float>() == 1){
            int closestRIndex = -1;
            Vector3 RHandPos = RTrackingHandPos.transform.position + OffsetR;
            //Instantiate(HandTracker, RHandPos, Quaternion.identity);
             for (int i = 0; i < PointArray.Length; i++)
            {
                
                Vector3 pointPosition = PointArray[i].transform.position;
                float distanceToPositionR = Vector3.Distance(RHandPos, pointPosition);
                
                if (distanceToPositionR < closestDistanceR)
                {
                    Debug.Log("first IF entered");
                    closestDistanceR = distanceToPositionR;
                    closestRIndex = i;
                    //Debug.Log(DevicePositionL.action.ReadValue<Vector3>());
                }
            }
            if (closestRIndex != -1)
            {
                isDraggingR = true;
                selectedPointIndexR = closestRIndex;
            }
        }
        else if (TriggerPressedR.action.ReadValue<float>() == 0)
        {
            // Stop dragging
            isDraggingR = false;
            selectedPointIndexR = -1;
        }
        if (isDraggingR && selectedPointIndexR != -1)
        {
            // Update point position based on mouse movement
            Debug.Log("point is updating");
            PointArray[selectedPointIndexR].transform.position = RTrackingHandPos.transform.position;
            Debug.Log("point updated from " + PointPositionsArray[selectedPointIndexR] + " to " + RTrackingHandPos.transform.position);
            PointPositionsArray[selectedPointIndexR] = RTrackingHandPos.transform.position;
            
        }



        if(TriggerPressedL.action.ReadValue<float>() == 1){
            Debug.Log("Trigger Pressed");
            int closestLIndex = -1;
            Vector3 LHandPos = LTrackingHandPos.transform.position + OffsetL;
            //Instantiate(HandTracker, LHandPos, Quaternion.identity);
            Debug.Log(DevicePositionL.action.ReadValue<Vector3>());
             for (int i = 0; i < PointArray.Length; i++)
            {
                
                Vector3 pointPosition = PointArray[i].transform.position;
                float distanceToPosition = Vector3.Distance(LHandPos, pointPosition);
                
                if (distanceToPosition < closestDistanceL)
                {
                    Debug.Log("first IF entered");
                    closestDistanceL = distanceToPosition;
                    closestLIndex = i;
                    //Debug.Log(DevicePositionL.action.ReadValue<Vector3>());
                }
            }
            if (closestLIndex != -1)
            {
                isDraggingL = true;
                selectedPointIndexL = closestLIndex;
            }
        }
        else if (TriggerPressedL.action.ReadValue<float>() == 0)
        {
            // Stop dragging
            isDraggingL = false;
            selectedPointIndexL = -1;
        }
        if (isDraggingL && selectedPointIndexL != -1)
        {
            // Update point position based on mouse movement
            Debug.Log("point is updating");
            PointArray[selectedPointIndexL].transform.position = LTrackingHandPos.transform.position;
            Debug.Log("point updated from " + PointPositionsArray[selectedPointIndexL] + " to " + LTrackingHandPos.transform.position);
            PointPositionsArray[selectedPointIndexL] = LTrackingHandPos.transform.position;
            
        }


    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Raycast to find the point
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            float closestDistance = inputDistance;
            int closestIndex = -1;

            for (int i = 0; i < PointArray.Length; i++)
            {
                Vector3 pointPosition = PointArray[i].transform.position;
                float distanceToRay = Vector3.Cross(ray.direction, pointPosition - ray.origin).magnitude;

                if (distanceToRay < closestDistance)
                {
                    closestDistance = distanceToRay;
                    closestIndex = i;
                }
            }

            if (closestIndex != -1)
            {
                isDragging = true;
                selectedPointIndex = closestIndex;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // Stop dragging
            isDragging = false;
            selectedPointIndex = -1;
        }

        if (isDragging && selectedPointIndex != -1)
        {
            // Update point position based on mouse movement
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero); // Assuming y-axis as up
            float distance = 10f;
            if (plane.Raycast(ray, out distance))
            {
                Vector3 point = ray.GetPoint(10f);
                PointArray[selectedPointIndex].transform.position = point;
                PointPositionsArray[selectedPointIndex] = point;
            }
        }
    }


    void Start()
    {

        //Following code creates arrays to hold the points the rope is made out of
        PointArray = new GameObject[numPoints];
        PointPositionsArray = new Vector3[numPoints + 1];

        for (var i = 0; i < numPoints; i++)
        {
            float t = ((i) * 2 * Mathf.PI / (numPoints));
            float scalar = 10 / (Mathf.Sqrt(2) - Mathf.Sin(qTorus * t));
            // make 2, 2, 3 to variables
            PointArray[i] = Instantiate(prefab, new Vector3(scalar * Mathf.Cos(pTorus * t), scalar*Mathf.Sin(pTorus * t), scalar *Mathf.Cos(qTorus*t)), Quaternion.identity);
            PointPositionsArray[i] = PointArray[i].transform.position;
            //Debug.Log(PointPositionsArray[i]);
        }
        // Make this better later, more organized
        float avg = 0f;
        for(int i = 0; i < numPoints; i++)
        {
            avg += (PointArray[i].transform.position - PointArray[(i+1) % numPoints].transform.position).magnitude;
        }
        avg = avg / numPoints;
        distanceApart = avg;
        Debug.Log(distanceApart);

        PointPositionsArray[numPoints] = PointPositionsArray[0];
        //this line above, sets the last point to the first point, so it will render as a loop.

        VectorFunctions = GameObject.Find("GameObject").GetComponent<VectorManager>();

        //InvokeRepeating("changePoints", .25f, .25f);
        
        tube = gameObject.AddComponent<TubeRenderer>();
        tube.MarkDynamic();


        
    }



    // Update is called once per frame
    void Update()
    {
        tube.points = PointPositionsArray;

        changePoints();

        HandleMouseInput();
        HandleVRInput();

        tube.radiuses = new float[] { radius };
        
        tube.ForceUpdate();
    }
}
