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

    private float initialBombTime;

    private int rule7Answer;

    private List<int> rule8Answer;

    private List<int> rule8Input;

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

        statusLightButton.OnInteract += delegate () { StatusLightPress(); return false; };

        topLeftSection.OnInteract += delegate () { SectionPress(topLeftSection); return false; };
        bottomLeftSection.OnInteract += delegate () { SectionPress(bottomLeftSection); return false; };
        bottomRightSection.OnInteract += delegate () { SectionPress(bottomRightSection); return false; };
        blueButton.OnInteract += delegate () { SectionPress(blueButton); return false; };


    }

    void Start () {

        timeOffset = 2;

        initialBombTime = Bomb.GetTime();

        buttonPressedNum = 0;

        rule4Started = false;

        rule5Started = false;


        Debug.Log("Unicorn: " + Unicorn());
        Debug.Log("Rule 1 " + Rule1());
        Debug.Log("Rule 2 " + Rule2());
        Debug.Log("Rule 3 " + Rule3());
        Debug.Log("Rule 4 " + Rule4());
        Debug.Log("Rule 5 " + Rule5());
        Debug.Log("Rule 6 " + Rule6());
        Debug.Log("Rule 7 " + Rule7());
        Debug.Log("Rule 8 " + Rule8());
        Debug.Log("Rule 9 " + Rule9());

        if (Rule8())
        {
            rule8Answer = FindRule8Answer();
            rule8Input = null;
            LogAnswer(8);
        }
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
        if (Unicorn())
        {
            return false;
        }

        //If the serial number contains four numbers and two letters

        return Bomb.GetSerialNumberNumbers().Count() == 4 && Bomb.GetSerialNumberLetters().Count() == 2;
    }

    private bool Rule2()
    {
        if (Unicorn() || Rule1())
        {
            return false;
        }

        //if there is a lit BOB indicator

        return Bomb.IsIndicatorOn(Indicator.BOB);
    }

    private bool Rule3()
    {
        if (Unicorn() || Rule1()|| Rule2())
        {
            return false;
        }

        //if there is a Parallel port and Serial port on the same port plate

        return Bomb.GetPortPlates().Where(plate => plate.Contains("Parallel") && plate.Contains("Serial")).Any();
    }

    private bool Rule4()
    {
        if (Unicorn() || Rule1() || Rule2() || Rule3())
        {
            return false;
        }

        //if there is 4 batteries in 2 holders
        return Bomb.GetBatteryCount() == 4 && Bomb.GetBatteryHolderCount() == 2;
    }

    private bool Rule5()
    {
        if (Unicorn() || Rule1() || Rule2() || Rule3() || Rule4())
        {
            return false;
        }

        //there is a simpleton
        return Bomb.GetModuleNames().Where(name => name.ToUpper() == "THE SIMPLETON").Any();
    }

    private bool Rule6()
    {
        if (Unicorn() || Rule1() || Rule2() || Rule3() || Rule4() || Rule5())
        {
            return false;
        }

        //more than half the time is on the bomb has passed
        return Bomb.GetTime() < initialBombTime / 2;
    }

    private bool Rule7()
    {
        if (Unicorn() || Rule1() || Rule2() || Rule3() || Rule4() || Rule5() || Rule6())
        {
            return false;
        }

        return Bomb.GetStrikes() > 0;
    }

    private bool Rule8()
    {
        if (Unicorn() || Rule1() || Rule2() || Rule3() || Rule4() || Rule5() || Rule6() || Rule7())
        {
            return false;
        }

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
        if (Unicorn() || Rule1() || Rule2() || Rule3() || Rule4() || Rule5() || Rule6() || Rule7())
        {
            return false;
        }

        return !Rule8();
    }

    #endregion

    #region Events
    private void BlueButton()
    {
        bool rule4Active = Rule4();
        bool rule5Active = Rule5();
        bool rule6Active = Rule6();
        bool rule7Active = Rule7();
        bool rule8Active = Rule8();
        bool rule9Active = Rule9();

        if (ModuleSolved)
        {
            return;
        }


        if (rule4Active)
        {
            rule4StartingTime = Time.time;
            rule4EndingTime = rule4StartingTime;
            rule4Started = true;
            return;
        }



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

        



        if (rule7Active || rule8Active)
        {
            return;
        }


        if (!rule6Active && rule9Active)
        { 
            GetComponent<KMBombModule>().HandleStrike();
            Debug.Log("Strike! Pressed the button when rule 6 didn't apply");
            return;
        }

        if (rule6Active)
        {
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
                return;
            }
        }


        if (rule9Active)
        {
            GetComponent<KMBombModule>().HandlePass();
            ModuleSolved = true;
        }
    }

    private void BlueButtonRelease()
    {
        if (Rule4())
        {
            return;
        }

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

    private void SectionPress(KMSelectable section)
    {
        
        string sectionName = SectionToName(section);
        int sectionNum = SectionToInt(section);

        bool rule4Active = Rule4();
        bool rule5Active = Rule5();
        bool rule6Active = Rule6();
        bool rule7Active = Rule7();
        bool rule8Active = Rule8();

        Debug.Log(sectionName + " section pressed");

        if (ModuleSolved)
        {
            return;
        }
        

        if (rule7Active)
        {
            rule7Answer = FindRule7Answer();   

            if (sectionNum == rule7Answer)
            {
                GetComponent<KMBombModule>().HandlePass();
                ModuleSolved = true;
                return;
            }

            else
            {
                GetComponent<KMBombModule>().HandleStrike();
                Debug.Log("Strike! Pressed section " + sectionNum + " insetead of section " + rule7Answer);
                return;
            }
        }


        if (rule8Active)
        {
            if (rule8Input == null)
            {
                rule8Input = new List<int>();
            }

            rule8Input.Add(sectionNum);

            int index = rule8Input.Count - 1;

            if (rule8Input.Last() != rule8Answer[index])
            { 
                GetComponent<KMBombModule>().HandleStrike();
                Debug.Log("Strike! Pressed section " + rule8Input.Last() + " insetead of section " + rule8Answer[index]);
                rule8Input.Clear();
                return;
            }

            if (rule8Input.Count == rule8Answer.Count)
            {
                GetComponent<KMBombModule>().HandlePass();
                ModuleSolved = true;
                return;
            }
        }


        if (rule4Active || rule5Active || rule6Active || Rule9())
        {
            if (sectionNum != 4)
            {
                Debug.Log("Strike! Pressed " + sectionName + " section instead of the button");
                GetComponent<KMBombModule>().HandleStrike();
                return;
            }
        }
    }

    private void StatusLightPress()
    {
        bool rule4Active = Rule4();
        bool rule5Active = Rule5();
        bool rule6Active = Rule6();
        bool rule8Active = Rule8();
        bool rule9Active = Rule9();

        if (rule8Active)
        {
            Debug.Log("Strike! Pressed status light instead of one of the sections");
            GetComponent<KMBombModule>().HandleStrike();
            return;
        }


        if (rule4Active || rule5Active || rule6Active || rule9Active)
        {
            Debug.Log("Strike! Pressed status light instead of the button");
            GetComponent<KMBombModule>().HandleStrike();
            return;
        }
    }


    #endregion

    #region Find Answers

    private int FindRule7Answer()
    {
        int strikes = Bomb.GetStrikes();

        while (strikes > 4)
        {
            strikes -= 4;
        }

        return strikes;
    }

    private List<int> FindRule8Answer()
    {
        int modNum = Bomb.GetModuleNames().Count();
        
        string[] strArr = modNum.ToString().Split();

        List<int> answer = new List<int>();

        foreach(string str in strArr)
        {
            int num = int.Parse(str) % 4;

            if (num == 0)
            {
                num = 4;
            }

            answer.Add(num);
        }

        return answer;
    }

    #endregion

    #region Helper Methods

    private int SectionToInt(KMSelectable section)
    {
        if (section == topLeftSection)
        {
            return 1;
        }

        if (section == bottomLeftSection)
        {
            return 2;
        }

        if (section == bottomRightSection)
        {
            return 3;
        }

        return 4;
    }

    private string SectionToName(KMSelectable section)
    {
        if (section == topLeftSection)
        {
            return "Top left";
        }

        if (section == bottomLeftSection)
        {
            return "Bottom Left";
        }

        if (section == bottomRightSection)
        {
            return "Bottom Right";
        }

        return "Button";
    }

    //Logs the answer that needs to be inputted
    private void LogAnswer(int rule)
    {
        if (rule == 8)
        {
            Debug.Log("Rule 8 answer: " + string.Join(", ", rule8Answer.Select(x => x.ToString()).ToArray()));
        }
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
