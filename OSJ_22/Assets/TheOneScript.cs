using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class TheOneScript : MonoBehaviour
{
    [System.Serializable]
    public class RoomManager
    {
        [System.Serializable]
        public class Room
        {
            [SerializeField] private Camera camera;
            [SerializeField] private Transform idlePoint;
            [SerializeField] [TextArea] private string helpTip;
            [SerializeField] AudioClip[] transitionClips;

            public Transform IdlePoint { get { return idlePoint; } }
            public ManyHeadTest RoomTest { get; private set; }
            public string HelpTip { get { return helpTip; } }
            public AudioClip[] TransitionClips { get { return transitionClips; } }

            public Room(Room room, ManyHeadTest test)
            {
                camera = room.camera;
                idlePoint = room.idlePoint;
                RoomTest = test;
                helpTip = room.helpTip;
                transitionClips = room.transitionClips;
            }

            public void Activate()
            {
                camera.gameObject.SetActive(true);
            }

            public void Deactivate()
            {
                camera.gameObject.SetActive(false);
            }
        }

        [System.Serializable]
        public abstract class ManyHeadTest
        {
            public delegate void ModifyIntelligence(float amount);

            [SerializeField] protected float cooldownTime = 10f;

            protected float cooldownTimer = -1;

            public bool Active { get; private set; } = false;

            public ManyHeadTest(ManyHeadTest test)
            {
                cooldownTime = test.cooldownTime;
            }

            public virtual void Initialise(ModifyIntelligence modifyMethod) { Active = true; }

            public virtual void Update() 
            {
                if(cooldownTimer >= 0)
                {
                    cooldownTimer += Time.deltaTime;
                    if(cooldownTimer >= cooldownTime)
                    {
                        cooldownTimer = -1;
                    }
                }
            }

            public virtual void Exit() { Active = false; cooldownTimer = 0; }
        }

        [System.Serializable]
        public class LightTest : ManyHeadTest
        {
            [SerializeField] private MeshRenderer redLightbulbMesh, greenLightbulbMesh, buttonMesh;
            [SerializeField] private Material defaultMat, redMat, greenMat, buttonMat, redButtonMat, greenButtonMat;
            [SerializeField] private Light greenLight, redLight;
            [Range(1, 5)] [SerializeField] private float minTime = 1, maxTime = 5;
            [Range(10, 100)] [SerializeField] private float greenChance = 50;

            private float currentTime = 0;
            private float timer = -1;

            public event ModifyIntelligence ModifyIntelligenceEvent;

            public LightTest(LightTest test) : base(test)
            {
                greenLight = test.greenLight;
                redLight = test.redLight;
                minTime = test.minTime;
                maxTime = test.maxTime;
                greenChance = test.greenChance;
                redLightbulbMesh = test.redLightbulbMesh;
                greenLightbulbMesh = test.greenLightbulbMesh;
                redMat = test.redMat;
                greenMat = test.greenMat;
                defaultMat = test.defaultMat;
                buttonMat = test.buttonMat;
                redButtonMat = test.redButtonMat;
                greenButtonMat = test.greenButtonMat;
                buttonMesh = test.buttonMesh;
            }

            public void Awake()
            {
                greenLight.enabled = false;
                redLight.enabled = false;
            }

            public override void Initialise(ModifyIntelligence modifyMethod)
            {
                if (cooldownTimer < 0)
                {
                    base.Initialise(modifyMethod);
                    greenLight.enabled = false;
                    greenLightbulbMesh.materials = new Material[] { greenLightbulbMesh.materials[0], greenLightbulbMesh.materials[1], defaultMat };
                    redLightbulbMesh.materials = new Material[] { redLightbulbMesh.materials[0], redLightbulbMesh.materials[1], redMat };
                    buttonMesh.material = redButtonMat;
                    redLight.enabled = true;
                    timer = 0;
                    currentTime = Random.Range(minTime, maxTime);
                    ModifyIntelligenceEvent = new ModifyIntelligence(modifyMethod);
                }
            }

            public override void Update()
            {
                if (Active == true)
                {
                    timer += Time.deltaTime;
                    if (timer >= currentTime)
                    {
                        if (greenLight.enabled == true)
                        {
                            Debug.Log("Missed target");
                            ModifyIntelligenceEvent.Invoke(-20);
                        }
                        currentTime = Random.Range(minTime, maxTime);
                        timer = 0;
                        ToggleLight();
                    }
                    if (Input.GetButtonDown("Fire1") == true)
                    {
                        if (greenLight.enabled == true)
                        {
                            buttonMesh.material = buttonMat;
                            greenLight.enabled = false;
                            greenLightbulbMesh.materials = new Material[] { greenLightbulbMesh.materials[0], greenLightbulbMesh.materials[1], defaultMat };
                            ModifyIntelligenceEvent.Invoke(20);
                        }
                        else if(redLight.enabled == true)
                        {
                            buttonMesh.material = buttonMat;
                            redLight.enabled = false;
                            redLightbulbMesh.materials = new Material[] { redLightbulbMesh.materials[0], redLightbulbMesh.materials[1], defaultMat };
                            ModifyIntelligenceEvent.Invoke(-20);
                        }
                    }
                }
                else
                {
                    base.Update();
                }
            }

            private void ToggleLight()
            {
                int r = Random.Range(0, 100);
                if (r < greenChance)
                {
                    greenLight.enabled = true;
                    redLight.enabled = false;
                    buttonMesh.material = greenButtonMat;
                    greenLightbulbMesh.materials = new Material[] { greenLightbulbMesh.materials[0], greenLightbulbMesh.materials[1], greenMat };
                    redLightbulbMesh.materials = new Material[] { redLightbulbMesh.materials[0], redLightbulbMesh.materials[1], defaultMat };
                }
                else
                {
                    greenLight.enabled = false;
                    redLight.enabled = true;
                    buttonMesh.material = redButtonMat;
                    greenLightbulbMesh.materials = new Material[] { greenLightbulbMesh.materials[0], greenLightbulbMesh.materials[1], defaultMat };
                    redLightbulbMesh.materials = new Material[] { redLightbulbMesh.materials[0], redLightbulbMesh.materials[1], redMat };
                }
            }

            public override void Exit()
            {
                base.Exit();
                ModifyIntelligenceEvent = null;
                greenLight.enabled = false;
                redLight.enabled = false;
                buttonMesh.material = buttonMat;
                greenLightbulbMesh.materials = new Material[] { greenLightbulbMesh.materials[0], greenLightbulbMesh.materials[1], defaultMat };
                redLightbulbMesh.materials = new Material[] { redLightbulbMesh.materials[0], redLightbulbMesh.materials[1], defaultMat };
            }
        }

        [System.Serializable]
        public class ShelfTest : ManyHeadTest
        {
            private enum ActivityState { show, guess, cooldown };

            [SerializeField] private Transform[] objPlaceLocations; //Where the player places the objects
            [SerializeField] private Collider[] moveableObjects; //Where the objects orginally spawn.
            [SerializeField] private Transform[] objOriginalPos; //Where the objects orginally spawn.
            [SerializeField] private Image[] tvScreenLocations;
            [SerializeField] private Sprite[] objSprites;
            [SerializeField] private float viewTime = 2f;
            [SerializeField] private float guessTime = 5f;
            [SerializeField] private float coolDownTime = 2f;
            [SerializeField] private Camera roomCamera; //Change with player
            [SerializeField] private Sprite blankText; //Change with player
            [SerializeField] private Light tvLight;

            private GameObject currentSelectedObj;
            private GameObject currentTarget;
            private int[] correctOrder; //Set on start
            private int[] currentOrder; //Change with player
            private ActivityState state = ActivityState.show;
            private float currentTime = 0;
            private float timer = -1;

            public event ModifyIntelligence ModifyIntelligenceEvent;

            public ShelfTest(ShelfTest test) : base(test)
            {
                objPlaceLocations = test.objPlaceLocations;
                moveableObjects = test.moveableObjects; 
                tvScreenLocations = test.tvScreenLocations;
                objSprites = test.objSprites;
                state = test.state;
                roomCamera = test.roomCamera;
                viewTime = test.viewTime;
                guessTime = test.guessTime;
                coolDownTime = test.coolDownTime;
                blankText = test.blankText;
                tvLight = test.tvLight;
            }

            public void Awake()
            {
                tvLight.gameObject.SetActive(false);
            }

            public override void Initialise(ModifyIntelligence modifyMethod)
            {
                if (cooldownTimer < 0)
                {
                    base.Initialise(modifyMethod);
                    timer = 0;
                    currentTime = viewTime;
                    ModifyIntelligenceEvent = new ModifyIntelligence(modifyMethod);
                    ChangeActivityState(ActivityState.show);
                    ShowOrder();
                }
            }

            public override void Update()
            {
                if (Active == true)
                {
                    timer += Time.deltaTime;
                    if (timer >= currentTime)
                    {
                        timer = 0;
                        if(state == ActivityState.show)
                        {
                            ChangeActivityState(ActivityState.guess);
                        }
                        else if (state == ActivityState.guess)
                        {
                            //Timer ran out and failed
                            ShowOrder();
                            ModifyIntelligenceEvent.Invoke(-20);
                            ChangeActivityState(ActivityState.cooldown);
                        }
                        else if (state == ActivityState.cooldown)
                        {
                            ChangeActivityState(ActivityState.show);
                        }
                    }

                    //Raycast
                    if(currentSelectedObj != null)
                    {
                        if (Input.GetButtonUp("Fire1"))
                        {
                            if(currentTarget != null)
                            {
                                if (CheckCurrentObject())
                                {
                                    //Correct
                                    currentSelectedObj.transform.position = currentTarget.transform.position;
                                    currentSelectedObj.GetComponent<Collider>().enabled = false;
                                    currentTarget.GetComponent<Collider>().enabled = false;
                                    currentSelectedObj = null;
                                    ModifyIntelligenceEvent.Invoke(20);
                                    bool allCorrect = true;
                                    for (int i = 0; i < correctOrder.Length; i++)
                                    {
                                        if (correctOrder[i] != currentOrder[i])
                                            allCorrect = false;
                                    }
                                    //All correct
                                    if (allCorrect)
                                        ChangeActivityState(ActivityState.cooldown);
                                }
                                else
                                {
                                    //Incorrect
                                    ModifyIntelligenceEvent.Invoke(-20);
                                    //reset Pos
                                    currentSelectedObj.transform.localPosition = Vector3.zero;
                                    currentSelectedObj = null;
                                }
                            }
                            else
                            {
                                //Deselect
                                currentSelectedObj.transform.localPosition = Vector3.zero;
                                currentSelectedObj = null;
                            }
                        }
                        //Held
                        else if (Input.GetButton("Fire1"))
                        {
                            //raycast for target
                            RaycastHit hit;
                            Ray ray = roomCamera.ScreenPointToRay(Input.mousePosition);
                            if (Physics.Raycast(ray, out hit, 20f, LayerMask.GetMask("Target")))
                            {
                                //Set target
                                currentTarget = hit.transform.gameObject;
                                currentSelectedObj.transform.position = currentTarget.transform.position;
                            }
                            else
                            {
                                //Remove target
                                currentTarget = null;
                                currentSelectedObj.transform.position = roomCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -roomCamera.transform.position.z));
                            }
                        }
                    }
                    else
                    {
                        RaycastHit hit;
                        Ray ray = roomCamera.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(ray, out hit, 20f, LayerMask.GetMask("Selectable")))
                        {
                            Transform objectHit = hit.transform;
                            if (Input.GetButtonDown("Fire1"))
                            {
                                //Select
                                currentSelectedObj = objectHit.gameObject;

                            }
                            // Do something with the object that was hit by the raycast.
                        }
                    }
                }
                else
                {
                    base.Update();
                }
            }



            public void ShowOrder()
            {
                Random.InitState((int)System.DateTime.Now.Ticks);
                correctOrder = new int[objPlaceLocations.Length];
                currentOrder = new int[objPlaceLocations.Length];
                
                List<int> placementIndexs = new List<int>(){};
                
                    //create list 1-length
                for (int i = 0; i < correctOrder.Length; i++)
                {
                    placementIndexs.Add(i);
                    currentOrder[i] = -1;
                    
                }
                //Random Order for in correctOrder
                for (int i = 0; i < correctOrder.Length; i++)
                {
                    int randomInd = Random.Range(0, placementIndexs.Count);
                    int correctInd = placementIndexs[randomInd];
                    placementIndexs.RemoveAt(randomInd);
                    tvScreenLocations[i].sprite = objSprites[correctInd];
                    correctOrder[i] = correctInd;
                }
                tvLight.gameObject.SetActive(true);
                //randomize 
            }

            public void HideOrder()
            {
                foreach (Image item in tvScreenLocations)
                {
                    item.sprite = blankText;
                }
                tvLight.gameObject.SetActive(false);
            }

            public bool CheckCurrentObject()
            {

                //Get ind of current object
                //get ind of target obj
                //correctOrder[targetind] == currentObjInd
                //Debug.Log(currentSelectedObj.name);
                int targetInd = -1;
                int selectedObjInd = -1;
                for (int i = 0; i < objPlaceLocations.Length; i++)
                {
                    if (objPlaceLocations[i] == currentTarget.transform)
                        targetInd = i;
                }
                for (int i = 0; i < moveableObjects.Length; i++)
                {
                    if (moveableObjects[i].gameObject == currentSelectedObj.gameObject)
                        selectedObjInd = i;
                }
                //Debug.Log("target: " + targetInd + " selected: " + selectedObjInd + " : correctOrder: " + correctOrder[targetInd]);
                if (correctOrder[targetInd] == selectedObjInd)
                {
                    currentOrder[targetInd] = selectedObjInd;

                    return true;
                }
                    
                return false;
            }

            public void ResetActivity()
            {
                //Reset
                currentSelectedObj = null;
                foreach (Collider item in moveableObjects)
                {
                    item.transform.localPosition = Vector3.zero;
                    item.enabled = true;
                }
                foreach (Transform item in objPlaceLocations)
                {
                    item.GetComponent<Collider>().enabled = true;
                }
                HideOrder();
            }

            void ChangeActivityState(ActivityState newState)
            {
                state = newState;
                if (newState == ActivityState.show)
                {
                    ResetActivity();
                    currentTime = viewTime;
                    ShowOrder();
                    foreach (Collider item in moveableObjects)
                    {
                        item.enabled = false;
                    }
                }

                if (newState == ActivityState.guess)
                {
                    currentTime = guessTime;
                    foreach (Collider item in moveableObjects)
                    {
                        item.enabled = true;
                    }
                    HideOrder();
                }

                if (newState == ActivityState.cooldown)
                {
                    currentTime = coolDownTime;
                    currentSelectedObj = null;
                    foreach (Collider item in moveableObjects)
                    {
                        item.enabled = false;
                    }
                    ShowOrder();
                }
            }

            public override void Exit()
            {
                base.Exit();
                ModifyIntelligenceEvent = null;
                ResetActivity();
            }
        }

        [System.Serializable]
        public class GridTest : ManyHeadTest
        {
            [SerializeField] private MeshRenderer[] cells;
            [SerializeField] private Material defaultMat, activeMat, correctMat;
            [SerializeField] private float activeTime = 1, turnTime = 3, intervalTime = 1;
            [SerializeField] private Text screenText;
            [SerializeField] private Light screenLight;

            private int maxSteps = 3;
            private int currentStep = 0;
            private int[] currentSequence;
            private float timer = -1;
            private bool playerTurn = false;
            private bool interval = false;

            public event ModifyIntelligence ModifyIntelligenceEvent;

            public GridTest(GridTest test) : base(test)
            {
                cells = test.cells;
                defaultMat = test.defaultMat;
                activeMat = test.activeMat;
                correctMat = test.correctMat;
                activeTime = test.activeTime;
                turnTime = test.turnTime;
                intervalTime = test.intervalTime;
                screenText = test.screenText;
                screenLight = test.screenLight;
            }

            public void Awake()
            {
                screenText.text = "";
                screenLight.gameObject.SetActive(false);
            }

            public override void Initialise(ModifyIntelligence modifyMethod)
            {
                if (cooldownTimer < 0)
                {
                    base.Initialise(modifyMethod);
                    maxSteps = 3;
                    GetNewSequence();
                    HighlightTile(currentSequence[0], activeMat);
                    timer = 0;
                    ModifyIntelligenceEvent = new ModifyIntelligence(modifyMethod);
                }
            }

            public override void Update()
            {
                if(Active == true) 
                {
                    if(playerTurn == false)
                    {
                        if (timer >= 0)
                        {
                            timer += Time.deltaTime;
                            if(interval == true)
                            {
                                if(timer >= intervalTime)
                                {
                                    timer = 0;
                                    interval = false;
                                    HighlightTile(currentSequence[currentStep], activeMat);
                                }
                            }
                            else if (timer >= activeTime)
                            {
                                if (currentStep + 1 < maxSteps)
                                {
                                    currentStep++;
                                    ClearBoard();
                                    interval = true;
                                }
                                else
                                {
                                    playerTurn = true;
                                    currentStep = 0;
                                    ClearBoard();
                                }
                                timer = 0;
                            }
                        }
                    }
                    else
                    {
                        if (timer >= 0)
                        {
                            timer += Time.deltaTime;
                            if (interval == true)
                            {
                                if (timer >= intervalTime)
                                {
                                    timer = 0;
                                    interval = false;
                                    ClearBoard();
                                    if (currentStep + 1 < maxSteps)
                                    {
                                        currentStep++;
                                    }
                                    else
                                    {
                                        maxSteps++;
                                        playerTurn = false;
                                        currentStep = 0;
                                        GetNewSequence(true);
                                        HighlightTile(currentSequence[currentStep], activeMat);
                                    }
                                }
                            }
                            else if (Input.GetKeyDown(KeyCode.Alpha1) == true) 
                            {
                                CheckTile(0);
                            }
                            else if(Input.GetKeyDown(KeyCode.Alpha2) == true)
                            {
                                CheckTile(1);
                            }
                            else if (Input.GetKeyDown(KeyCode.Alpha3) == true)
                            {
                                CheckTile(2);
                            }
                            else if (Input.GetKeyDown(KeyCode.Alpha4) == true)
                            {
                                CheckTile(3);
                            }
                            else if (Input.GetKeyDown(KeyCode.Alpha5) == true)
                            {
                                CheckTile(4);
                            }
                            else if (Input.GetKeyDown(KeyCode.Alpha6) == true)
                            {
                                CheckTile(5);
                            }
                            else if (Input.GetKeyDown(KeyCode.Alpha7) == true)
                            {
                                CheckTile(6);
                            }
                            else if (Input.GetKeyDown(KeyCode.Alpha8) == true)
                            {
                                CheckTile(7);
                            }
                            else if (Input.GetKeyDown(KeyCode.Alpha9) == true)
                            {
                                CheckTile(8);
                            }

                            if (timer >= turnTime)
                            {
                                playerTurn = false;
                                timer = 0;
                                maxSteps = 3;
                                currentStep = 0;
                                GetNewSequence();
                                HighlightTile(currentSequence[currentStep], activeMat);
                                ModifyIntelligenceEvent.Invoke(-20);
                            }
                        }
                    }
                }
                else 
                {
                    base.Update();
                }
            }

            public override void Exit()
            {
                base.Exit();
                screenText.text = "";
                screenLight.gameObject.SetActive(false);
                currentStep = 0;
                ClearBoard();
                timer = -1;
                playerTurn = false;
                interval = false;
                ModifyIntelligenceEvent = null;
            }

            private void CheckTile(int value)
            {
                timer = 0;
                if (currentSequence[currentStep] == value)
                {
                    interval = true;
                    HighlightTile(currentSequence[currentStep], correctMat);
                    ModifyIntelligenceEvent.Invoke(20);
                    //positive response
                }
                else
                {
                    playerTurn = false;
                    maxSteps = 3;
                    currentStep = 0;
                    GetNewSequence();
                    HighlightTile(currentSequence[currentStep], activeMat);
                    ModifyIntelligenceEvent.Invoke(-20);
                    //negative response
                }
            }

            private void HighlightTile(int index, Material mat)
            {
                for (int i = 0; i < cells.Length; i++)
                {
                    if (i == index)
                    {
                        cells[i].materials = new Material[] { cells[i].materials[0], mat };
                        screenText.text = (i + 1).ToString();
                        screenLight.gameObject.SetActive(true);
                    }
                    else
                    {
                        cells[i].materials = new Material[] { cells[i].materials[0], defaultMat };
                    }
                }
            }

            private void ClearBoard()
            {
                foreach(MeshRenderer cell in cells)
                {
                    cell.materials = new Material[] { cell.materials[0], defaultMat };
                    screenText.text = "";
                    screenLight.gameObject.SetActive(false);
                }
            }

            private void GetNewSequence(bool append = false)
            {
                if (append == false)
                {
                    currentSequence = new int[maxSteps];
                    for (int i = 0; i < maxSteps; i++)
                    {
                        int r = Random.Range(0, cells.Length);
                        currentSequence[i] = r;
                    }
                }
                else
                {
                    int[] newSequence = new int[maxSteps];
                    for(int i = 0; i < maxSteps; i++)
                    {
                        if(i == maxSteps - 1)
                        {
                            int r = Random.Range(0, cells.Length);
                            newSequence[i] = r;
                        }
                        else
                        {
                            newSequence[i] = currentSequence[i];
                        }
                    }
                    currentSequence = newSequence;
                }
            }
        }

        [SerializeField] private Room[] rooms;
        [SerializeField] private LightTest lightTest;
        [SerializeField] private ShelfTest shelfTest;
        [SerializeField] private GridTest gridTest;

        public int CurrentRoomIndex { get; private set; } = -1;
        public Room CurrentRoom { get { return rooms[CurrentRoomIndex]; } }

        public void Awake()
        {
            ResetEvent += delegate { ActivateRoom(0); };
            lightTest = new LightTest(lightTest);
            lightTest.Awake();
            shelfTest = new ShelfTest(shelfTest);
            shelfTest.Awake();
            gridTest = new GridTest(gridTest);
            gridTest.Awake();
            for (int i = 0; i < rooms.Length; i++)
            {
                switch (i)
                {
                    case 1:
                        rooms[i] = new Room(rooms[i], lightTest);
                        break;
                    case 2:
                        rooms[i] = new Room(rooms[i], shelfTest);
                        break;
                    case 3:
                        rooms[i] = new Room(rooms[i], gridTest);
                        break;
                }
            }
        }

        public void Update()
        {
            foreach(Room room in rooms)
            {
                room.RoomTest?.Update();
            }
        }

        public bool ActivateRoom(int index)
        {
            if (index != CurrentRoomIndex && index < rooms.Length)
            {
                for (int i = 0; i < rooms.Length; i++)
                {
                    if (i == index)
                    {
                        rooms[i].Activate();
                    }
                    else
                    {
                        rooms[i].Deactivate();
                    }
                }
                CurrentRoomIndex = index;
                return true;
            }
            return false;
        }

        public void SubscribeToManyHeadDeathEvent(ManyHead manyHead)
        {
            foreach (Room room in rooms)
            {
                manyHead.DeadEvent += delegate 
                { 
                    if (room.RoomTest?.Active == true) 
                    {
                        room.RoomTest.Exit();
                    }
                };
            }
        }
    }

    [System.Serializable]
    public class ManyHead
    {
        public delegate void DeadDelegate();

        [SerializeField] private int ageLimit = 15;
        [SerializeField] private int yearCycle = 60;
        [SerializeField] private Image hungerImage;
        [Range(2, 10)] [SerializeField] private float feedRatio = 5;
        [SerializeField] private float hungerDepleteRatio = 1;

        private float currentHunger = 1;
        private float ageTimer = 0;

        public float CurrentIntelligence { get; private set; } = 0;
        public int Age { get; private set; } = 0;
        public int Gender { get; private set; } = -1;
        public bool Dead { get; private set; }
        public GameObject Head { get; private set; }

        public event DeadDelegate DeadEvent;

        public ManyHead(ManyHead properties, GameObject selectedHead, int selectedGender)
        {
            hungerDepleteRatio = properties.hungerDepleteRatio;
            feedRatio = properties.feedRatio;
            hungerImage = properties.hungerImage;
            ageLimit = properties.ageLimit;
            yearCycle = properties.yearCycle;
            Head = selectedHead;
            Gender = selectedGender;
            ResetEvent += delegate { Head.transform.SetParent(null); Dead = false; };
            DeadEvent = delegate { Dead = true; };
        }

        public void Update()
        {
            if (Dead == false && currentHunger > 0)
            {
                if(Age < ageLimit)
                {
                    ageTimer += Time.deltaTime;
                    if(ageTimer >= yearCycle)
                    {
                        Age++;
                        ageTimer = 0;
                    }
                }
                else
                {
                    Die();
                    return;
                }
                currentHunger -= (1 / hungerDepleteRatio) * Time.deltaTime;
                if (currentHunger <= 0)
                {
                    Die();
                    return;
                }
                hungerImage.fillAmount = currentHunger / 1;
            }
        }

        public void Feed(bool decrease = false)
        {
            if (decrease == false)
            {
                currentHunger += 1 / feedRatio;
                if(currentHunger > 1)
                {
                    currentHunger = 1;
                }
            }
            else
            {
                currentHunger -= 1 / feedRatio;
                if (currentHunger <= 0)
                {
                    Die();
                    return;
                }
            }
            hungerImage.fillAmount = currentHunger / 1;
        }

        private void Die()
        {
            currentHunger = 0;
            hungerImage.fillAmount = currentHunger / 1;
            DeadEvent.Invoke();
        }

        public void ModifyIntelligence(float amount)
        {
            CurrentIntelligence += amount;
            if(CurrentIntelligence < 0)
            {
                CurrentIntelligence = 0;
            }
        }
    }

    public delegate void ResetDelegate();

    [SerializeField] private Text camText;
    [SerializeField] private Text alarmText;
    [SerializeField] private Text endText;
    [Header("Food Camera")]
    [SerializeField] private Camera foodCamera;
    [SerializeField] private float foodCameraSpeed = 30f;
    [SerializeField] private float rotationAngle = 90f;
    [SerializeField] private float startingAngle = 45f;
    [SerializeField] private Transform feedPoint;
    [SerializeField] private float feedCooldown;
    [Header("Rooms")]
    [SerializeField] private float roomTransitionDelay = 1f;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private ManyHeadSelection selectionMenu;
    [SerializeField] private AudioClip camerSwapClip;
    [Header("Help Panel")]
    [SerializeField] private Text helpText;
    [SerializeField] private RectTransform helpPanel;
    [Header("Many Head/Agent")]
    [SerializeField] private ManyHead manyHead;
    [SerializeField] private AIAgent agent;
    [SerializeField] private Material maleMaterial;
    [SerializeField] private Material femaleMaterial;
    [SerializeField] private SkinnedMeshRenderer bodyMesh;
    [SerializeField] private Transform headObject;

    private float foodTimer = -1;
    private float helpTimer = -1;
    private AudioSource audioSource;
    private bool transitioning = false;
    private Vector3 helpPanelDefaultPos;
    private Vector3 helpPanelTargetPos;

    public static event ResetDelegate ResetEvent;

    public static TheOneScript Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
        TryGetComponent(out audioSource);
        ResetEvent = delegate
        {
            bodyMesh.gameObject.SetActive(false);
            foodCamera.gameObject.SetActive(false);
            camText.text = "";
            helpText.text = "Use the left and right arrow keys or W and A to iterate through options.\n" +
            "\nPress spacebar or enter to confirm your selection.\n\nPress Q to quit the game.";
            endText.gameObject.SetActive(false);
        };
        bodyMesh.gameObject.SetActive(false);
        agent.Awake();
        roomManager.Awake();
        selectionMenu.Awake();
    }

    public void Start()
    {
        selectionMenu.Start();
        roomManager.ActivateRoom(0);
        foodCamera.gameObject.SetActive(false);
        endText.gameObject.SetActive(false);
        camText.text = "";
        helpText.text = "Use the left and right arrow keys or W and A to iterate through options.\n" +
        "\nPress spacebar or enter to confirm your selection.\n\nPress Q to quit the game.";
        helpPanelDefaultPos = helpPanel.position;
        helpPanelTargetPos = helpPanelDefaultPos + new Vector3(helpPanel.rect.width + 50, 0, 0);
    }

    public void Update()
    {
        manyHead.Update();
        selectionMenu.Update();
        roomManager.Update();
        if (Input.GetKeyDown(KeyCode.Q) == true)
        {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.H) == true && helpTimer == -1)
        {
            //open help menu
            helpTimer = 0;
        }
        else if(helpTimer >= 0)
        {
            helpTimer += Time.deltaTime;
            if (helpPanelTargetPos == helpPanelDefaultPos)
            {
                helpPanel.position = Vector3.Lerp(helpPanel.position, helpPanelDefaultPos, helpTimer / 0.5f);
            }
            else
            {
                helpPanel.position = Vector3.Lerp(helpPanel.position, helpPanelTargetPos, helpTimer / 0.5f);
            }
            if (helpTimer >= 0.5f)
            {
                helpTimer = -1;
                if (helpPanelTargetPos == helpPanelDefaultPos)
                {
                    helpPanelTargetPos = helpPanelDefaultPos + new Vector3(helpPanel.rect.width + 50, 0, 0);
                }
                else
                {
                    helpPanelTargetPos = helpPanelDefaultPos;
                }
            }
        }
        alarmText.text = string.Format("{0:hh:mm tt}", System.DateTime.Now);
        if (manyHead.Dead == true)
        {
            if (Input.GetKeyDown(KeyCode.F) == true)
            {
                ResetEvent.Invoke();
            }
        }
        else if (bodyMesh.gameObject.activeSelf == true)
        {
            agent.Update();
            if (foodCamera.gameObject.activeSelf == true && transitioning == false)
            {
                foodCamera.transform.localEulerAngles = new Vector3(45, Mathf.PingPong(Time.time * foodCameraSpeed, rotationAngle) - startingAngle, 0);

                if (Input.GetButtonDown("Fire1") == true)
                {
                    StartCoroutine(ShootFood());
                }
                else if (Input.GetButtonDown("Cancel") == true)
                {
                    foodTimer = 0;
                    foodCamera.gameObject.SetActive(false);
                    audioSource.PlayOneShot(camerSwapClip);
                    agent.SetState(new AIAgent.MoveState(agent, roomManager.CurrentRoom.IdlePoint.position));
                    camText.gameObject.SetActive(true);
                    helpText.text = roomManager.CurrentRoom.HelpTip;
                }
            }
            if (agent.Busy == false)
            {
                if(roomManager.CurrentRoom.RoomTest?.Active == true)
                {
                    if(Input.GetButtonDown("Cancel") == true)
                    {
                        roomManager.CurrentRoom.RoomTest.Exit();
                    }
                }
                else if(transitioning == false)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1) == true)
                    {
                        StartCoroutine(RoomTransition(0));
                        camText.text = "FEEDING ROOM";
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha2) == true)
                    {
                        StartCoroutine(RoomTransition(1));
                        camText.text = "REACTION TEST";
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha3) == true)
                    {
                        StartCoroutine(RoomTransition(2));
                        camText.text = "MEMORY TEST A";
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha4) == true)
                    {
                        StartCoroutine(RoomTransition(3));
                        camText.text = "MEMORY TEST B";
                    }
                    else if(Input.GetButtonDown("Submit") == true)
                    {
                        roomManager.CurrentRoom.RoomTest?.Initialise(Respond);
                    }
                    else if (foodTimer == -1 && Input.GetButtonDown("Fire1") == true)
                    {
                        foodCamera.gameObject.SetActive(true);
                        camText.gameObject.SetActive(false);
                        audioSource.PlayOneShot(camerSwapClip);
                        agent.SetState(new AIAgent.MoveState(agent, feedPoint.position));

                        helpText.text = "Left click or press left control with the crosshair over your Many Head to feed it.\n" +
                            "\nPress escape to cancel. Press Q to quit the game.";
                    }
                }
            }
        }

        if (foodTimer > -1)
        {
            foodTimer += Time.deltaTime;
            if (foodTimer >= feedCooldown)
            {
                foodTimer = -1;
            }
        }
    }

    private void Respond(float amount)
    {
        manyHead.ModifyIntelligence(amount);
        if(amount > 0)
        {
            agent.Anim.SetTrigger("positive");
        }
        else
        {
            agent.Anim.SetTrigger("negative");
        }
    }

    IEnumerator RoomTransition(int roomIndex)
    {
        if (roomManager.ActivateRoom(roomIndex) == true)
        {
            audioSource.PlayOneShot(camerSwapClip);
            helpText.text = roomManager.CurrentRoom.HelpTip;
            if (roomManager.CurrentRoom.TransitionClips.Length > 0 && manyHead.Dead == false)
            {
                yield return new WaitForSeconds(0.2f);
                audioSource.clip = roomManager.CurrentRoom.TransitionClips[Random.Range(0, roomManager.CurrentRoom.TransitionClips.Length)];
                audioSource.Play();
            }
            transitioning = true;
            yield return new WaitForSeconds(roomTransitionDelay);
            transitioning = false;
            agent.SetState(new AIAgent.MoveState(agent, roomManager.CurrentRoom.IdlePoint.position));
        }
    }

    public void SpawnManyHead(Transform head, int gender)
    {
        manyHead = new ManyHead(manyHead, head.gameObject, gender);
        manyHead.DeadEvent += delegate 
        { 
            agent.SetState(new AIAgent.DeadState(agent));
            helpText.text = "Your Many Head has died. Press F to create a new one.\n" +
            "\nPress Q to quit the game.";
            if (head.name.Contains("radio") == true)
            {
                agent.AudioSource.enabled = false;
            }
            endText.gameObject.SetActive(true);
            endText.text = "Your Many Head's attributes are as follows.\n" +
            "\nIntelligence: " + manyHead.CurrentIntelligence +
            "\nAge: " + manyHead.Age;
        };
        roomManager.SubscribeToManyHeadDeathEvent(manyHead);
        if (gender == 1)
        {
            bodyMesh.material = maleMaterial;
        }
        else if (gender == 0)
        {
            bodyMesh.material = femaleMaterial;
        }
        bodyMesh.gameObject.SetActive(true);
        head.SetParent(headObject);
        head.localPosition = Vector3.zero;
        head.localEulerAngles = Vector3.zero;
        agent.MoveToPosition(roomManager.CurrentRoom.IdlePoint.position);
        agent.SetState(new AIAgent.IdleState(agent));
        camText.text = "FEEDING ROOM";
        helpText.text = roomManager.CurrentRoom.HelpTip;
        agent.Anim.SetBool("dead", false);
        agent.Anim.SetTrigger("birth");
        if(head.name.Contains("radio") == true)
        {
            agent.AudioSource.enabled = true;
        }
    }

    public IEnumerator ShootFood()
    {
        float duration = 1f;
        transitioning = true;
        if(Physics.Raycast(foodCamera.transform.position, foodCamera.transform.forward, out RaycastHit hit, 10f) == true)
        {
            if (hit.collider.CompareTag("Player") == true) 
            { 
                manyHead.Feed(); 
                agent.Anim.SetTrigger("eat");
                duration = 2f;
            } 
            else 
            { 
                manyHead.Feed(true); 
                agent.Anim.SetTrigger("negative"); 
            }
        }
        else
        {
            agent.Anim.SetTrigger("negative");
            manyHead.Feed(true);
        }
        yield return new WaitForSeconds(duration);
        transitioning = false;
        foodTimer = 0;
        foodCamera.gameObject.SetActive(false);
        camText.gameObject.SetActive(true);
        audioSource.PlayOneShot(camerSwapClip);
        agent.SetState(new AIAgent.MoveState(agent, roomManager.CurrentRoom.IdlePoint.position));
    }
}

