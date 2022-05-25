using DriverUI.Model;
using System;
using System.Collections.Generic;
using System.Text;
using EasyModbus;
using System.Media;

namespace DriverUI.ViewModel
{
    class viewModel : ObservableObject
    {
        ModbusClient modbusClient = new ModbusClient();
        private string _info;

        public Driver Altivar { get; set; } = new Driver();

        private int [] read { get; set; } = { 0, 0, 0 };
        public string info { get => Info; set { Info = value; OnPropertyChanged( ); } }

        public ModbusClient ModbusClient { get => modbusClient; set => modbusClient = value; }
        public string Info { get => _info; set => _info = value; }

        public viewModel()
        {
            
            //ModbusClient.SerialPort = "COM2";
            //ModbusClient.UnitIdentifier = 1;
            //ModbusClient.Baudrate = 9600;
            //ModbusClient.Parity = System.IO.Ports.Parity.None;
            //ModbusClient.StopBits = System.IO.Ports.StopBits.One;
            //ModbusClient.ConnectionTimeout = 100;
            //Altivar.Status = "";
        }       
        public void Run()
        {
            try
            {
                while ( true )
                {
                    try
                    {
                        ModbusClient.Connect("10.34.60.165", 502);
                        if (ModbusClient.Connected)
                        {
                            Altivar.Moving();
                            Altivar.ToQuickStop();
                            Altivar.ToReset();
                            Altivar.ToStart();
                            Altivar.ToStop();
                            //Altivar.GetFault( );

                            read = ModbusClient.ReadHoldingRegisters(1, 2);
                            ModbusClient.WriteSingleRegister(0, Altivar.ControlWord);
                            Altivar.StatusWord = read[0];
                            Altivar.ErrorCode = read[1];
                            Altivar.Status = "";
                            Altivar.ControlStatus = "";
                            Altivar.ErrorStatus = "";
                            info = "Connection was established with Driver";
                            Altivar.GetCurrentFrequence = (int)(Altivar.SpeedReference * 260 / 3500);
                            Altivar.GetCurrentSpeed = (int)(Altivar.SpeedReference * 260 / 3500);
                            Altivar.GetCurrent = (int)(Altivar.SpeedReference * 237 / 3500);
                            Altivar.GetVoltage = (int)(Altivar.SpeedReference * 237 / 3500);
                        }
                    }
                    catch (EasyModbus.Exceptions.ModbusException ex)
                    {
                        ModbusClient.Disconnect();
                        info = ex.Message;
                    }        
                    catch (Exception ex)
                    {          
                        info = ex.Message;
                    }
                }                
            }
            catch ( System.IO.FileNotFoundException ex )
            {
                ModbusClient.Disconnect();
                info = ex.Message;
            }
            

        }
    }
}
