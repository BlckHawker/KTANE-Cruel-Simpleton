using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class CruelSimpleton : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

    public KMSelectable topLeftSection;
    public KMSelectable bottomLeftSection;
    public KMSelectable bottomRightSection;
    public KMSelectable statusLightButton;
    public KMSelectable blueButton;

    private bool unicorn;
    private bool rule1;
    private bool rule2;
    private bool rule3;
    private bool rule4;
    private bool rule5;
    private bool rule8;

    private float initialBombTime;

    //time the user started holding down and releasing the button for rule 4
    private float rule4StartingTime;
    private float rule4EndingTime;

    private bool rule4Started;

    //number of time button has been pressed for rule 5
    private int buttonPressedNum;

    //tells if the user started pressing the button for rule 5
    private bool rule5Started;
    

    private float timeOffset;



    static int ModuleIdCounter = 1;
   int ModuleId;
   private bool ModuleSolved;


   void Awake () {
      ModuleId = ModuleIdCounter++;
        /*
        foreach (KMSelectable object in keypad) {
            object.OnInteract += delegate () { keypadPress(object); return false; };
        }
        */

        //button.OnInteract += delegate () { buttonPress(); return false; };

        blueButton.OnInteract += delegate () { BlueButton(); return false; };

        blueButton.OnInteractEnded += delegate () { BlueButtonRelease(); };


        topLeftSection.OnInteract += delegate () { StrikeButton(); return false; };

    }

   void Start () {

        timeOffset = 2;

        initialBombTime = Bomb.GetTime();

        buttonPressedNum = 0;

        rule4Started = false;

        unicorn = Unicorn();
        rule1 = Rule1();
        rule2 = Rule2();
        rule3 = Rule3();
        rule4 = Rule4();
        rule5 = Rule5();
        rule8 = Rule8();



        
        Debug.Log("Unicorn: " + unicorn);
        Debug.Log("Rule 1 " + rule1);
        Debug.Log("Rule 2 " + rule2);
        Debug.Log("Rule 3 " + rule3);
        Debug.Log("Rule 4 " + rule4);
        Debug.Log("Rule 5 " + rule5);
        

        /*
        Debug.Log("Rule 6 " + Rule6());
        Debug.Log("Rule 7 " + Rule7());
        Debug.Log("Rule 8 " + rule8);
        Debug.Log("Rule 9 " + Rule9());
        */
        

    }

    void Update () {
        if (rule5Started && !ModuleSolved)
        { 
            timeOffset -= Time.deltaTime;
        }

        if(rule4Started && !ModuleSolved)
        {
            rule4StartingTime += Time.deltaTime;
        }


        if (timeOffset <= 0 && !ModuleSolved)
        {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.Log("Strike! 2 seconds have passed since you pressed the button. Restarting module");
            rule5Started = false;
            buttonPressedNum = 0;
            timeOffset = 2;
        }
   }

    #region Rules

    private bool Unicorn()
    {
        //if there are 2 batteries in 2 holders, 2 indicators, a DVI, RJ-45, PS2, and RCA ports on the same port plate, and the serial number contains a "U"

        int holderNum = Bomb.GetBatteryHolderCount();
        int batteryNum = Bomb.GetBatteryCount();
        int indicatorNum = Bomb.GetIndicators().Count();
        string serialNumber = Bomb.GetSerialNumber();

        bool fullPlate = Bomb.GetPortPlates().Where(plate => plate.Contains("DVI") && plate.Contains("PS2") && plate.Contains("RJ45") && plate.Contains("StereoRCA")).Any();


        return holderNum == 2 && batteryNum == 2 && indicatorNum == 2 && fullPlate && serialNumber.ToUpper().Contains('U');
    }

    private bool Rule1()
    {
        //If the serial number contains four numbers and two letters

        return Bomb.GetSerialNumberNumbers().Count() == 4 && Bomb.GetSerialNumberLetters().Count() == 2;
    }

    private bool Rule2()
    {
        //if there is a lit BOB indicator

        return Bomb.IsIndicatorOn(Indicator.BOB);
    }

    private bool Rule3()
    {
        //if there is a Parallel port and Serial port on the same port plate

        return Bomb.GetPortPlates().Where(plate => plate.Contains("Parallel") && plate.Contains("Serial")).Any();
    }

    private bool Rule4()
    {
        //if there is 4 batteries in 2 holders
        return Bomb.GetBatteryCount() == 4 && Bomb.GetBatteryHolderCount() == 2;
    }

    private bool Rule5()
    {
        //there is a simpleton
        return Bomb.GetModuleNames().Where(name => name.ToUpper() == "THE SIMPLETON").Any();
    }

    private bool Rule6()
    {
        //more than half the time is on the bomb has passed
        return Bomb.GetTime() < initialBombTime / 2;
    }

    private bool Rule7()
    {
        return Bomb.GetStrikes() > 0;
    }

    private bool Rule8()
    {
        //total mod count is prime 

        int moduleNum = Bomb.GetModuleNames().Count();

        if (moduleNum == 1) return false;
        if (moduleNum == 2) return true;

        var limit = Math.Ceiling(Math.Sqrt(moduleNum)); //hoisting the loop limit

        for (int i = 2; i <= limit; ++i)
            if (moduleNum % i == 0)
                return false;

        return true;
    }

    private bool Rule9()
    {
        return !rule1 && !rule2 && !rule3 && !rule4 && !rule5 && !Rule6() && !Rule7() && !rule8;
    }

    #endregion

    #region Events
    private void BlueButton()
    {
        if(ModuleSolved)
        {
            return;
        }

        bool rule4Active = !rule1 && !rule2 && !rule3 && rule4;

        if (rule4Active)
        {
            rule4StartingTime = Time.time;
            rule4EndingTime = rule4StartingTime;
            rule4Started = true;
            return;
        }

        bool rule5Active = !rule4Active && rule5;


        if (rule5Active)
        {
            rule5Started = true;
            timeOffset = 2f;
            buttonPressedNum++;
            
            Debug.Log("Button has been pressed " + buttonPressedNum + " times");


            if (buttonPressedNum == 69)
            {
                rule5Started = false;
                GetComponent<KMBombModule>().HandlePass();
                ModuleSolved = true;
                Debug.Log("Module solved: " + ModuleSolved);
            }
            return;
        }

        bool rule6Active = !rule5Active && Rule6();

        if (!rule6Active)
        { 
            GetComponent<KMBombModule>().HandleStrike();
            Debug.Log("Strike! Pressed the button when rule 6 didn't apply");
            return;
        }

        int seconds = (int)Bomb.GetTime() % 60;

        if (seconds % 10 != 0)
        {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.Log("Strike! Pressed the button when the seconds were " + seconds + ", which is not a multiple of 10");
            return;
        }

        else
        {
            GetComponent<KMBombModule>().HandlePass();
            ModuleSolved = true;
        }


    }

    private void BlueButtonRelease()
    {
        rule4Started = false;
        float deltaTime = Math.Abs(rule4EndingTime - rule4StartingTime);

        float minValue = 7.5f;
        float maxValue = 8.5f;

        if (minValue <= deltaTime && deltaTime <= maxValue)
        {
            GetComponent<KMBombModule>().HandlePass();
            ModuleSolved = true;
        }

        else
        {
            GetComponent<KMBombModule>().HandleStrike();

            string time = string.Format("{0:0.#}", deltaTime);

            Debug.Log("Strike! Held button for " + time + " seconds");
        }
    }

    private void StrikeButton()
    {
        GetComponent<KMBombModule>().HandleStrike();
    }
    #endregion




#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      yield return null;
   }

   IEnumerator TwitchHandleForcedSolve () {
      yield return null;
   }
}
