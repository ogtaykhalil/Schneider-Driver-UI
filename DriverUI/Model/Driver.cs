using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace DriverUI.Model
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
    enum ErrorCode
    {
        noFault = 0,
        calibrationError= 1,
        controlEepromFault = 2,
        incorrectConfiguration = 3,
        invalidConfiguration = 4,
        modbusCommunicationFault = 5,
        incorrectCommunicationLinkFault = 6,
        communicationNetworkFault = 7,
        externalFault_LI_Bit = 8,
        overCurrent = 9,
        precharge = 10,
        speedFeedbackLoss = 11,
        driveOverheat = 16,
        motorOverload = 17,
        overBreaking = 18,
        mainsOverVoltage = 19,
        _1_OutputPhaseLoss = 20,
        inputPhaseLoss = 21,
        underVoltage = 22,
        motorShortCircuit = 23,
        overSpeed = 24,
        autoTunningFault = 25,
        ratingError = 26,
        powerCalibrationFault = 27,
        internalSerialLink = 28,
        internalMfgFault = 29,
        powerEepromFault = 30,
        impedantShortCircuit = 31,
        groundShortCircuit = 32,
        _3_OutputPhaseLoss = 33,
        CAN_CommunicationFault = 34,
        brakeControlFault = 35,
        externalCommunicationFault = 38,
        brakeFeedbackFault = 41,
        PC_CommunicationFault = 42,
        torque_currentLimitFault = 44,
        HMI_CommunicationFault = 45,
        powerRemovalFault = 46,
        LI6_PTC_ProbeFault = 49,
        PTC_Fault = 50,
        internal_I_MeasureFault = 51,
        internalMainsCircuitFault = 52,
        internalThermicSensorFault = 53,
        IGBT_OverHeat = 54,
        IGBT_ShortCircuit = 55,
        motorShortCircuit_2 = 56,
        torqueTimeOutFault = 57,
        outputContactStuckFault = 58,
        outputContactOpenFault = 59,
        analogInput2_Fault = 61,
        inputContactorFault = 64,
        differencial_I_Fault = 66,
        IgbtDesaturationFault = 67,
        internalOptionFault = 68,
        internalCPUFault = 69,
        analogInput3_4_20mA_LossFault = 71,
        cardsPairingFault = 73,
        loadFault = 76,
        badConfiguration = 77,
        ch_Sw_Fault = 99,
        ProcUnderLoadFault = 100,
        proc_OverloadFault = 101,
        angleError = 105,
        safetyFault = 107,
        FB_Fault = 108,
        FB_StopFault = 109
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
        private string _errorStatus;
        private string _controlStatus;
        private int _errorCode;
        private int _controlWord;
        private int _statusWord;
        private double _speedReference=0;
        private double _frequencyReference;
        private int _risingRamp;
        private int _fallingRamp;
        private int _quickStopRamp;
        private int _getCurrentSpeed;
        private int _getCurrentFrequency;
        private int _getVoltage;
        private int _getCurrent=100;
        private int _modBusAddress;
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
        public string Status { get => Convert.ToString(( VFDStatus) StatusWord ); set { _status = value; OnPropertyChanged( ); } }
        public string ErrorStatus { get => Convert.ToString((ErrorCode) ErrorCode); set { _errorStatus = value; OnPropertyChanged( ); } }
        public string ControlStatus { get => Convert.ToString( ( VFDControl ) ControlWord ); set { _controlStatus = value; OnPropertyChanged( ); } }
        public int ErrorCode { get => _errorCode; set { _errorCode = value;OnPropertyChanged( ); } }
        public int ControlWord { get => _controlWord; set { _controlWord = value; OnPropertyChanged( ); } }
        public int StatusWord { get => _statusWord; set { _statusWord = value; OnPropertyChanged( ); } }
        public double SpeedReference 
        { 
            get 
            {
                if ( _speedReference < 0 )
                    _speedReference = 0;
                else if ( _speedReference > 3500 )
                    _speedReference = 3500;
                return Math.Abs( Math.Round( _speedReference )); 
            } 
            set { _speedReference = value;  OnPropertyChanged( );  } }
        public double FrequenceReference 
        { 
            get
            {
                if ( _frequencyReference < 0 )
                    _frequencyReference = 0;
                else if ( _frequencyReference > 50 )
                    _frequencyReference = 50;
                return Math.Abs( Math.Round( _frequencyReference *10 ));
            }
            set { _frequencyReference = value;  OnPropertyChanged( ); } }
        //public double SpeedReference1 { get => Math.Round(_speedReference1); set { _speedReference1 = value; OnPropertyChanged(); } }
        //public double FrequenceReference1 { get => _frequenceReference1; set { _frequenceReference1 = value; OnPropertyChanged(); } }
        public int RisingRamp { get => _risingRamp; set { _risingRamp = value; OnPropertyChanged( ); } }
        public int FallingRamp { get => _fallingRamp; set { _fallingRamp = value; OnPropertyChanged( ); } }
        public int QuickStopRamp { get => _quickStopRamp; set { _quickStopRamp = value; OnPropertyChanged( ); } }
        public int GetCurrentSpeed { get => _getCurrentSpeed; set { _getCurrentSpeed = value; OnPropertyChanged( ); } }
        public int GetCurrentFrequence { get => _getCurrentFrequency; set { _getCurrentFrequency = value; OnPropertyChanged( ); } }
        public int GetVoltage { get => _getVoltage; set { _getVoltage = value; OnPropertyChanged( ); } }
        public int GetCurrent { get => _getCurrent; set { _getCurrent = value; OnPropertyChanged( ); } }
        public int ModbusAddress { get => _modBusAddress; set { _modBusAddress = value; OnPropertyChanged( ); } }
        //public bool Start { get => _start; set { _start = value; OnPropertyChanged( ); } }
        //public bool Stop { get => _stop; set { _stop = value; OnPropertyChanged( ); } }
        //public bool QuickStop { get => _quickStop; set { _quickStop = value; OnPropertyChanged( ); } }
        //public bool Reset { get => _reset; set { _reset = value; OnPropertyChanged( ); } }
        //public bool Forward { get => _forward; set { _forward = value; OnPropertyChanged( ); } }
        //public bool Reverse { get => _reverse; set { _reverse = value; OnPropertyChanged( ); } }
        #endregion
        #region Method Field
        public string Ok { get; init; }
        private bool CanExecuted(object parametr) => true;
        private void StartExecuted( object parametr ) => _start = true;
        private void StopExecuted( object parametr ) => _stop = true;
        private void QuickStopExecuted( object parametr ) => _quickStop = true;
        private void ResetExecuted( object parametr ) => _reset = true;
        private void ForwardExecuted( object parametr ) => _forward = true;
        private void ReverseExecuted( object parametr ) => _reverse = true;
        public void GetFault()
        {
            ErrorCode e = ( ErrorCode ) _errorCode;
            switch ( e )
            {
                case Model.ErrorCode.noFault:
                    ErrorStatus = "sürücüdə xəta mövcud deyildir";
                    break;
                case Model.ErrorCode.calibrationError:
                    ErrorStatus = "kalibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.controlEepromFault:
                    ErrorStatus = "kontrol EEPROM xətası mövcuddur";
                    break;
                case Model.ErrorCode.incorrectConfiguration:
                    ErrorStatus = "düzgün olmayan konfigurasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.invalidConfiguration:
                    ErrorStatus = "xətalı configurasiya mövcuddur";
                    break;
                case Model.ErrorCode.modbusCommunicationFault:
                    ErrorStatus = "modbus xəbərləşməsi xətası mövcuddur";
                    break;
                case Model.ErrorCode.incorrectCommunicationLinkFault:
                    ErrorStatus = "düzgün olmayan əlaqələndirici linkin xətası mövcuddur";
                    break;
                case Model.ErrorCode.communicationNetworkFault:
                    ErrorStatus = "əlaqələndirici networkun xətası mövcuddur";
                    break;
                case Model.ErrorCode.externalFault_LI_Bit:
                    ErrorStatus = "xarici Lİ/bit xətası mövcuddur";
                    break;
                case Model.ErrorCode.overCurrent:
                    ErrorStatus = "yuxarı axım xətası mövcuddur";
                    break;
                case Model.ErrorCode.precharge:
                    ErrorStatus = "precharge xətası mövcuddur";
                    break;
                case Model.ErrorCode.speedFeedbackLoss:
                    ErrorStatus = "sürətin geri dönüşü itdi xətası mövcuddur";
                    break;
                case Model.ErrorCode.driveOverheat:
                    ErrorStatus = "sürücünün temperaturu yüksəkdir xətası mövcuddur";
                    break;
                case Model.ErrorCode.motorOverload:
                    ErrorStatus = "motor yüklənib xətası mövcuddur";
                    break;
                case Model.ErrorCode.overBreaking:
                    ErrorStatus = "over breaking xətası mövcuddur";
                    break;
                case Model.ErrorCode.mainsOverVoltage:
                    ErrorStatus = "qidalanma gərginliyi yüksəkdir xətası mövcuddur";
                    break;
                case Model.ErrorCode._1_OutputPhaseLoss:
                    ErrorStatus = "1-ci çıxış xəttinin fazası itdi xətası mövcuddur";
                    break;
                case Model.ErrorCode.inputPhaseLoss:
                    ErrorStatus = "giriş fazası itdi xətası mövcuddur";
                    break;
                case Model.ErrorCode.underVoltage:
                    ErrorStatus = "aşağı gərginlik xətası mövcuddur";
                    break;
                case Model.ErrorCode.motorShortCircuit:
                    ErrorStatus = "motor qısa qapanma xətası mövcuddur";
                    break;
                case Model.ErrorCode.overSpeed:
                    ErrorStatus = "yüksək sürət xətası mövcuddur";
                    break;
                case Model.ErrorCode.autoTunningFault:
                    ErrorStatus = "auto tunning xətası mövcuddur";
                    break;
                case Model.ErrorCode.ratingError:
                    ErrorStatus = "rating xətası mövcuddur";
                    break;
                case Model.ErrorCode.powerCalibrationFault:
                    ErrorStatus = "güc kalibirasiyası xətası mövcuddur";
                    break;
                case Model.ErrorCode.internalSerialLink:
                    ErrorStatus = "daxili serial link xətası mövcuddur";
                    break;
                case Model.ErrorCode.internalMfgFault:
                    ErrorStatus = "daxili Mfg xətası mövcuddur";
                    break;
                case Model.ErrorCode.powerEepromFault:
                    ErrorStatus = "güc EEPROM xətası mövcuddur";
                    break;
                case Model.ErrorCode.impedantShortCircuit:
                    ErrorStatus = "impedant qısa qapanma xətası mövcuddur";
                    break;
                case Model.ErrorCode.groundShortCircuit:
                    ErrorStatus = "torpaqlama qısa qapanma xətası mövcuddur";
                    break;
                case Model.ErrorCode._3_OutputPhaseLoss:
                    ErrorStatus = "3-cü çıxış xəttinin fazası itdi xətası mövcuddur";
                    break;
                case Model.ErrorCode.CAN_CommunicationFault:
                    ErrorStatus = "CAN xəbərləşmə xətası mövcuddur";
                    break;
                case Model.ErrorCode.brakeControlFault:
                    ErrorStatus = "tormuzlama nəzarəti xətası mövcuddur";
                    break;
                case Model.ErrorCode.externalCommunicationFault:
                    ErrorStatus = "xarici xəbərləşmə xətası mövcuddur";
                    break;
                case Model.ErrorCode.brakeFeedbackFault:
                    ErrorStatus = "tormuz geri dönüş xətası mövcuddur";
                    break;
                case Model.ErrorCode.PC_CommunicationFault:
                    ErrorStatus = "PC xəbərləşmə xətası mövcuddur";
                    break;
                case Model.ErrorCode.torque_currentLimitFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.HMI_CommunicationFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.powerRemovalFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.LI6_PTC_ProbeFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.PTC_Fault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.internal_I_MeasureFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.internalMainsCircuitFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.internalThermicSensorFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.IGBT_OverHeat:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.IGBT_ShortCircuit:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.motorShortCircuit_2:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.torqueTimeOutFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.outputContactStuckFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.outputContactOpenFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.analogInput2_Fault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.inputContactorFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.differencial_I_Fault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.IgbtDesaturationFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.internalOptionFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.internalCPUFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.analogInput3_4_20mA_LossFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.cardsPairingFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.loadFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.badConfiguration:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.ch_Sw_Fault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.ProcUnderLoadFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.proc_OverloadFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.angleError:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.safetyFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.FB_Fault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                case Model.ErrorCode.FB_StopFault:
                    ErrorStatus = "calibrasiya xətası mövcuddur";
                    break;
                default:
                    ErrorStatus = "xəta kodu təyin edilməyib";
                    break;
            }
        }
        public void ToStart()
        {
            if ( _start )
            {
                VFDStatus c = ( VFDStatus ) _statusWord;
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
        public void Moving()
        {
            if ( _forward )
            {
                VFDStatus c = ( VFDStatus ) _statusWord;
                switch ( c )
                {
                    case VFDStatus.switchedOn & ( VFDStatus ) 0xff:
                    case VFDStatus.reverseOperationEnabled & ( VFDStatus )(-3841):
                        ControlWord = ( int ) VFDControl.forward;
                        break;
                }
                _forward = false;
            }
            else if ( _reverse )
            {
                VFDStatus c = ( VFDStatus ) _statusWord;
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
            if (_reset)
            {
                if (StatusWord == (int)(VFDStatus.fault & (VFDStatus)0xf))
                    ControlWord = (int)VFDControl.faultReset;
                _reset = false;
            }
        }
        #endregion
    }
}
