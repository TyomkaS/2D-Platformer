using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Threading;

public class GameManager : MonoBehaviour
{
    //*****Значения вводимые из инспектора
    //Значения вводимые при геймдизайне
    [SerializeField] string password;               //строка для создания пароля при дизайне уровня
    [SerializeField] int playerLivesCount;          //количество жизней игрока

    //Игрок
    [SerializeField] Transform playerPrefab;              //шаблон игрока
    [SerializeField] Transform playerRespawnPoint;  //Место появления игрока

    //Камеры
    //Сделано списком на всякий случай. Для этого проекта достаточно 1 камеры
    [SerializeField] private List<Camera> cameras = new List<Camera>();

    //Дверь закрывающая exit
    [SerializeField] Transform door;
    [SerializeField] Transform openedExitDoorPosition;


    //Игровые точки (срабатывают при заходе или выходе игрока)(нужна подписка)
    [SerializeField] GamePoint pass;                //Место, куда нужно зайти, чтобы увидеть пароль
    [SerializeField] GamePoint safe;                //Место, переносящее место старта игрока
    [SerializeField] GamePoint passEnter;           //Место, где игрок должен ввести пароль
    [SerializeField] GamePoint exit;                //Место, куда игрок должен попасть,чтобы закончить уровень

    //Элементы UI
    [SerializeField] TextMeshProUGUI livesCountValue;
    [SerializeField] TextMeshProUGUI healthValue;
    //Панели
    [SerializeField] GameObject enterPasswordPanel;
    [SerializeField] GameObject finalPanel;
    [SerializeField] GameObject passShowPanel;
    //Тексты
    [SerializeField] TextMeshProUGUI finalText;
    [SerializeField] TextMeshProUGUI isCorrectPassword;

    //Переменные
    [SerializeField] float openExitDoorSpeed;
    [SerializeField] float showCorestionPasswordTime;

    //*****Privates
    private bool _isPlayerOutOfPass;
    private bool _isGettedCorectPass;
    private bool _isExitClosed;
    private CameraFollower _cameraFollower;
    private Transform _player;
    private PhisicController _playerController;
    private EnterPassword _enterPassword;
    private float corectionPasswordTime;


    private void Awake()
    {
        if (cameras.Count!=0)
        {
            _cameraFollower = cameras[0].GetComponent<CameraFollower>();
            _cameraFollower.SetNewTarget(null);
        }

        isCorrectPassword.gameObject.SetActive(false);

        _enterPassword = enterPasswordPanel.GetComponent<EnterPassword>();

        livesCountValue.text = playerLivesCount.ToString();
        _isGettedCorectPass = false;

        _isExitClosed = true;
    }
    public void ClosePassPanel()
    {
        passShowPanel.SetActive(false);
    }

    public void CloseEnterPassPanel()
    {
        enterPasswordPanel.SetActive(false);
    }

    private Transform CreatePlayer()
    {
        return Instantiate(playerPrefab, playerRespawnPoint.position, Quaternion.identity);
    }
    private void EnterPanel(string value)
    {

        Debug.Log("Getted pasword is " + value);
        if (password == value)
        {
            isCorrectPassword.text = "Correct password";
            _isGettedCorectPass = true;
        }
        else
        {
            isCorrectPassword.text = "Wrong password";
        }

        corectionPasswordTime = showCorestionPasswordTime;
    }

    private void LevelComplete(GamePoint sender, Collider collider)
    {
        ShowFinalPanel(true);
    }

    private void OnEnable()
    {
        pass.OnEnter += ShowPassPanel;
        pass.OnExit += PlayerLeavePassArea;
        safe.OnEnter += RelocatePlayerRespawnPoint;
        passEnter.OnEnter += ShowEnterPassPanel;
        passEnter.OnExit += PlayerLeaveEnterPassArea;
        exit.OnEnter += LevelComplete;
        


        _enterPassword.OnEntered += EnterPanel;

    }

    private void OnDisable()
    {
        pass.OnEnter -= ShowPassPanel;
        pass.OnExit -= PlayerLeavePassArea;
        safe.OnEnter -= RelocatePlayerRespawnPoint;
        passEnter.OnEnter -= ShowEnterPassPanel;
        passEnter.OnExit -= PlayerLeaveEnterPassArea;
        exit.OnEnter -= LevelComplete;
        exit.OnExit -= PlayerLeaveEnterPassArea;

        _enterPassword.OnEntered -= EnterPanel;
    }

