using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace Implementation
{
    class MagnetController
    {
        public MccDaq.MccBoard DaqBoard;

        //I am using this code with a Mccdaq USB-3105
        //debugging: the config file must be stored in //the config file must be set at C:\Users\YourUserName\AppData\Roaming\MagnetConfig.txt
        //When pulling this project from GitHub, you will find an example config file in the 
        System.Int32 outputChannel;
        MccDaq.Range AORange = MccDaq.Range.Bip10Volts; //I have hard coded the range for my device
        float dataValue;
        MccDaq.VOutOptions options = MccDaq.VOutOptions.Default; //I have hardcoded the options for my device
        List<string> currentStateOfDevice;
        List<string> newDeviceState;
        string stringOfDataValue;

        public void initialise()
        {
          currentStateOfDevice = new List<string>();
          newDeviceState = new List<string>();

          //First Lets make sure there's a USB-3101FS plugged in,
          System.Int16 BoardNum;
          System.Boolean Boardfound = false;
          for (BoardNum = 0; BoardNum < 99; BoardNum++)
          { 
            DaqBoard = new MccDaq.MccBoard(BoardNum);
            try
            {
                if (DaqBoard.BoardName.Contains("3105"))
                {
                    //we found a valid board, let's print that out.
                    Boardfound = true;
                    //DaqBoard.FlashLED();
                    System.Console.WriteLine("MagnetController: Just found a device. Here is the result of its .getType() method: " + DaqBoard.GetType());
                    System.Console.WriteLine("MagnetController: Here is the board number of the device found: " + BoardNum);
                    break;
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("MagnetController: An exception occurred");
            }
          }

          if (Boardfound == false)
          {
              System.Console.WriteLine("MagnetController: No USB-3105 found in system.  Please run InstaCal.", "No Board detected");
          }

          //print out how many channels this device has
          System.Int32 NumDAChans;
          DaqBoard.BoardConfig.GetNumDaChans(out NumDAChans);
          if (NumDAChans < 1)
          {
              System.Console.WriteLine("MagnetController: No Analog Outputs on this USB-3105.  To run this program you must have a USB-3105.", "No DACs detected");
          }
          System.Console.WriteLine("MagnetController: The detected board has " + NumDAChans + " DA channels.");

          while (true)
          {
            readInConfigFile();
            updateCurrentListAndExecuteChanges();
          }
        }

        public void setVoltage(System.Int32 channel, float voltage)
        {
            System.Console.WriteLine("MagnetController: Setting voltage to " + voltage);
            DaqBoard.VOut(channel, AORange, voltage, options); //options and range are hardcoded
        }

        //Reads in from the config file, puts it in the newDeviceState list
        public void readInConfigFile()
        {
          string textFromFile = "";
          //the config file must be set in C:\Users\YourUserName\AppData\Roaming\MagnetConfig.txt
          string fileLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MagnetConfig.txt";
          
          //rewrite the list with whatever is in the config file this time.
          newDeviceState = new List<string>();

          //debugging: the config file must be stored in C:\Users\Jess\AppData\Roaming
          //System.Console.WriteLine("The file is being read from: " + fileLocation);
          //C:\Users\Jess\AppData\Roaming

          //read the file using a slightly more complicated way, to allow for locking of the file
          using (FileStream stream = File.Open(fileLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
          {
            using (StreamReader reader = new StreamReader(stream))
            {
              //Debugging
              //System.Console.WriteLine("**About to read in from the file**");
              //System.Console.WriteLine("Each of the 16 channels is below:");
              while (!reader.EndOfStream)
              {
                textFromFile = reader.ReadLine();
                newDeviceState.Add(textFromFile);
                //Debugging
                //System.Console.WriteLine(textFromFile);
              }
            }
          }
        }

        /*
         * A method to update the current list with the changed values. Compares the two lists.
         * 
         * For use when we've read in the values from the file, let's check that there are any changes with the current state.
         * If there are no changes, then don't update the device.
         * If there are changes, update the device.
         * 
         **/
        public void updateCurrentListAndExecuteChanges()
        {
          for (int i = 0; i < currentStateOfDevice.Count; i++)
          {
            if (newDeviceState.Count == 0)
            {
                return;
            }
            try
            {
                if (currentStateOfDevice.ElementAt(i) == newDeviceState.ElementAt(i))
                {
                    //if they are equal, then do nothing, don't update the device
                }
                else
                {
                    stringOfDataValue = "";
                    outputChannel = i; //element we're up to
                    stringOfDataValue = newDeviceState.ElementAt(i);
                    //turn the string into a float
                    float.TryParse(stringOfDataValue, NumberStyles.Any, CultureInfo.InvariantCulture, out dataValue);
                    setVoltage(outputChannel, dataValue);
                    System.Console.WriteLine("MagnetController: The values are different. Updated the output channel: " + outputChannel + " with " + dataValue + " volts.");

                    //System.Threading.Thread.Sleep(3000);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("MagnetController: A problem occured when checking if " + currentStateOfDevice.ElementAt(i) + " and " + newDeviceState.ElementAt(i) + " were equal.**");
                System.Console.WriteLine("MagnetController: Debugging: the currentDeviceState size: " + currentStateOfDevice.Count);
                System.Console.WriteLine("MagnetController: Debugging: the newDeviceState size: " + newDeviceState.Count);
            }
          }
          //update so we store the new values as the current values, ahead of next time.
          currentStateOfDevice = newDeviceState;
        }
    }
}
