using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
public class Movement : MonoBehaviour
{

    // Параметры движения машинки
    [SerializeField] private float moveSpeed = 300f;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float braking = 1f;
    [SerializeField] private float turnAngle = 28f;

    // Колеса машинки
    [SerializeField] private GameObject frontLeftWheel;
    [SerializeField] private GameObject frontRightWheel;
    [SerializeField] private GameObject rearLeftWheel;
    [SerializeField] private GameObject rearRightWheel;

    private Rigidbody rb;
    [SerializeField] private float currentSpeed = 0f;
    private bool isMoving = false;

    //дрифт
    public bool isRacePressed = false;
    public bool isbrakePressed = false;

    public float moveInput;
    public float turnInput;

    // Путь, пройденный машинкой
    private List<Vector3> pathPositions = new List<Vector3>();  // Список для позиций
    private List<Quaternion> pathRotations = new List<Quaternion>();  // Список для поворотов
    private bool hasFinishedRace = false;

    // Флаг для контроля, активировался ли призрак
    private bool ghostCarActivated = false;

    // Тег для проверки, является ли объект призраком
    private string ghostTag = "GhostCar";


    // Призрак машинки
    public GameObject ghostCarPrefab;
    private GameObject ghostCar;

    // Координаты финиша
    [SerializeField] private GameObject finishLineObject;
    private bool hasCrossedFinish = false; // Для отслеживания пересечения финиша

    // Ссылка на UI-текст для отображения текущего заезда
    [SerializeField] private TextMeshProUGUI raceCounterText;
    private int currentRace = 0;

  

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f); // Смещение центра масс для более реалистичной физики  \
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Проверка, является ли текущий объект призраком
        if (CompareTag(ghostTag))
        {
            return;  // Если объект является призраком, не выполняем дальнейшую логику
        }
        if (isRacePressed) { }
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
        // Применяем ускорение или торможение
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


        //УГОЛ
        transform.Rotate(Vector3.up, turnInput * rotationSpeed * Time.deltaTime);

        // Сохраняем путь
        if (!hasFinishedRace)
        {
            pathPositions.Add(transform.position);
            pathRotations.Add(transform.rotation);
        }

        // Передвигаем машинку
        rb.MovePosition(transform.position + transform.forward * currentSpeed * Time.deltaTime);

        // Вращаем колеса
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
            // Создаём призрака на основе префаба
            ghostCar = Instantiate(ghostCarPrefab, pathPositions[0], pathRotations[0]);
            ghostCar.tag = ghostTag;  // Присваиваем тег "GhostCar" призраку
            ghostCarActivated = true;
            StartCoroutine(MoveGhostCar());
        }
    }

    // Корутин для движения призрака
    IEnumerator MoveGhostCar()
    {
        // Призрак будет двигаться по пути первой машинки
        for (int i = 0; i < pathPositions.Count; i++)
        {
            ghostCar.transform.position = pathPositions[i];
            ghostCar.transform.rotation = pathRotations[i];
            yield return new WaitForFixedUpdate();  // Ожидаем до следующего кадра
        }

        // Удаляем призрака после завершения пути
        Destroy(ghostCar);
        ghostCarActivated = false;
    }

    void UpdateRaceCounterUI()
    {
            raceCounterText.text = "Круг: " + currentRace;
    }
}