[System.Serializable]
public class ManyHeadSelection
{
    [System.Serializable]
    private class UIElementGenderEgg
    {
        [SerializeField] private RawImage uiImage;

        private Outline imageOutline;

        public void Initialise()
        {
            uiImage.TryGetComponent(out imageOutline);
        }

        public void Select(bool toggle)
        {
            imageOutline.enabled = toggle;
        }

        public void Enable()
        {
            uiImage.gameObject.SetActive(true);
        }

        public void Disable()
        {
            uiImage.gameObject.SetActive(false);
        }
    }

    [Header("Gender Selection")]
    [SerializeField] private float eggRotateSpeed = 1f;
    [SerializeField] private Transform eggATransform, eggBTransform;
    [SerializeField] private Transform eggACam, eggBCam;
    [SerializeField] private UIElementGenderEgg eggAElement, eggBElement;
    [Header("Head Selection")]
    [SerializeField] private RawImage headUIImage;
    [SerializeField] private GameObject[] heads;
    [SerializeField] private Camera headCamera;
    [SerializeField] private Vector3[] headGlobalPositions;
    [SerializeField] private Vector3 cameraOffset;
    [SerializeField] private float cameraMotionTime = 1f;

    private int selectedGender = -1;
    private int selectedHead = -1;
    private float lerpTimer = -1;

