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

    private string rule3Answer;

    private List<string> rule3Input;

    private List<List<string>> rule2Input;

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

    //if the status light being help
    private bool holdingStatus;

    //tells if the input is a dash or dot
    private int dashOrDot;

    int dashThreshold;
    int dotThreshold;
    int breakThreshold;
    int rule3SubmitThreshold;
    int rule2SubmitThreshold;

    int rule2CurrentIndex;



    //tells if the module is submitting the morse code answer
    private int submitting;



    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;


    void Awake()
    {
        ModuleId = ModuleIdCounter++;

        blueButton.OnInteract += delegate () { BlueButton(); return false; };

        blueButton.OnInteractEnded += delegate () { BlueButtonRelease(); };

        statusLightButton.OnInteract += delegate () { StatusLightPress(); return false; };
        statusLightButton.OnInteractEnded += delegate () { StatusLightRelease(); };

        topLeftSection.OnInteract += delegate () { SectionPress(topLeftSection); return false; };
        bottomLeftSection.OnInteract += delegate () { SectionPress(bottomLeftSection); return false; };
        bottomRightSection.OnInteract += delegate () { SectionPress(bottomRightSection); return false; };
        blueButton.OnInteract += delegate () { SectionPress(blueButton); return false; };

    }

    void Start()
    {

        rule2Input = new List<List<string>>()
        {
            new List<string>(),
            new List<string>(),
            new List<string>()
        };

        timeOffset = 2;

        initialBombTime = Bomb.GetTime();

        buttonPressedNum = 0;

        rule4Started = false;

        rule5Started = false;

        holdingStatus = false;

        rule2CurrentIndex = 0;

        dashOrDot = 0;

        submitting = 0;

        dotThreshold = 50;
        dashThreshold = 150;
        rule3SubmitThreshold = 125;

        breakThreshold = 125;
        rule2SubmitThreshold = 300;


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

        if (Rule3())
        {
            rule3Answer = FindRule3Answer();
            rule3Input = new List<string>();
            Debug.Log("Expecting " + rule3Answer);

        }

        if (Rule8())
        {
            rule8Answer = FindRule8Answer();
            rule8Input = null;
            LogAnswer(8);
        }

    }

    void Update() {
        if (rule5Started && !ModuleSolved)
        {
            timeOffset -= Time.deltaTime;
        }

        if (rule4Started && !ModuleSolved)
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

    void FixedUpdate()
    {
        if (Rule2())

        {
            if (holdingStatus && !ModuleSolved)
            {
                dashOrDot++;
            }

            if (dashOrDot != 0 && dashOrDot <= dotThreshold)
            {
                //play dot sound
                Debug.Log("Dot Inputted");
            }

            if (dashOrDot > dotThreshold && dashOrDot <= dashThreshold)
            {
                //play dash sound
                Debug.Log("Dash Inputted");
            }

            else if (dashOrDot > dashThreshold)
            {
                //play input clear sound
                Debug.Log("Input Cleared");
            }

            else if (!holdingStatus && !ModuleSolved && rule2Input.Where(list => list.Any()).Any())
            {
                submitting++;
            }

            if (submitting == breakThreshold)
            {
                Debug.Log("BREAK");
                rule2CurrentIndex++;
            }

            else if (submitting == rule2SubmitThreshold)
            {
                string answer = Rule2Answer();

                Debug.Log("Submitted " + answer);
                Debug.Log("Expected -... --- -...");


                if (answer == "-... --- -...")
                {
                    ModuleSolved = true;
                    submitting = 0;
                    GetComponent<KMBombModule>().HandlePass();
                }

                else
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    ClearRule2Input();
                    submitting = 0;

                    Debug.LogError("First Section: " + string.Join("", rule2Input[0].ToArray()));
                    Debug.LogError("Second Section: " + string.Join("", rule2Input[1].ToArray()));
                    Debug.LogError("Third Section: " + string.Join("", rule2Input[2].ToArray()));

                }
            }
        }

        if (Rule3())
        {
            if (holdingStatus && !ModuleSolved)
            {
                dashOrDot++;
            }

            if (dashOrDot != 0 && dashOrDot <= dotThreshold)
            {
                //play dot sound
                Debug.Log("Dot Inputted");
            }

            if (dashOrDot > dotThreshold && dashOrDot <= dashThreshold)
            {
                //play dash sound
                Debug.Log("Dash Inputted");
            }

            else if (dashOrDot > dashThreshold)
            {
                //play input clear sound
                Debug.Log("Input Cleared");
            }

            else if (!holdingStatus && !ModuleSolved && rule3Input.Count != 0)
            {
                submitting++;
            }

            if (submitting == rule3SubmitThreshold)
            {
                Debug.Log("Submitted " + string.Join(" ", rule3Input.ToArray()));
                Debug.Log("Expected " + rule3Answer);


                if (Rule3Correct())
                {
                    ModuleSolved = true;
                    submitting = 0;
                    GetComponent<KMBombModule>().HandlePass();
                }

                else
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    rule3Input.Clear();
                    submitting = 0;
                }
            }
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

        if (!(Bomb.GetSerialNumberNumbers().Count() == 4 && Bomb.GetSerialNumberLetters().Count() == 2))
        {
            return false;
        }

        return !Unicorn();
    }

    private bool Rule2()
    {
        //if there is a lit BOB indicator
        if (!(Bomb.IsIndicatorOn(Indicator.BOB)))
        {
            return false;
        }

        return !Unicorn() && !Rule1();
    }

    private bool Rule3()
    {
        //if there is a Parallel port and Serial port on the same port plate
        if (!Bomb.GetPortPlates().Where(plate => plate.Contains("Parallel") && plate.Contains("Serial")).Any())
        {
            return false;
        }

        return !Unicorn() && !Rule1() && !Rule2();
    }

    private bool Rule4()
    {
        //if there is 4 batteries in 2 holders
        if (!(Bomb.GetBatteryCount() == 4 && Bomb.GetBatteryHolderCount() == 2))
        {
            return false;
        }

        return !Unicorn() && !Rule1() && !Rule2() && !Rule3();
    }

    private bool Rule5()
    {
        //there is a simpleton

        if (!Bomb.GetModuleNames().Where(name => name.ToUpper() == "THE SIMPLETON").Any())
        {
            return false;
        }

        return !Unicorn() && !Rule1() && !Rule2() && !Rule3() && !Rule4();
    }

    private bool Rule6()
    {
        //more than half the time is on the bomb has passed
        if (!(Bomb.GetTime() < initialBombTime / 2))
        {
            return false;
        }

        return !Unicorn() && !Rule1() && !Rule2() && !Rule3() && !Rule4() && !Rule5();
    }

    private bool Rule7()
    {
        if (!(Bomb.GetStrikes() > 0))
        {
            return false;
        }

        return !Unicorn() && !Rule1() && !Rule2() && !Rule3() && !Rule4() && !Rule5() && !Rule6();
    }

    private bool Rule8()
    {
        //total mod count is prime 
        if (!ModCountPrime())
        {
            return false;
        }

        return !Unicorn() && !Rule1() && !Rule2() && !Rule3() && !Rule4() && !Rule5() && !Rule6() && !Rule7();
    }

    private bool Rule9()
    {
        return !Unicorn() && !Rule1() && !Rule2() && !Rule3() && !Rule4() && !Rule5() && !Rule6() && !Rule7() && !Rule8();
    }

    #endregion

    #region Events
    private void BlueButton()
    {
        bool rule2Active = Rule2();
        bool rule3Active = Rule3();
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


        if (rule2Active || rule3Active || rule7Active || rule8Active)
        {
            return;
        }


        if (!rule6Active && !rule9Active)
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
        blueButton.AddInteractionPunch(0.1f);



        if (!Rule4())
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
        section.AddInteractionPunch(0.1f);

        string sectionName = SectionToName(section);
        int sectionNum = SectionToInt(section);

        bool rule1Active = Rule1();
        bool rule2Active = Rule2();
        bool rule3Active = Rule3();
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

        if (rule2Active || rule3Active)
        {
            if (sectionNum == 4)
            {
                Debug.Log("Strike! Pressed button instead of the status light");
            }

            else
            {
                Debug.Log("Strike! Pressed " + sectionName + " section instead of the status light");
            }

            GetComponent<KMBombModule>().HandleStrike();
            return;
        }

        if (rule1Active || rule4Active || rule5Active || rule6Active || rule9Active)
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
        statusLightButton.AddInteractionPunch(0.1f);

        bool rule1Active = Rule1();
        bool rule2Active = Rule2();
        bool rule3Active = Rule3();
        bool rule4Active = Rule4();
        bool rule5Active = Rule5();
        bool rule6Active = Rule6();
        bool rule7Active = Rule7();
        bool rule8Active = Rule8();
        bool rule9Active = Rule9();

        if (rule2Active && rule2CurrentIndex < 3)
        {
            submitting = 0;
            holdingStatus = true;
        }

        if (rule3Active)
        {
            submitting = 0;
            holdingStatus = true;
        }

        if (rule7Active || rule8Active)
        {
            Debug.Log("Strike! Pressed status light instead of one of the sections");
            GetComponent<KMBombModule>().HandleStrike();
            return;
        }


        if (rule1Active || rule4Active || rule5Active || rule6Active || rule9Active)
        {
            Debug.Log("Strike! Pressed status light instead of the button");
            GetComponent<KMBombModule>().HandleStrike();
            return;
        }
    }

    private void StatusLightRelease()
    {
        if (Rule2() && holdingStatus)
        {
            holdingStatus = false;

            if (dashOrDot <= dotThreshold)
            {
                rule2Input[rule2CurrentIndex].Add(".");
            }

            else if (dashOrDot > dotThreshold && dashOrDot <= dashThreshold)
            {
                rule2Input[rule2CurrentIndex].Add("-");
            }

            else if (dashOrDot > dashThreshold)
            {
                rule2CurrentIndex = 0;

                ClearRule2Input();
            }

            dashOrDot = 0;


            Debug.Log("Input is now: " + Rule2Answer());
        }

        else if (Rule3() && holdingStatus)
        {
            holdingStatus = false;

            if (dashOrDot <= dotThreshold)
            {
                rule3Input.Add(".");
            }

            else if (dashOrDot > dotThreshold && dashOrDot <= dashThreshold)
            {
                rule3Input.Add("-");
            }

            else if (dashOrDot > dashThreshold)
            {
                rule3Input.Clear();
            }

            dashOrDot = 0;


            Debug.Log("Input is now: " + string.Join("", rule3Input.ToArray()));
        }
    }


    #endregion

    #region Find Answers

    private string FindRule3Answer()
    {
        switch (Bomb.GetSerialNumberLetters().First())
        {
            case 'A':
                return ".-";

            case 'B':
                return "-...";

            case 'C':
                return "-.-.";

            case 'D':
                return "-..";

            case 'E':
                return ".";

            case 'F':
                return "..-.";

            case 'G':
                return "--.";

            case 'H':
                return "....";

            case 'I':
                return "..";

            case 'J':
                return ".---";

            case 'K':
                return "-.-";

            case 'L':
                return ".-..";

            case 'M':
                return "--";

            case 'N':
                return "-.";

            case 'O':
                return "---";

            case 'P':
                return ".--.";

            case 'Q':
                return "--.-";

            case 'R':
                return ".-.";

            case 'S':
                return "...";

            case 'T':
                return "-";

            case 'U':
                return "..-";

            case 'V':
                return "...-";

            case 'W':
                return ".--";

            case 'X':
                return "-..-";

            case 'Y':
                return "-.--";

            default:
                return "--..";
        }
    }


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

        foreach (string str in strArr)
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
            return "top left";
        }

        if (section == bottomLeftSection)
        {
            return "bottom left";
        }

        if (section == bottomRightSection)
        {
            return "bottom right";
        }

        return "button";
    }

    //Logs the answer that needs to be inputted
    private void LogAnswer(int rule)
    {
        if (rule == 8)
        {
            Debug.Log("Rule 8 answer: " + string.Join(", ", rule8Answer.Select(x => x.ToString()).ToArray()));
        }
    }

    private bool ModCountPrime()
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

    //Tells if the rule 3 are correct
    private bool Rule3Correct()
    {
        if (rule3Answer.Length != rule3Input.Count)
        {
            Debug.LogError("Lengths are not equal");
            return false;
        }

        for (int i = 0; i < rule3Answer.Length; i++)
        {
            if (rule3Answer[i].ToString() != rule3Input[i])
            {
                return false;
            }
        }

        return true;
    }

    private string Rule2Answer()
    {
        return string.Join("", rule2Input[0].ToArray()) + " " + string.Join("", rule2Input[1].ToArray()) + " " + string.Join("", rule2Input[2].ToArray());
    }

    private void ClearRule2Input()
    {
        rule2Input = new List<List<string>>()
        {
            new List<string>(),
            new List<string>(),
            new List<string>()
        };
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
