namespace Parallel.Shared.DataTransferObjects
{
    public class SMobileConfig
    {
        public uint MobileParamsSet1 { get; set; } = uint.MaxValue;
        public ushort UwbParams { get; set; } = ushort.MaxValue;
        public ushort HelloVoltageParams { get; set; } = ushort.MaxValue;
        public ushort HwParams { get; set; } = ushort.MaxValue;
        public byte SwParams { get; set; } = byte.MaxValue;
        public byte AlarmParams { get; set; } = byte.MaxValue;
        public byte ButtonClosePushCount { get; set; } = byte.MaxValue;
        public byte RfChannel { get; set; } = byte.MaxValue;
        public byte SafeZoneRfChannel { get; set; } = byte.MaxValue;
        public byte AssumeImmobilePeriodSec { get; set; } = byte.MaxValue;
        public ushort MotionAlarmPeriod { get; set; } = ushort.MaxValue;
        public ushort TiltAlarmPeriod { get; set; } = ushort.MaxValue;
        public byte AccAlarmMaxMsgCnt { get; set; } = byte.MaxValue;
        public byte AccAlarmMaxWarnCnt { get; set; } = byte.MaxValue;
        public byte TemperAlarmMaxMsgCnt { get; set; } = byte.MaxValue;
        public byte ProximityAlarmMaxMsgCnt { get; set; } = byte.MaxValue;
        public byte ProximityAlarmPeriod { get; set; } = byte.MaxValue;
        public byte SleepPeriodNormal100Ms { get; set; } = byte.MaxValue;
        public byte SleepPeriodFast100Ms { get; set; } = byte.MaxValue;
        public byte SleepPeriodWarn100Ms { get; set; } = byte.MaxValue;
        public byte SleepPeriodLongSleepSec { get; set; } = byte.MaxValue;
        public byte SleepPeriodStandbyMin { get; set; } = byte.MaxValue;
        public byte MobileHelloPeriod100Ms { get; set; } = byte.MaxValue;
        public byte MobileHakoNormalPeriod100Ms { get; set; } = byte.MaxValue;
        public ushort MobileHakoLongPeriod100Ms { get; set; } = ushort.MaxValue;
        public byte MobileHako2FastPeriod100Ms { get; set; } = byte.MaxValue;
        public byte MobileHako2NormalPeriod100Ms { get; set; } = byte.MaxValue;
        public ushort MobileVoltagePeriodSec { get; set; } = ushort.MaxValue;
        public ushort MobileVoltageMeasurePeriodSec { get; set; } = ushort.MaxValue;
        public byte MobileOnChipTempPeriodSec { get; set; } = byte.MaxValue;
        public byte MobileChargeNormalPeriodMin { get; set; } = byte.MaxValue;
        public byte MobileChargeOnChargePeriodSec { get; set; } = byte.MaxValue;
        public byte MobileRfInitFromScratchMin { get; set; } = byte.MaxValue;
        public byte PeriodWakeTimerCalibrateMin { get; set; } = byte.MaxValue;
        public byte MobileHelloPeriodPostAlarmSec { get; set; } = byte.MaxValue;
        public ushort MobilePower5PeriodSec { get; set; } = ushort.MaxValue;
        public byte PeriodPostAlarmHoldSec { get; set; } = byte.MaxValue;
        public byte AdditionalParams { get; set; } = byte.MaxValue;
        public ushort PeriodBreakFromFastAwakeSec { get; set; } = ushort.MaxValue;
    }
}