    public void Awake()
    {
        eggAElement.Initialise();
        eggBElement.Initialise();
    }

    public void Initialise()
    {
        selectedGender = 0; //reset gender
        selectedHead = -1; //reset head
        //turn on egg elements
        eggAElement.Enable();
        eggBElement.Enable();
        //select first element
        eggAElement.Select(true);
        eggBElement.Select(false);
        //turn on egg cameras
        eggACam.gameObject.SetActive(true);
        eggBCam.gameObject.SetActive(true);
        //turn off head ui element and camera
        headUIImage.gameObject.SetActive(false);
        headCamera.gameObject.SetActive(false);
        //set head scene positions
        for (int i = 0; i < heads.Length; i++)
        {
            if (i < headGlobalPositions.Length)
            {
                heads[i].transform.position = headGlobalPositions[i];
                heads[i].transform.eulerAngles = Vector3.zero;
            }
        }
    }

    public void Start()
    {
        TheOneScript.ResetEvent += Initialise;
        Initialise();
    }

    public void Update()
    {
        eggATransform.transform.Rotate(0, eggRotateSpeed, 0);
        eggBTransform.transform.Rotate(0, eggRotateSpeed, 0);

        float axis = Input.GetAxis("Horizontal");
        if (selectedHead == -1)
        {
            if (axis > 0 && selectedGender != 1)
            {
                selectedGender = 1;
                eggAElement.Select(false);
                eggBElement.Select(true);

            }
            else if (axis < 0 && selectedGender != 0)
            {
                selectedGender = 0;
                eggAElement.Select(true);
                eggBElement.Select(false);
            }

            if (Input.GetButtonDown("Submit") == true)
            {
                //disable egg UI elements
                eggAElement.Disable();
                eggBElement.Disable();
                //disable egg cameras
                eggACam.gameObject.SetActive(false);
                eggBCam.gameObject.SetActive(false);
                //toggle on the head selection
                headUIImage.gameObject.SetActive(true);
                headCamera.gameObject.SetActive(true);
                selectedHead = 0;
                lerpTimer = 0;
            }
        }
        else if (headUIImage.gameObject.activeSelf == true)
        {
            if (lerpTimer > -1)
            {
                lerpTimer += Time.deltaTime;
                headCamera.transform.position = Vector3.Lerp(headCamera.transform.position, heads[selectedHead].transform.position + cameraOffset, lerpTimer / cameraMotionTime);
                if (lerpTimer >= cameraMotionTime)
                {
                    lerpTimer = -1;
                }
            }
            else
            {
                if (axis > 0 && selectedHead < heads.Length - 1)
                {
                    selectedHead++;
                    lerpTimer = 0;
                }
                else if (axis < 0 && selectedHead > 0)
                {
                    selectedHead--;
                    lerpTimer = 0;
                }
            }

            if (Input.GetButtonDown("Submit") == true)
            {
                headUIImage.gameObject.SetActive(false);
                headCamera.gameObject.SetActive(false);
                lerpTimer = -1;
                TheOneScript.Instance.SpawnManyHead(heads[selectedHead].transform, selectedGender);
            }
        }
    }
}

