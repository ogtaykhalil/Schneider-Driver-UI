using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace SchneiderVFDUI.Model
{
    enum VFDControl
    {
        disableVoltage = 0x0,
        quickStop = 0x2,
        shotDown = 0x6,
        switchOn = 0x7,
        faultReset = 0x80,
        forward = 0xf,
        reverse=0x80f
    }
    enum VFDStatus
    {
        reverseOperationEnabled = -32713,
        fault = 0x8,
        quickStopActive = 0x17,
        switchOnDisabled = 0x50,
        readyToSwitchOn = 0x31,
        switchedOn = 0x33,
        forwardOperationEnabled = 0x37
    }
    class Driver : ObservableObject
    {
        public ICommand StartCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand QuickStopCommand { get; set; }
        public ICommand ForwardCommand { get; set; }
        public ICommand ReverseCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        #region Private Field
        private string _name;
        private string _status;
        private int _controlWord;
        private int _statusWord;
        private int _speedReference;
        private int _frequenceReference;
        private int _risingRamp;
        private int _fallingRamp;
        private int _quickStopRamp;
        private int _getCurrentSpeed;
        private int _getCurrentFrequence;
        private int _getVoltage;
        private int _getCurrent;
        private int _modbusAddress;
        private bool _start;
        private bool _stop;
        private bool _quickStop;
        private bool _reset;
        private bool _forward;
        private bool _reverse;
        #endregion
        public Driver()
        {
            StartCommand = new RelayCommand( StartExecuted, CanExecuted );
            StopCommand = new RelayCommand( StopExecuted, CanExecuted );
            QuickStopCommand = new RelayCommand( QuickStopExecuted, CanExecuted );
            ForwardCommand = new RelayCommand( ForwardExecuted, CanExecuted );
            ReverseCommand = new RelayCommand( ReverseExecuted, CanExecuted );
            ResetCommand = new RelayCommand( ResetExecuted, CanExecuted );
        }
        #region Property Field
        public string Name { get => _name; set { _name = value; OnPropertyChanged( ); } }
        public string Status { get => _status = Convert.ToString(( VFDStatus) StatusWord ); set { _status = value; OnPropertyChanged( ); } }
        public int ControlWord { get => _controlWord; set { _controlWord = value; OnPropertyChanged( ); } }
        public int StatusWord { get => _statusWord; set { _statusWord = value; OnPropertyChanged( ); } }
        public int SpeedReference { get => _speedReference; set { _speedReference = value;  OnPropertyChanged( );  } }
        public int FrequenceReference { get => _frequenceReference; set { _frequenceReference = value;  OnPropertyChanged( ); } }
        public int RisingRamp { get => _risingRamp; set { _risingRamp = value; OnPropertyChanged( ); } }
        public int FallingRamp { get => _fallingRamp; set { _fallingRamp = value; OnPropertyChanged( ); } }
        public int QuickStopRamp { get => _quickStopRamp; set { _quickStopRamp = value; OnPropertyChanged( ); } }
        public int GetCurrentSpeed { get => _getCurrentSpeed; set { _getCurrentSpeed = value; OnPropertyChanged( ); } }
        public int GetCurrentFrequence { get => _getCurrentFrequence; set { _getCurrentFrequence = value; OnPropertyChanged( ); } }
        public int GetVoltage { get => _getVoltage; set { _getVoltage = value; OnPropertyChanged( ); } }
        public int GetCurrent { get => _getCurrent; set { _getCurrent = value; OnPropertyChanged( ); } }
        public int ModbusAddress { get => _modbusAddress; set { _modbusAddress = value; OnPropertyChanged( ); } }
        //public bool Start { get => _start; set { _start = value; OnPropertyChanged( ); } }
        //public bool Stop { get => _stop; set { _stop = value; OnPropertyChanged( ); } }
        //public bool QuickStop { get => _quickStop; set { _quickStop = value; OnPropertyChanged( ); } }
        //public bool Reset { get => _reset; set { _reset = value; OnPropertyChanged( ); } }
        //public bool Forward { get => _forward; set { _forward = value; OnPropertyChanged( ); } }
        //public bool Reverse { get => _reverse; set { _reverse = value; OnPropertyChanged( ); } }
        #endregion
        #region Method Field
        private bool CanExecuted( object parametr ) => true;
        private void StartExecuted( object parametr ) => _start = true;
        private void StopExecuted( object parametr ) => _stop = true;
        private void QuickStopExecuted( object parametr ) => _quickStop = true;
        private void ResetExecuted( object parametr ) => _reset = true;
        private void ForwardExecuted( object parametr ) => _forward = true;
        private void ReverseExecuted( object parametr ) => _reverse = true;
        public void ToStart( int status )
        {
            if ( _start )
            {
                VFDStatus c = ( VFDStatus ) status;
                switch ( c )
                {
                    case VFDStatus.switchOnDisabled & ( VFDStatus ) 0xff:
                        ControlWord = ( int ) VFDControl.shotDown;
                        goto case VFDStatus.readyToSwitchOn & ( VFDStatus ) 0xff;
                    case VFDStatus.readyToSwitchOn & ( VFDStatus ) 0xff:
                        ControlWord = ( int ) VFDControl.switchOn;
                        break;
                }
                _start = false;
            }
        }
        public void ToStop()
        {
            if ( _stop )
            {
                if ( StatusWord != (int) ( VFDStatus.readyToSwitchOn & ( VFDStatus ) 0xff ) )
                    ControlWord = ( int ) VFDControl.shotDown;
                _stop = false;
            }
        }
        public void ToQuickStop()
        {
            if ( _quickStop )
            {
                if ( StatusWord != (int)( VFDStatus.quickStopActive & ( VFDStatus ) 0xff ) )
                    ControlWord = ( int ) VFDControl.quickStop;
                _quickStop = false;
            }
        }
        public void Moving( int status )
        {
            if ( _forward )
            {
                VFDStatus c = ( VFDStatus ) status;
                switch ( c )
                {
                    case VFDStatus.switchedOn & ( VFDStatus ) 0xff:
                    case VFDStatus.reverseOperationEnabled & ( VFDStatus ) 0xf0ff:
                        ControlWord = ( int ) VFDControl.forward;
                        break;
                }
                _forward = false;
            }
            else if ( _reverse )
            {
                VFDStatus c = ( VFDStatus ) status;
                switch ( c )
                {
                    case VFDStatus.switchedOn & ( VFDStatus ) 0xff:
                    case VFDStatus.forwardOperationEnabled & ( VFDStatus ) 0xff:
                        ControlWord = ( int ) VFDControl.reverse;
                        break;
                }
                _reverse = false;
            }
        }
        public void ToReset()
        {
            if ( _reset )
            {
                if ( StatusWord == (int) ( VFDStatus.fault & ( VFDStatus ) 0xf ) )
                    ControlWord = ( int ) VFDControl.faultReset;
                _reset = false;
            }
        }
        #endregion
    }
}
