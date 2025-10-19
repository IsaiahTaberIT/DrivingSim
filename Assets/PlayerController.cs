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
    [SerializeField] private float Drag = 0.95f;
    [SerializeField] private float BreakDrag = 0.95f;

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
            if (SteeringAngle < 90)
            {
                SteeringAngle =Min(SteeringAngle + TurningSensitivity, 90);
            }
        }

        if (Input.GetKey(KeyCode.A))
        {
            IsHoldingWheel = true;
            if (SteeringAngle > -90)
            {
                SteeringAngle = Max(SteeringAngle - TurningSensitivity, -90);
            }
        }


        if (!IsHoldingWheel)
        {
            //decay steering angle if not held
            SteeringAngle = Max(Abs(SteeringAngle) - TurningDecayRate, 0) * Sign(SteeringAngle) ;
        }



        if (Input.GetKey(KeyCode.LeftControl))
        {
            Body.AddForce(Quaternion.AngleAxis(Body.rotation.eulerAngles.y , new Vector3(0,1,0)) * Vector3.forward * Accel * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {

            Body.linearVelocity *= BreakDrag;
        }

        Vector3 steeringdirPurp = Quaternion.AngleAxis(Body.rotation.eulerAngles.y + SteeringAngle + 90, new Vector3(0, 1, 0)) * Vector3.forward;
        Vector3 steeringdir = Quaternion.AngleAxis(Body.rotation.eulerAngles.y + SteeringAngle, new Vector3(0, 1, 0)) * Vector3.forward;

        Vector3 forward = Body.transform.TransformDirection(Vector3.forward);

        Body.MoveRotation(Body.transform.rotation * Quaternion.Euler(new Vector3(0, SteeringAngle * Vector3.Dot(Body.linearVelocity, forward) / SteeringDampning * Time.deltaTime, 0)));

        Vector3 newvel = Body.linearVelocity;
        // float velMag = newvel.magnitude;

        //newvel /= 2f;

        newvel = newvel * Drag;



        MovementDirection = Body.transform.TransformDirection(Vector3.forward);


        newvel -= steeringdirPurp * (Vector3.Dot(steeringdirPurp, newvel));

        Body.linearVelocity = newvel;



        if (Input.GetKeyDown(KeyCode.R))
        {
            Body.transform.rotation = Quaternion.Euler(Vector3.zero);
            Body.transform.position = Body.transform.position + Vector3.up * 2;
        }

    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (Body != null)
        {
            Gizmos.DrawRay(transform.position + Vector3.up, Quaternion.AngleAxis(Body.rotation.eulerAngles.y + SteeringAngle, new Vector3(0, 1, 0)) * Vector3.forward);
            Gizmos.color = Color.red;


            Vector3 steeringdir = Quaternion.AngleAxis(Body.rotation.eulerAngles.y + SteeringAngle + 90, new Vector3(0, 1, 0)) * Vector3.forward;

            Debug.Log(Abs(Vector3.Dot(steeringdir, Body.linearVelocity.normalized)));



            Gizmos.DrawRay(transform.position + Vector3.up, Quaternion.AngleAxis(Body.rotation.eulerAngles.y + SteeringAngle + 90, new Vector3(0, 1, 0)) * Vector3.forward);

        }
    }

}
