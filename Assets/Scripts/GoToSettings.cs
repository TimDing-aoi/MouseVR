﻿using UnityEngine;
using TMPro;
using SFB;
using System;
using System.Xml;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
//using static ViveSR.anipal.Eye.SRanipal_Eye_Framework;

public class GoToSettings : MonoBehaviour
{
    public Canvas mainMenu;
    public Canvas taskSelectMenu;
    public Canvas human2DMenu;
    public Canvas humanArenaMenu;
    public Canvas mouse2DMenu;
    public Canvas mouseCorridorMenu;
    public Canvas motionCueingMenu;
    public GameObject obj;
    public GameObject settingMenu1;
    public GameObject settingMenu2;
    public MotionCueingController MotionCueingController;
    public UnityEngine.UI.Button saveButton;
    public UnityEngine.UI.Button loadButton;
    private TMP_InputField input;
    private TMP_Text buttonText;
    private TMP_Text fireflyText;
    public TMP_InputField freq;
    public TMP_InputField duty;
    public TMP_InputField span;
    public TMP_InputField vmin;
    public TMP_InputField vmax;
    public TMP_InputField rmin;
    public TMP_InputField rmax;
    public TMP_InputField rampTime;
    public TMP_InputField rampDelay;
    public TMP_InputField Lspeed;
    public TMP_InputField Aspeed;
    public TMP_InputField v1;
    public TMP_InputField v2;
    public TMP_InputField v3;
    public TMP_InputField v4;
    public TMP_InputField v5;
    public TMP_InputField v6;
    public TMP_InputField v7;
    public TMP_InputField v8;
    public TMP_InputField v9;
    public TMP_InputField v10;
    public TMP_InputField v11;
    public TMP_InputField v12;
    public TMP_InputField size;
    public TMP_InputField radius;
    public TMP_InputField min;
    public TMP_InputField max;
    public TMP_InputField dist;
    public TMP_InputField height;
    public GameObject panel;
    public TMP_Text error;
    public GameObject okayButton;
    public GameObject loading;
    public Canvas window;

    // Start is called before the first frame update
    void Start()
    {
        if (obj.name == "Update") SetXML();
        //UnityEngine.Screen.SetResolution(1920, 1080, true);
        if (obj.name == "Switch Mode")
        {
            buttonText = obj.transform.Find("Text (TMP)").GetComponent<TMP_Text>();
        }
        else if (obj.name == "Switch Behavior")
        {
            fireflyText = obj.transform.Find("Text (TMP)").GetComponent<TMP_Text>();
            fireflyText.text = "fixed";
            PlayerPrefs.SetString("Switch Behavior", fireflyText.text);
            //ActivateFields();
        }
        else if (obj.name == "Okay :)")
        {
            input = null;
        }
        else if (obj.name == "Port")
        {
            List<TMP_Dropdown.OptionData> optionData = new List<TMP_Dropdown.OptionData>();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData
                {
                    text = port
                };
                optionData.Add(option);
            }
            obj.GetComponent<TMP_Dropdown>().options = optionData;
        }
        else
        {
            input = obj.GetComponent<TMP_InputField>();
        }

        MotionCueingController = FindObjectOfType<MotionCueingController>();

