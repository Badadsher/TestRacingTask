using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
public class Movement : MonoBehaviour
{

    // ��������� �������� �������
    [SerializeField] private float moveSpeed = 300f;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float braking = 1f;
    [SerializeField] private float turnAngle = 28f;

    // ������ �������
    [SerializeField] private GameObject frontLeftWheel;
    [SerializeField] private GameObject frontRightWheel;
    [SerializeField] private GameObject rearLeftWheel;
    [SerializeField] private GameObject rearRightWheel;

    private Rigidbody rb;
    [SerializeField] private float currentSpeed = 0f;
    private bool isMoving = false;

    //�����
    public bool isRacePressed = false;
    public bool isbrakePressed = false;

    public float moveInput;
    public float turnInput;

    // ����, ���������� ��������
    private List<Vector3> pathPositions = new List<Vector3>();  // ������ ��� �������
    private List<Quaternion> pathRotations = new List<Quaternion>();  // ������ ��� ���������
    private bool hasFinishedRace = false;

    // ���� ��� ��������, ������������� �� �������
    private bool ghostCarActivated = false;

    // ��� ��� ��������, �������� �� ������ ���������
    private string ghostTag = "GhostCar";


    // ������� �������
    public GameObject ghostCarPrefab;
    private GameObject ghostCar;

    // ���������� ������
    [SerializeField] private GameObject finishLineObject;
    private bool hasCrossedFinish = false; // ��� ������������ ����������� ������

    // ������ �� UI-����� ��� ����������� �������� ������
    [SerializeField] private TextMeshProUGUI raceCounterText;
    private int currentRace = 0;

  

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f); // �������� ������ ���� ��� ����� ������������ ������  \
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // ��������, �������� �� ������� ������ ���������
        if (CompareTag(ghostTag))
        {
            return;  // ���� ������ �������� ���������, �� ��������� ���������� ������
        }
        if (isRacePressed) { }
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
        // ��������� ��������� ��� ����������
        if (moveInput > 0)
        {
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
            isMoving = true;
        }
        else if (moveInput < 0)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, -maxSpeed / 2f, braking * Time.deltaTime);
            isMoving = true;
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, braking * Time.deltaTime);
        }


        //����
        transform.Rotate(Vector3.up, turnInput * rotationSpeed * Time.deltaTime);

        // ��������� ����
        if (!hasFinishedRace)
        {
            pathPositions.Add(transform.position);
            pathRotations.Add(transform.rotation);
        }

        // ����������� �������
        rb.MovePosition(transform.position + transform.forward * currentSpeed * Time.deltaTime);

        // ������� ������
        float rotationAmount = currentSpeed / (2 * Mathf.PI * frontLeftWheel.GetComponent<MeshRenderer>().bounds.size.z / 2f) * 360f * Time.deltaTime;
        frontLeftWheel.transform.Rotate(Vector3.right, rotationAmount);
        frontRightWheel.transform.Rotate(Vector3.right, rotationAmount);
        rearLeftWheel.transform.Rotate(Vector3.right, rotationAmount);
        rearRightWheel.transform.Rotate(Vector3.right, rotationAmount);


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finisher"))
        {
            if (!hasCrossedFinish)
            {
                hasCrossedFinish = true;
                currentRace++;
                UpdateRaceCounterUI();
            }

            if (!hasFinishedRace)
            {
                hasFinishedRace = true;
                if (!ghostCarActivated)
                {
                    SpawnGhostCar();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Finisher"))
        {
            hasCrossedFinish = false;    
        }
    }

    void SpawnGhostCar()
    {
        if (ghostCarPrefab != null)
        {
            // ������ �������� �� ������ �������
            ghostCar = Instantiate(ghostCarPrefab, pathPositions[0], pathRotations[0]);
            ghostCar.tag = ghostTag;  // ����������� ��� "GhostCar" ��������
            ghostCarActivated = true;
            StartCoroutine(MoveGhostCar());
        }
    }

    // ������� ��� �������� ��������
    IEnumerator MoveGhostCar()
    {
        // ������� ����� ��������� �� ���� ������ �������
        for (int i = 0; i < pathPositions.Count; i++)
        {
            ghostCar.transform.position = pathPositions[i];
            ghostCar.transform.rotation = pathRotations[i];
            yield return new WaitForFixedUpdate();  // ������� �� ���������� �����
        }

        // ������� �������� ����� ���������� ����
        Destroy(ghostCar);
        ghostCarActivated = false;
    }

    void UpdateRaceCounterUI()
    {
            raceCounterText.text = "����: " + currentRace;
    }
}

