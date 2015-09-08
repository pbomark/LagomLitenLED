﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace L3
{
    class LagomLitenLed
    {
        private UsbDevice MyUsbDevice;
        private UsbDeviceFinder MyUsbFinder;
        private byte[] red;
        private byte[] green;
        private byte[] blue;
        private int numberOfDiodes = 28; // hard code number of diodes like a boss

        public LagomLitenLed()
        {
            MyUsbFinder = new UsbDeviceFinder(0x1781, 0x1111);
            red = new byte[numberOfDiodes];
            green = new byte[numberOfDiodes];
            blue = new byte[numberOfDiodes];

        }
        public void open()
        {
            // Find and open the usb device.
            MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);
            if (MyUsbDevice == null) throw new Exception("Device Not Found.");
            UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
        }
         public void update()
        {
            for (int i = 0; i < numberOfDiodes; i++)
            {
                // send all diode colors without refreshing (command 2)
                // pack led number and red into "value" pack green and blue into "index"
                sendCommand(2, (short)(i | red[i] << 8), (short)(green[i] | blue[i] << 8));
            }
            sendRefresh();
        }
        public void close()
        {
            if (MyUsbDevice != null)
            {
                if (MyUsbDevice.IsOpen)
                {
                    MyUsbDevice.Close();
                }
                MyUsbDevice = null;
                UsbDevice.Exit();
            }
        }
        public void setToBlack()
        {
            for (int i = 0; i < numberOfDiodes; i++)
            {
                red[i] = 0;
                blue[i] = 0;
                green[i] = 0;
            }
        }
        public void setRed(int index, byte value)
        {
            red[index] = value;
        }
        public void setGreen(int index, byte value)
        {
            green[index] = value;
        }
        public void setBlue(int index, byte value)
        {
            blue[index] = value;
        }
        public byte getRed(int index)
        {
            return red[index];
        }
        public byte getGreen(int index)
        {
            return green[index];
        }
        public byte getBlue(int index)
        {
            return blue[index];
        }
        public string sendRefresh()
        {
            // send refresh command (request 3)
            return (sendCommand(3, 0, 0));
        }

        public string sendCommand(byte command, short value, short index)
        {
            byte[] buffer = new byte[8];
            int actuallySent;

            UsbSetupPacket usp = new UsbSetupPacket((byte)((byte)UsbRequestType.TypeVendor | (byte)UsbRequestRecipient.RecipDevice | (byte)UsbEndpointDirection.EndpointIn), command, value, index, (short)buffer.Length);
            if (MyUsbDevice.ControlTransfer(ref usp, buffer, buffer.Length, out actuallySent))
                return "Trinket says: " + buffer[0] + " " + buffer[1] + " " + buffer[2] + " " + buffer[3];
            else
                return "Command" + command.ToString() + "failed.";
        }
        public int getNumberOfDiodes()
        {
            return numberOfDiodes;
        }
    }
}