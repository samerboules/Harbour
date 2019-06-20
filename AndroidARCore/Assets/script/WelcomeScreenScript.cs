using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WelcomeScreenScript : MonoBehaviour
{
    public InputField InputFieldIPAddress;
    public string IPAddress;

    public Button OkButton;
    public InputField IPAddressInputField;
    public Text InstructionText;

    public void Start()
    {
        IPAddressInputField.text = PlayerPrefs.GetString("ServerIPAddress");
    }

    public void GetIPAddress()
    {
        //Set the variable IPAddress by the inputfield value
        IPAddress = InputFieldIPAddress.text.ToString();

        //Save IPAddress to PlayerPrefs
        PlayerPrefs.SetString("ServerIPAddress", IPAddress);

        //Hide Input Field, Text and Button
        OkButton.gameObject.SetActive(false);
        IPAddressInputField.gameObject.SetActive(false);
        InstructionText.gameObject.SetActive(false);
    }
}
