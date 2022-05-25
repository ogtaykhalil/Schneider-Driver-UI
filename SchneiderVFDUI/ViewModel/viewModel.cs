using SchneiderVFDUI.Model;
using System;
using System.Collections.Generic;
using System.Text;
//using EasyModbus;
using SchneiderVFDUI.View;
using System.Media;

namespace SchneiderVFDUI.ViewModel
{
    class viewModel : ObservableObject
    {
        //ModbusClient modbusClient = new ModbusClient( );
        private string _info;

        public Driver Altivar { get; set; } = new Driver( );
       
        private int [] read { get; set; } = { 0, 0, 0 };
        public string info { get=>_info; set { _info = value; OnPropertyChanged() ; } }
        public viewModel()
        {
            //modbusClient.SerialPort = "COM2";
            //modbusClient.UnitIdentifier = 1;
            //modbusClient.Baudrate = 9600;
            //modbusClient.Parity = System.IO.Ports.Parity.None;
            //modbusClient.StopBits = System.IO.Ports.StopBits.One;
            //modbusClient.ConnectionTimeout = 100;
            //Altivar.Status = "";
        }
        public void Run()
        {

            //while ( true )
            //{
            //    if ( modbusClient.Connected )
            //    {
            //        try
            //        {
            //            Altivar.Moving( Altivar.StatusWord );
            //            Altivar.ToQuickStop( );
            //            Altivar.ToReset( );
            //            Altivar.ToStart( Altivar.StatusWord );
            //            Altivar.ToStop( );

            //            read = modbusClient.ReadHoldingRegisters( 1, 1 );
            //            modbusClient.WriteSingleRegister( 0, Altivar.ControlWord );
            //            Altivar.StatusWord = read [ 0 ];
            //            Altivar.Status = "";
            //            info = "Norm";
            //        }
            //        catch ( Exception ex )
            //        {
            //            //modbusClient.Disconnect( );
            //            info = ex.Message;

            //        }
            //    }
            //    modbusClient.Connect( );
            //}
        }
    }
}