        PlayerPrefs.SetInt("Save", 0);
    }

    public void ToSettings()
    {
        //Camera.main.transform.position = new Vector3(1500, 9, 0);
        mainMenu.enabled = false;
        human2DMenu.enabled = false;
        humanArenaMenu.enabled = false;
        mouse2DMenu.enabled = false;
        taskSelectMenu.enabled = true;
    }

    public void ToHuman2DSettings()
    {
        taskSelectMenu.enabled = false;
        human2DMenu.enabled = true;
        PlayerPrefs.SetInt("Scene", 0);
        if (obj.GetComponentInChildren<TMP_Text>().text == "monkey2d") PlayerPrefs.SetInt("Scene", 9);
    }

    public void ToHumanArenaSettings()
    {
        taskSelectMenu.enabled = false;
        humanArenaMenu.enabled = true;
        PlayerPrefs.SetInt("Scene", 1);
    }

    public void ToMouse2DSettings()
    {
        taskSelectMenu.enabled = false;
        mouse2DMenu.enabled = true;
        PlayerPrefs.SetInt("Scene", 2);
    }

    public void ToMouseArenaSettings()
    {

    }

    public void ToMouseCorridorSettings()
    {
        taskSelectMenu.enabled = false;
        mouseCorridorMenu.enabled = true;
        PlayerPrefs.SetInt("Scene", 4);
    }

    public void ToMotionCueingSettings()
    {
        mainMenu.enabled = false;
        human2DMenu.enabled = false;
        humanArenaMenu.enabled = false;
        mouse2DMenu.enabled = false;
        motionCueingMenu.enabled = true;
    }

    public void Enable()
    {
        MotionCueingController.ControlOnAndEnable();
    }

    public void Disable()
    {
        MotionCueingController.ControlOffAndDisable();
    }

    public void Zero()
    {
        MotionCueingController.GoToZeroPos();
    }

    public void Retracted()
    {
        MotionCueingController.GoToRetractedPos();
    }

    public void ConfigERTT()
    {
        MotionCueingController.ConfigureERTT();
    }

    //public void BeginCalibration()
    //{
    //    panel.SetActive(true);
    //    loading.SetActive(true);
    //    LoadingAnimation();
    //    int result = ViveSR.anipal.Eye.SRanipal_Eye_API.LaunchEyeCalibration(System.IntPtr.Zero);
    //    error.text = "error " + result.ToString() + ": " + (ViveSR.Error)result;
    //    panel.SetActive(false);
    //    loading.SetActive(false);
    //}

    public async void LoadingAnimation()
    {
        while (panel.activeInHierarchy)
        {
            loading.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 30.0f));
            await new WaitForSeconds(0.5f);
        }
    }

    public void Okay()
    {
        panel.SetActive(false);
        loading.SetActive(false);
        okayButton.SetActive(false);
    }

    public void ToMainMenu()
    {
        //Camera.main.transform.position = new Vector3(0, 9, 0);
        mainMenu.enabled = true;
        taskSelectMenu.enabled = false;
        human2DMenu.enabled = false;
        humanArenaMenu.enabled = false;
        // motionCueingMenu.enabled = false;
        // set mouse menus to .enabled = false as well, whenever they get implemented
    }

    public void SwitchMode()
    {
        if (buttonText.text == "experiment")
        {
            buttonText.text = "replication";
        } 
        else
        {
            buttonText.text = "experiment";
        }
    }

    public void SwitchBehavior()
    {
        switch (fireflyText.text)
        {
            case "always on":
                fireflyText.text = "flashing";
                break;
            case "flashing":
                fireflyText.text = "fixed";
                break;
            case "fixed":
                fireflyText.text = "always on";
                break;
            default:
                fireflyText.text = "fixed";
                break;
        }
    }

    public void SwitchPageRegular()
    {
        print(settingMenu1.name);
        print(settingMenu2.name);
        settingMenu1.SetActive(true);
        settingMenu2.SetActive(false);
    }

    public void SwitchPageFF()
    {
        print(settingMenu1.name);
        print(settingMenu2.name);
        settingMenu1.SetActive(false);
        settingMenu2.SetActive(true);
    }

    /// <summary>
    /// Instead of hard-coding every single setting, just use the name of the
    /// object that this function is currently acting upon as the key for its
    /// value. There is no avoiding hard-coding setting the respective varibles
    /// to the correct value, however; you need to remember what the names of
    /// the objects and what variable they are associated with.
    /// 
    /// For example:
    /// If I have an object whose name is "Distance" and, in the game, I set it
    /// to "90", as in the TMP_InputField.text = "90", that value gets stored in
    /// PlayerPrefs associated to the key "Distance", but there is no way to 
    /// store the keys in a separate class and use them later. Anyway, trying to
    /// get keys from somewhere is harder, so just hard-code it when retrieving
    /// the values.
    /// </summary>
    public void saveSetting()
    {
        try
        {
            if (obj.name == "Perturbation" || obj.name == "Moving ON" || obj.name == "Feedback ON" || obj.name == "AboveBelow" || obj.name == "Full ON" || obj.name == "VertHor" || obj.name == "End Trial On Stop" || obj.name == "Ramp" || obj.name == "Firefly Only")
            {
                PlayerPrefs.SetInt(obj.name, obj.GetComponent<UnityEngine.UI.Toggle>().isOn ? 1 : 0);
            }
            else if (obj.name == "Eye Mode" || obj.name == "FP Mode" || obj.name == "Distal Object" || obj.name == "Dimensions" || obj.name == "Stop Option")
            {
                PlayerPrefs.SetInt(obj.name, obj.GetComponent<TMP_Dropdown>().value);
            }
            else if (obj.name == "Port")
            {
                PlayerPrefs.SetString(obj.name, obj.GetComponent<TMP_Dropdown>().options[obj.GetComponent<TMP_Dropdown>().value].text);
            }
            else if (obj.name == "Path")
            {
                string temp = input.text;
                PlayerPrefs.SetString(obj.name, input.text);
                try
                {
                    File.WriteAllText(temp, "test");
                }
                catch (Exception e)
                {
                    Debug.LogException(e, this);
                }
            }
            else if (obj.name == "Replay Path")
            {
                //string temp = input.text;
                //PlayerPrefs.SetString(obj.name, input.text);
            }
            else if (obj.name == "Name" || obj.name == "Date")
            {
                string temp = input.text;
             
                PlayerPrefs.SetString(obj.name, input.text);
            }
            else
            {
                PlayerPrefs.SetFloat(obj.name, float.Parse(input.text));
                if (input.text == null)
                {
                    throw new Exception("Invalid or missing TMP_InputField text.");
                }
            }
            if (obj.name == null)
            {
                throw new Exception("Invalid or missing object name.");
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    public void SwitchPtb()
    {
        if (obj.GetComponent<UnityEngine.UI.Toggle>().isOn)
        {
            vmin.interactable = true;
            vmax.interactable = true;
            rmin.interactable = true;
            rmax.interactable = true;
        }
        else
        {
            vmin.interactable = false;
            vmax.interactable = false;
            rmin.interactable = false;
            rmax.interactable = false;
        }
    }

    public void SwitchThresh()
    {
        if (obj.GetComponent<UnityEngine.UI.Toggle>().isOn)
        {
            vmin.interactable = true;
            rmin.interactable = true;
        }
        else
        {
            vmin.interactable = false;
            rmin.interactable = false;
        }
    }

    public void SwitchRamp()
    {
        if (obj.GetComponent<UnityEngine.UI.Toggle>().isOn)
        {
            rampTime.interactable = true;
            rampDelay.interactable = true;
        }
        else
        {
            rampTime.interactable = false;
            rampDelay.interactable = false;
        }
    }

    public void SwitchVel()
    {
        if (obj.GetComponent<UnityEngine.UI.Toggle>().isOn)
        {
            v1.interactable = true;
            v2.interactable = true;
            v3.interactable = true;
            v4.interactable = true;
            v5.interactable = true;
            v6.interactable = true;
            v7.interactable = true;
            v8.interactable = true;
            v9.interactable = true;
            v10.interactable = true;
            v11.interactable = true;
            v12.interactable = true;
        }
        else
        {
            v1.interactable = false;
            v2.interactable = false;
            v3.interactable = false;
            v4.interactable = false;
            v5.interactable = false;
            v6.interactable = false;
            v7.interactable = false;
            v8.interactable = false;
            v9.interactable = false;
            v10.interactable = false;
            v11.interactable = false;
            v12.interactable = false;
        }
    }

    private void LoadSetting(Transform gameObject)
    {
        try
        {
            PlayerPrefs.SetFloat(gameObject.name, float.Parse(input.text));
            if (gameObject.name == null)
            {
                throw new Exception("Invalid or missing object name.");
            }
            if (input.text == null)
            {
                throw new Exception("Invalid or missing TMP_InputField text.");
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    public void SaveMode()
    {
        PlayerPrefs.SetString(obj.name, buttonText.text);
    }

    public void SaveBehavior()
    {
        PlayerPrefs.SetString(obj.name, fireflyText.text);
    }

    public void SwitchMenu()
    {
        if (buttonText.text == "experiment")
        {
            Lspeed.interactable = true;
            Aspeed.interactable = true;
            saveButton.interactable = true;
        }
        else
        {
            Lspeed.interactable = false;
            Aspeed.interactable = false;
            saveButton.interactable = false;
        }
    }

    public void SetScale()
    {
        float scale = float.Parse(input.text);
        size.text = (0.25f * scale).ToString();
        radius.text = (0.5f * scale).ToString();
        min.text = (1.0f * scale).ToString();
        max.text = (4.0f * scale).ToString();
        dist.text = (15.0f * scale).ToString();
        height.text = (0.1f * scale).ToString();

        PlayerPrefs.SetFloat(size.name, float.Parse(size.text));
        PlayerPrefs.SetFloat(radius.name, float.Parse(radius.text));
        PlayerPrefs.SetFloat(min.name, float.Parse(min.text));
        PlayerPrefs.SetFloat(max.name, float.Parse(max.text));
        PlayerPrefs.SetFloat(dist.name, float.Parse(dist.text));
        PlayerPrefs.SetFloat(height.name, float.Parse(height.text));
    }

    public void SetAll()
    {
        string[] name = input.name.Split(' ');
        float set = float.Parse(input.text);
        for (int i = 0; i < 3; i++)
        {
            string objname = name[1] + " " + (i + 1).ToString();
            //print(objname);
            TMP_InputField thing = input.gameObject.transform.parent.Find(objname).gameObject.GetComponent<TMP_InputField>();
            thing.text = set.ToString();
            PlayerPrefs.SetFloat(thing.name, float.Parse(thing.text));
        }
    }

    public void ActivateFields()
    {
        switch (fireflyText.text)
        {
            case "flashing":
                freq.interactable = true;
                duty.interactable = true;
                span.interactable = false;
                break;
            case "fixed":
                freq.interactable = false;
                duty.interactable = false;
                span.interactable = true;
                break;
            case "always on":
                freq.interactable = false;
                duty.interactable = false;
                span.interactable = false;
                break;
            default:
                freq.interactable = false;
                duty.interactable = false;
                span.interactable = true;
                break;
        }
    }

    public void SaveXML()
    {
        try
        {
            var extensions = new[] {
                new ExtensionFilter("Extensible Markup Language ", "xml")
            };
            var path = StandaloneFileBrowser.SaveFilePanel("Set File Destination", "", "", extensions);
            PlayerPrefs.SetInt("Save", 1);
            PlayerPrefs.SetString("Config Path", path);
            print(PlayerPrefs.GetInt("Save"));
            print(PlayerPrefs.GetString("Config Path"));
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    public void SetXML()
    {
        try
        {
            string path = PlayerPrefs.GetString("Config Path");

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            if (settingMenu1.activeInHierarchy)
            {
                foreach (Transform child in settingMenu1.transform)
                {
                    foreach (Transform children in child)
                    {
                        if (children.gameObject.CompareTag("Setting"))
                        {
                            if (children.name == "Eye Mode" || children.name == "FP Mode")
                            {
                                TMP_Dropdown drop = children.GetComponent<TMP_Dropdown>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            drop.value = int.Parse(setting.InnerText);
                                            PlayerPrefs.SetInt(children.name, drop.value);
                                        }
                                    }
                                }
                            }
                            else if (children.name == "Perturbation On" || children.name == "Moving ON" || children.name == "Feedback ON" || children.name == "AboveBelow" || children.name == "VertHor" || children.name == "Full ON")
                            {
                                UnityEngine.UI.Toggle toggle = children.GetComponent<UnityEngine.UI.Toggle>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            toggle.isOn = int.Parse(setting.InnerText) == 1;
                                            PlayerPrefs.SetInt(children.name, toggle.isOn ? 1 : 0);
                                        }
                                    }
                                }
                            }
                            //else if (children.name == "VertHor")
                            //{
                            //    TMP_InputField field = children.GetComponent<TMP_InputField>();
                            //    foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            //    {
                            //        foreach (XmlNode setting in node.ChildNodes)
                            //        {
                            //            if (setting.Name == children.name.Replace(" ", ""))
                            //            {
                            //                field.text = setting.InnerText;
                            //                PlayerPrefs.SetInt(children.name, int.Parse(field.text));
                            //            }
                            //        }
                            //    }
                            //}
                            else if (children.name == "Path")
                            {
                                TMP_InputField field = children.GetComponent<TMP_InputField>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            field.text = setting.InnerText;
                                            PlayerPrefs.SetString(children.name, field.text);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                TMP_InputField field = children.GetComponent<TMP_InputField>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            field.text = setting.InnerText;
                                            PlayerPrefs.SetFloat(children.name, float.Parse(field.text));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (settingMenu2.activeInHierarchy)
            {
                foreach (Transform child in settingMenu2.transform)
                {
                    if (child.gameObject.CompareTag("Setting"))
                    {
                        TMP_InputField field = child.GetComponent<TMP_InputField>();
                        foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                        {
                            foreach (XmlNode setting in node.ChildNodes)
                            {
                                if (setting.Name == child.name.Replace(" ", ""))
                                {
                                    field.text = setting.InnerText;
                                    if (child.name == "Optic Flow Seed" || child.name == "Firefly Seed")
                                    {
                                        PlayerPrefs.SetInt(child.name, int.Parse(field.text));
                                        print(PlayerPrefs.GetInt(child.name));
                                    }
                                    else
                                    {
                                        PlayerPrefs.SetFloat(child.name, float.Parse(field.text));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    public void LoadXML()
    {
        try
        {
            var extensions = new[] {
                new ExtensionFilter("Extensible Markup Language ", "xml")
            };
            var path = StandaloneFileBrowser.OpenFilePanel("Open File Destination", "", extensions, false);
            // TODO: set all playerprefs and corresponding text fields to xml settings

            XmlDocument doc = new XmlDocument();
            doc.Load(path[0]);

            if (settingMenu1.activeInHierarchy)
            {
                foreach (Transform child in settingMenu1.transform)
                {
                    foreach (Transform children in child)
                    {
                        if (children.gameObject.CompareTag("Setting"))
                        {
                            if (children.name == "Eye Mode" || children.name == "FP Mode" || children.name == "Distal Object" || children.name == "Dimensions")
                            {
                                TMP_Dropdown drop = children.GetComponent<TMP_Dropdown>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            drop.value = int.Parse(setting.InnerText);
                                            PlayerPrefs.SetInt(children.name, drop.value);
                                        }
                                    }
                                }
                            }
                            else if (children.name == "Perturbation On" || children.name == "Moving ON" || children.name == "Feedback ON" || children.name == "AboveBelow" || children.name == "VertHor" || children.name == "Full ON" || children.name == "End Trial On Stop" || children.name == "Ramp")
                            {
                                UnityEngine.UI.Toggle toggle = children.GetComponent<UnityEngine.UI.Toggle>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            toggle.isOn = int.Parse(setting.InnerText) == 1;
                                            PlayerPrefs.SetInt(children.name, toggle.isOn ? 1 : 0);
                                        }
                                    }
                                }
                            }
                            //else if (children.name == "VertHor")
                            //{
                            //    TMP_InputField field = children.GetComponent<TMP_InputField>();
                            //    foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            //    {
                            //        foreach (XmlNode setting in node.ChildNodes)
                            //        {
                            //            if (setting.Name == children.name.Replace(" ", ""))
                            //            {
                            //                field.text = setting.InnerText;
                            //                PlayerPrefs.SetInt(children.name, int.Parse(field.text));
                            //            }
                            //        }
                            //    }
                            //}
                            else if (children.name == "Path")
                            {
                                TMP_InputField field = children.GetComponent<TMP_InputField>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            field.text = setting.InnerText;
                                            PlayerPrefs.SetString(children.name, field.text);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                TMP_InputField field = children.GetComponent<TMP_InputField>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            //print(setting.Name);
                                            field.text = setting.InnerText;
                                            PlayerPrefs.SetFloat(children.name, float.Parse(field.text));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (settingMenu2.activeInHierarchy)
            {
                foreach (Transform child in settingMenu2.transform)
                {
                    if (child.gameObject.CompareTag("Setting"))
                    {
                        TMP_InputField field = child.GetComponent<TMP_InputField>();
                        foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                        {
                            foreach (XmlNode setting in node.ChildNodes)
                            {
                                if (setting.Name == child.name.Replace(" ", ""))
                                {
                                    field.text = setting.InnerText;
                                    if (child.name == "Optic Flow Seed" || child.name == "Firefly Seed")
                                    {
                                        PlayerPrefs.SetInt(child.name, int.Parse(field.text));
                                        print(PlayerPrefs.GetInt(child.name));
                                    }
                                    else
                                    {
                                        PlayerPrefs.SetFloat(child.name, float.Parse(field.text));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }
}
