using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Mathf;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody Body;
    [SerializeField] private float Accel = 20f;
    [SerializeField] private float SteeringAngle;
    [SerializeField] private float MaxSteeringAngle = 45;

    [SerializeField] private float Drag = 0.95f;
    [SerializeField] private float BreakDrag = 0.95f;
    [SerializeField] private float MaxWheelResistance = 100f;
    [SerializeField] private float WheelSpeed = 0;
    [SerializeField] private float SpeedDifference = 0;
    [SerializeField] private float Speed = 0;
    [SerializeField] private float MinChange = 20;

    [SerializeField] private float GripForce = 1;

    [SerializeField] private Vector3 WheelVelocity = Vector3.zero;

    [SerializeField] private float TurningSensitivity = 5f;
    [SerializeField] private float SteeringDampning = 5f;

    [SerializeField] private float TurningDecayRate = 5f;
    [SerializeField] private bool IsHoldingWheel;
    [SerializeField] private Vector3 MovementDirection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Body == null)
        {
            Body = GetComponent<Rigidbody>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        IsHoldingWheel = false;

        if (Input.GetKey(KeyCode.W))
        {
            IsHoldingWheel = true;
        }

        if (Input.GetKey(KeyCode.D))
        {
            IsHoldingWheel = true;
            if (SteeringAngle < MaxSteeringAngle)
            {
                SteeringAngle =Min(SteeringAngle + TurningSensitivity * Time.deltaTime, MaxSteeringAngle);
            }
        }

        if (Input.GetKey(KeyCode.A))
        {
            IsHoldingWheel = true;
            if (SteeringAngle > -MaxSteeringAngle)
            {
                SteeringAngle = Max(SteeringAngle - TurningSensitivity * Time.deltaTime,  -MaxSteeringAngle);
            }
        }


        if (!IsHoldingWheel)
        {
            //decay steering angle if not held
            SteeringAngle = Max(Abs(SteeringAngle) - (TurningDecayRate * Time.deltaTime), 0) * Sign(SteeringAngle);

        }
      


        Vector3 steeringdirPurp = Quaternion.AngleAxis(Body.rotation.eulerAngles.y + SteeringAngle + 90, new Vector3(0, 1, 0)) * Vector3.forward;
        Vector3 steeringdir = Quaternion.AngleAxis(Body.rotation.eulerAngles.y + SteeringAngle, new Vector3(0, 1, 0)) * Vector3.forward;

        Vector3 forward = Body.transform.TransformDirection(Vector3.forward);



        if (Input.GetKey(KeyCode.LeftControl))
        {
            WheelSpeed += Accel;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            float sign = Sign(WheelSpeed);
            WheelSpeed = Max(0,(Abs(WheelSpeed) - BreakDrag * Time.deltaTime)) * sign;
        }


      



      //  Body.AddForce(Quaternion.AngleAxis(Body.rotation.eulerAngles.y, new Vector3(0, 1, 0)) * Vector3.forward * Accel * Time.deltaTime);








     


        if (Input.GetKeyDown(KeyCode.R))
        {
            Body.transform.rotation = Quaternion.Euler(Vector3.zero);
            Body.transform.position = Body.transform.position + Vector3.up * 2;
        }

    }


    private void FixedUpdate()
    {
        WheelSpeed = Sign(WheelSpeed) * Max(Abs(WheelSpeed * 0.98f - 0.01f), 0);

        Vector3 steeringdir = Quaternion.AngleAxis(Body.rotation.eulerAngles.y + SteeringAngle, new Vector3(0, 1, 0)) * Vector3.forward;

        float angleDetaPerFrame = Sign(SteeringAngle) * Max((Abs(SteeringAngle * Vector3.Dot(Body.linearVelocity, steeringdir)) - MinChange) / SteeringDampning ,0);


        Body.MoveRotation(Body.transform.rotation * Quaternion.Euler(new Vector3(0, angleDetaPerFrame, 0)));


        Vector3 steeringdirPurp = Quaternion.AngleAxis(Body.rotation.eulerAngles.y + SteeringAngle + 90, new Vector3(0, 1, 0)) * Vector3.forward;

        Vector3 forward = Body.transform.TransformDirection(Vector3.forward);


        WheelVelocity = WheelSpeed * steeringdir;

        float VelocityInSteeringDir = Vector3.Dot(steeringdir, Body.linearVelocity);

        SpeedDifference = VelocityInSteeringDir - WheelSpeed;
        Speed = VelocityInSteeringDir;


        float DifSign = Sign(SpeedDifference);

        SpeedDifference = Abs(SpeedDifference);

        float RemovedSpeed = Min(SpeedDifference, GripForce) * DifSign * -1;

        float VelocityDifference = Sqrt(Vector3.Dot(WheelVelocity, Body.linearVelocity));

        if (VelocityDifference > GripForce)
        {
            Debug.Log("Reached");
            RemovedSpeed *= 0.1f;
        }


        WheelSpeed -= RemovedSpeed;
        Body.linearVelocity += RemovedSpeed * steeringdir.normalized;



        Vector3 newvel = Body.linearVelocity;
      

        newvel -= (newvel.normalized * Max(0, newvel.sqrMagnitude * Drag * 0.01f));



        MovementDirection = Body.transform.TransformDirection(Vector3.forward);


        newvel -= steeringdirPurp * Max(MaxWheelResistance * -1, (Vector3.Dot(steeringdirPurp, newvel)));

        Body.linearVelocity = newvel;



    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (Body != null)
        {
            Gizmos.DrawRay(transform.position + Vector3.up, Quaternion.AngleAxis(Body.rotation.eulerAngles.y + SteeringAngle, new Vector3(0, 1, 0)) * Vector3.forward);
            Gizmos.color = Color.red;


            Vector3 steeringdir = Quaternion.AngleAxis(Body.rotation.eulerAngles.y + SteeringAngle + 90, new Vector3(0, 1, 0)) * Vector3.forward;
//
         //   Debug.Log(Abs(Vector3.Dot(steeringdir, Body.linearVelocity.normalized)));



            Gizmos.DrawRay(transform.position + Vector3.up, Quaternion.AngleAxis(Body.rotation.eulerAngles.y + SteeringAngle + 90, new Vector3(0, 1, 0)) * Vector3.forward);

        }
    }

}