    private void OpenExit()
    {
        //Debug.Log("OpenExit Works");
        if (_isExitClosed)
        {
            if (door.position != openedExitDoorPosition.position)
            {
                var nextPosition = Vector3.Lerp(door.position, openedExitDoorPosition.position, Time.fixedDeltaTime * openExitDoorSpeed);
                door.position = nextPosition;
            }
            else
            {
                _isExitClosed = false;
            }    
        }         
    }

    private void PlayerOnDestroy(GameObject player)
    {
        //Debug.Log("Player On Destroy Works");
        _cameraFollower.SetNewTarget(null);
        playerLivesCount--;
        livesCountValue.text = playerLivesCount.ToString();


        if (_playerController != player)
        {
            if (true)
            {
                _playerController.OnDestroy -= PlayerOnDestroy;
                _playerController.OnDamage -= PlayerTookDamage;
            } 
        }

        if (playerLivesCount > 0)
        {
            Start();
        }
        else
        {
            ShowFinalPanel(false);
        }
    }

    private void PlayerTookDamage(GameObject player, int health)
    {
        healthValue.text = _playerController.GetHealthI().ToString();
    }   

    private void ShowFinalPanel(bool isSuccsess)
    {
        if (isSuccsess)
        {
            finalText.text = "Level Complete";
            finalText.color = new Color(255, 0, 0);
        }
        else
        {
            finalText.text = "You Died";
            finalText.color = new Color(255, 0, 0);
        }

        finalPanel.SetActive(true);
    }

    private void ShowEnterPassPanel(GamePoint sender, Collider collider)
    {
        enterPasswordPanel.SetActive(true);
        //Time.timeScale = 0f;
    }

    private void ShowPassPanel(GamePoint sender, Collider collider)
    {
        //проверка чтобы метод не срабатывал при открытой панели
        //проверяет на вхождение игрока в зону, и отображаемость панели
        if (!_isPlayerOutOfPass && !passShowPanel.activeSelf)
        {
            passShowPanel.SetActive(true);
            _isPlayerOutOfPass = !_isPlayerOutOfPass;
        }   
    }

    private void PlayerLeavePassArea(GamePoint sender, Collider collider)
        //Используется, только 
    { _isPlayerOutOfPass = false; }

    private void PlayerLeaveEnterPassArea(GamePoint sender, Collider collider)
    {
        CloseEnterPassPanel();
    }

    private void RelocatePlayerRespawnPoint(GamePoint sender, Collider collider)
    {
        Vector3 newPosition = new Vector3(-3.30f, 8.15f, 0);
        playerRespawnPoint.position= newPosition;
    }

    public void RestartLevel()
    {
        //Debug.Log("Restart Level");
        SceneManager.LoadScene(0);
    }
    // Start is called before the first frame update
    private void Start()
    {
        finalPanel.SetActive(false);
        passShowPanel.SetActive(false);
        _isPlayerOutOfPass = false;
        corectionPasswordTime = 0;          //чтобы не показывался статус введённого пароля


        //Генерация игрока
        _player = CreatePlayer();
        _playerController = _player.GetComponent<PhisicController>();

        //Подписка в методе старт, т.к. на момент вызова метода OnEnable объекта не существует
        if (_playerController != null)
        {
            //Debug.Log("_playerController is not null");
            _playerController.OnDestroy += PlayerOnDestroy;
            _playerController.OnDamage += PlayerTookDamage;
            healthValue.text = _playerController.GetHealthI().ToString();
        }

        _cameraFollower.SetNewTarget(_player);
    }

    public void Quit()
    {
        //Debug.Log("Application Quit");
        Application.Quit();
    }

    // Update is called once per frame
    private void Update()
    {
        //Блок отвечающий за открытие двери перед выходом
        //Debug.Log("Update| is correct password " + _isGettedCorectPass);
        if (_isGettedCorectPass)
        {
            if(_isExitClosed)
            {
                OpenExit();
            }
        }

        //Блок отвечающий за отображения ответа правильности ввода пароля
        if (corectionPasswordTime > 0)
        {
            if (!isCorrectPassword.gameObject.activeSelf)
            {
                isCorrectPassword.gameObject.SetActive(true);
            }
            else
            {
                corectionPasswordTime -= Time.deltaTime;
            }            
        }
        else
        {
            if (isCorrectPassword.gameObject.activeSelf)
            {
                isCorrectPassword.gameObject.SetActive(false);
            }  
        }
    }
}