[System.Serializable]
public class AIAgent
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    private FiniteStateMachine stateMachine;

    public AudioSource AudioSource { get; private set; }
    public bool Busy { get; private set; } = false;
    public Animator Anim { get { return animator; } }

    public abstract class AIState : FiniteStateMachine.IState
    {
        public AIAgent Instance { get; private set; }

        public AIState(AIAgent instance)
        {
            Instance = instance;
        }

        public virtual void OnStateEnter()
        {
        }

        public virtual void OnStateExit()
        {
        }

        public virtual void OnStateUpdate()
        {
        }
    }

    public class IdleState : AIState
    {
        public IdleState(AIAgent instance) : base(instance)
        {
        }

        public override void OnStateEnter()
        {
            Instance.Busy = false;
            Instance.agent.isStopped = true;
            Instance.animator.SetBool("moving", false);
            //Debug.Log("Set state to idle");
        }

        public override void OnStateUpdate()
        {
            //Debug.Log("Idling");
        }

        public override void OnStateExit()
        {
            Instance.agent.isStopped = false;
        }
    }

    public class DeadState : AIState
    {
        public DeadState(AIAgent instance) : base(instance)
        {
        }

        public override void OnStateEnter()
        {
            Instance.Busy = false;
            Instance.agent.isStopped = true;
            Instance.animator.SetBool("dead", true);
        }
    }

    public class MoveState : AIState
    {
        Vector3 targetPosition;

        public MoveState(AIAgent instance, Vector3 position) : base(instance)
        {
            targetPosition = position;
        }

        public override void OnStateEnter()
        {
            if(NavMesh.SamplePosition(targetPosition, out NavMeshHit point, Instance.agent.height * 2, NavMesh.AllAreas) == true)
            {
                targetPosition = point.position;
                Instance.agent.SetDestination(targetPosition);
                Instance.Busy = true;
                Instance.animator.SetBool("moving", true);
            }
            else
            {
                Instance.stateMachine.SetState(new IdleState(Instance));
            }
        }

        public override void OnStateUpdate()
        {
            //Debug.Log("Moving to position");
            if(Instance.agent.remainingDistance <= Instance.agent.stoppingDistance)
            {
                Instance.stateMachine.SetState(new IdleState(Instance));
            }
        }
    }

    public AIAgent(NavMeshAgent agentInstance)
    {
        agent = agentInstance;
    }

    public void Awake()
    {
        stateMachine = new FiniteStateMachine();
        if(agent.TryGetComponent(out AudioSource aSrc) == true)
        {
            AudioSource = aSrc;
        }
        AudioSource.enabled = false;
    }

    public void Update()
    {
        stateMachine.Update();
    }

    public void SetState(AIState state)
    {
        stateMachine.SetState(state);
    }

    public void MoveToPosition(Vector3 pos)
    {
        agent.transform.position = pos;
    }
}

public class FiniteStateMachine
{
    public interface IState
    {
        void OnStateEnter();
        void OnStateUpdate();
        void OnStateExit();
    }

    public IState CurrentState { get; private set; }

    public void SetState(IState state)
    {
        if(CurrentState != null)
        {
            CurrentState.OnStateExit();
        }
        CurrentState = state;
        CurrentState.OnStateEnter();
    }

    public void Update()
    {
        if(CurrentState != null)
        {
            CurrentState.OnStateUpdate();
        }
    }
}