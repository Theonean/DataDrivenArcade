using System.Collections;
using System.IO.Ports;
using UnityEngine;

public class ArcadeInput : MonoBehaviour
{
    private string[] baudRates = new string[] { "9600", "14400", "19200", "38400", "57600", "115200" }; // Add or remove baud rates as needed
    private string[] portNames = new string[]{"COM1","COM2","COM3","COM4","COM5","COM6","COM7","COM8"}; // List of serial ports to try connecting to

    void Start()
    {
        //portNames = SerialPort.GetPortNames(); // Get all COM ports available on the system
        print("Portnames length: " + portNames.Length);
        StartCoroutine(TestSerialPorts());
    }

    IEnumerator TestSerialPorts()
    {
        foreach (var portName in portNames)
        {
            foreach (var baudRate in baudRates)
            {
                using (SerialPort serialPort = new SerialPort(portName, int.Parse(baudRate)))
                {
                    try
                    {
                        serialPort.Open();
                        if (serialPort.IsOpen)
                        {
                            Debug.Log($"Opened {portName} with baud rate {baudRate}.");
                            serialPort.ReadTimeout = 500; // Set a read timeout (in milliseconds)

                            try
                            {
                                string data = serialPort.ReadLine(); // Try reading data
                                Debug.Log($"Data received on {portName} at baud rate {baudRate}: {data}");
                                // Consider adding a break here if you want to stop after the first successful read
                            }
                            catch(System.TimeoutException)
                            {
                                // Handle timeout where no data was received
                                Debug.Log($"No data received on {portName} at baud rate {baudRate} within timeout period.");
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Failed to open {portName} with baud rate {baudRate}: {e.Message}");
                    }
                    finally
                    {
                        if (serialPort.IsOpen)
                        {
                            serialPort.Close();
                        }
                    }
                }
                yield return null; // Ensure the Unity Editor remains responsive
            }
        }
        Debug.Log("Finished testing all COM ports with specified baud rates.");
    }
}
